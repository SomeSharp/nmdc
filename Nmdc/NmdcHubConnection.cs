using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeSharp.Nmdc
{
    /// <summary>
    /// Represents client-to-hub connection.
    /// </summary>
    public sealed class NmdcHubConnection : IDisposable
    {
        #region Events

        /// <summary>
        /// Occurs when an instance of the <see cref="NmdcHubConnection"/> class receives a command.
        /// </summary>
        public event EventHandler<NmdcCommandTransferredArgs> CommandReceived;

        /// <summary>
        /// Occurs when an instance of the <see cref="NmdcHubConnection"/> class sends a command.
        /// </summary>
        public event EventHandler<NmdcCommandTransferredArgs> CommandSent;

        #endregion

        #region Constants

        private const int DefaultPort = 411;

        #endregion

        #region Fields

        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private bool _disposed;
        private bool _connected;

        private byte[] _receivedBytesRemainder;
        private byte[] _receiveBuffer;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NmdcHubConnection"/> class to the specified host
        /// and port (default port number is 411).
        /// </summary>
        /// <param name="encoding">Encoding of hub's commands.</param>
        /// <param name="host">The DNS name of the remote host to connect to.</param>
        /// <param name="port">The port number of the remote host to connect to.</param>
        /// <remarks>The real connection is not established until the <see cref="ConnectAsync"/> method
        /// is executed.</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is null or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not between
        /// <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        public NmdcHubConnection(Encoding encoding, string host, int port = DefaultPort)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding), "Encoding is null.");

            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("Host is null or empty.", nameof(host));

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException("Port is out of range.", nameof(port));

            Host = host;
            Port = port;
            Encoding = encoding;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the port number of the remote host to connect to.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the DNS name of the remote host to connect to.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets the encoding of hub's commands.
        /// </summary>
        public Encoding Encoding { get; private set; }
        
        /// <summary>
        /// Gets or sets the size of the receive buffer.
        /// </summary>
        public int ReceiveBufferSize
        {
            get { return _tcpClient.ReceiveBufferSize; }
            set
            {
                _tcpClient.ReceiveBufferSize = value;
                _receiveBuffer = new byte[value];
            }
        }

        /// <summary>
        /// Gets or sets the size of the send buffer.
        /// </summary>
        public int SendBufferSize
        {
            get { return _tcpClient.SendBufferSize; }
            set { _tcpClient.SendBufferSize = value; }
        }

        /// <summary>
        /// Gets a value that indicates whether a <see cref="NmdcHubConnection"/> is connected to a remote computer.
        /// </summary>
        public bool IsConnected
        {
            get { return _connected && _tcpClient?.Client?.Connected == true; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Performs connection to the <see cref="Port"/> on the <see cref="Host"/> as an asynchronous operation.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ConnectAsync()
        {
            Close();

            _tcpClient = new TcpClient();

            while (!_connected)
            {
                try
                {
                    await _tcpClient.ConnectAsync(Host, Port);
                    _connected = true;
                }
                catch (SocketException socketException) when ((SocketError)socketException.ErrorCode == SocketError.TimedOut)
                {
                    // If connection cannot be established increment port number and try to connect again.
                    Port++;
                }
            }

            _networkStream = _tcpClient.GetStream();
            _receiveBuffer = new byte[ReceiveBufferSize];
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <remarks>Connection can be reopened again with the <see cref="ConnectAsync"/> method.</remarks>
        public void Close()
        {
            _tcpClient?.Close();

            _networkStream = null;
            _tcpClient = null;

            _connected = false;
        }

        /// <summary>
        /// Asynchronously receives a sequence of commands and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous receive operation. The value of
        /// the TResult parameter contains collection of received commands.</returns>
        /// <exception cref="InvalidOperationException">
        ///     <para>Attempt to receive data via dead connection.</para>
        ///     <para>-or-</para>
        ///     <para>Data cannot be received via this connection.</para>
        ///     <para>-or-</para>
        ///     <para>Underlying network stream is currently in use by a previous receive operation.</para>
        /// </exception>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
        public async Task<IEnumerable<NmdcCommand>> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NmdcHubConnection), "The connection has been disposed.");

            // [0] Check the connection state.

            if (!IsConnected)
                throw new InvalidOperationException("Attempt to receive data via dead connection.");

            if (_networkStream?.CanRead != true)
                throw new InvalidOperationException("Data cannot be received via this connection.");

            // [1] Receive bytes via connection until the command stop byte (|) is encountered.

            cancellationToken.ThrowIfCancellationRequested();

            byte[] commandsBytes = null;

            using (var memoryStream = new MemoryStream())
            {
                if (_receivedBytesRemainder != null && _receivedBytesRemainder.Length > 0)
                    await memoryStream.WriteAsync(_receivedBytesRemainder, 0, _receivedBytesRemainder.Length, cancellationToken);

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var receivedBytesCount = await _networkStream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length, cancellationToken);
                    if (receivedBytesCount <= 0)
                        continue;

                    await memoryStream.WriteAsync(_receiveBuffer, 0, receivedBytesCount, cancellationToken);

                    if (Array.IndexOf(_receiveBuffer, NmdcCommand.StopByte, 0, receivedBytesCount) >= 0)
                        break;
                }

                commandsBytes = memoryStream.ToArray();
            }

            // [2] Parse commands presented in the received bytes.

            cancellationToken.ThrowIfCancellationRequested();

            var result = new List<NmdcCommand>();
            var offset = 0;

            while (offset < commandsBytes.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var commandStopBytePosition = Array.IndexOf(commandsBytes, NmdcCommand.StopByte, offset);
                if (commandStopBytePosition < 0)
                    break;

                var message = Encoding.GetString(commandsBytes, offset, commandStopBytePosition - offset);
                var command = NmdcCommandParser.Parse(message);

                result.Add(command);
                OnCommandReceived(message, command);

                offset = commandStopBytePosition + 1;
            }

            // [3] Save the bytes remained after parsing since they must be used as start of a command on next receive operation.

            cancellationToken.ThrowIfCancellationRequested();

            _receivedBytesRemainder = null;
            var receivedDataTailLength = commandsBytes.Length - offset;
            if (receivedDataTailLength > 0)
            {
                _receivedBytesRemainder = new byte[receivedDataTailLength];
                Buffer.BlockCopy(commandsBytes, offset, _receivedBytesRemainder, 0, receivedDataTailLength);
            }

            //

            return result;
        }

        /// <summary>
        /// Asynchronously sends a sequence of commands and monitors cancellation requests.
        /// </summary>
        /// <param name="commands">Commands to send.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="InvalidOperationException">
        ///     <para>Attempt to send data via dead connection.</para>
        ///     <para>-or-</para>
        ///     <para>Data cannot be sent via this connection.</para>
        ///     <para>-or-</para>
        ///     <para>Underlying network stream is currently in use by a previous send operation.</para>
        /// </exception>
        /// <exception cref="ObjectDisposedException">The connection has been disposed.</exception>
        /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
        public async Task SendAsync(IEnumerable<NmdcCommand> commands, CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(NmdcHubConnection), "The connection has been disposed.");

            if (commands == null || !commands.Any())
                return;

            // [0] Check the connection state.

            if (!IsConnected)
                throw new InvalidOperationException("Attempt to send data via dead connection.");

            if (_networkStream?.CanWrite != true)
                throw new InvalidOperationException("Data cannot be sent via this connection.");

            // [1] Iterate through passed commands in order to send them.

            foreach (var command in commands)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var message = command.ToString();
                var commandBytes = Encoding.GetBytes(message);

                var bytesToSend = new byte[commandBytes.Length + 1];
                Buffer.BlockCopy(commandBytes, 0, bytesToSend, 0, commandBytes.Length);
                Buffer.SetByte(bytesToSend, commandBytes.Length, NmdcCommand.StopByte);

                await _networkStream.WriteAsync(bytesToSend, 0, bytesToSend.Length, cancellationToken);

                OnCommandSent(message, command);
            }
        }

        /// <summary>
        /// Raises the <see cref="CommandReceived"/> event with passed data.
        /// </summary>
        /// <param name="message">Original message the <paramref name="command"/> was constructed from.</param>
        /// <param name="command">Received command.</param>
        private void OnCommandReceived(string message, NmdcCommand command)
        {
            CommandReceived?.Invoke(this, new NmdcCommandTransferredArgs(DateTime.Now, message, command));
        }

        /// <summary>
        /// Raises the <see cref="CommandSent"/> event with passed data.
        /// </summary>
        /// <param name="message">NMDC-message that represents the <paramref name="command"/>.</param>
        /// <param name="command">Sent command.</param>
        private void OnCommandSent(string message, NmdcCommand command)
        {
            CommandSent?.Invoke(this, new NmdcCommandTransferredArgs(DateTime.Now, message, command));
        }

        #endregion

        #region IDisposable Support

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                Close();

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
