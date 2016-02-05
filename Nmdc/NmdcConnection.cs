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
    public class NmdcConnection : IDisposable
    {
        private const byte CommandStopByte = NmdcUtilities.VerticalBarByte;

        public event EventHandler<NmdcCommandTransferredArgs> CommandReceived;
        public event EventHandler<NmdcCommandTransferredArgs> CommandSent;
        
        private TcpClient _tcpClient = new TcpClient();
        private NetworkStream _networkStream;
        private bool _disposed;

        private byte[] _receivedBytesRemainder;
        private byte[] _receiveBuffer;

        #region Constructors

        public NmdcConnection(string host, int port)
        {
            if (string.IsNullOrEmpty(host))
                throw new ArgumentException("Host is null.", nameof(host));

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException("Port is out of range.", nameof(port));
        }

        public NmdcConnection(IPAddress address, int port)
        {
            if (address == null)
                throw new ArgumentException("Address is null.", nameof(address));

            if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException("Port is out of range.", nameof(port));
        }

        #endregion

        #region Properties

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
        /// Gets a value that indicates whether a <see cref="NmdcConnection"/> is connected to a remote
        /// host as of the last <see cref="SendAsync(Encoding, IEnumerable{NmdcCommand}, CancellationToken)"/>
        /// or <see cref="ReceiveAsync(Encoding, CancellationToken)"/> operation.
        /// </summary>
        public bool IsConnected
        {
            get { return _tcpClient?.Client?.Connected == true; }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="NmdcConnection"/> supports receiving data.
        /// </summary>
        public bool CanReceive
        {
            get { return _networkStream?.CanRead == true; }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="NmdcConnection"/> supports sending data.
        /// </summary>
        public bool CanSend
        {
            get { return _networkStream?.CanWrite == true; }
        }

        #endregion

        public async Task ConnectAsync(string host, int port)
        {
            await _tcpClient.ConnectAsync(host, port);
            _networkStream = _tcpClient.GetStream();
            _receiveBuffer = new byte[ReceiveBufferSize];
        }

        public void Disconnect()
        {
            _tcpClient.Close();
        }

        public async Task<IEnumerable<NmdcCommand>> ReceiveAsync(Encoding encoding, CancellationToken cancellationToken)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding), "Encoding of data to receive is null.");

            // [0] Check the connection state.

            if (!IsConnected)
                throw new InvalidOperationException("Attempt to receive data via dead connection.");

            if (!CanReceive)
                throw new InvalidOperationException("Data cannot be received via this connection.");

            // [1] Receive bytes via connection until the command stop byte (|) is encountered.

            if (cancellationToken.IsCancellationRequested)
                return Enumerable.Empty<NmdcCommand>();

            byte[] commandsBytes = null;

            using (var memoryStream = new MemoryStream())
            {
                if (_receivedBytesRemainder != null && _receivedBytesRemainder.Length > 0)
                    await memoryStream.WriteAsync(_receivedBytesRemainder, 0, _receivedBytesRemainder.Length, cancellationToken);

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return Enumerable.Empty<NmdcCommand>();

                    var receivedBytesCount = await _networkStream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length, cancellationToken);
                    if (receivedBytesCount <= 0)
                        continue;

                    await memoryStream.WriteAsync(_receiveBuffer, 0, receivedBytesCount, cancellationToken);

                    if (Array.IndexOf(_receiveBuffer, CommandStopByte, 0, receivedBytesCount) >= 0)
                        break;
                }

                commandsBytes = memoryStream.ToArray();
            }

            // [2] Parse commands presented in the received bytes.

            if (cancellationToken.IsCancellationRequested)
                return Enumerable.Empty<NmdcCommand>();
            
            var result = new List<NmdcCommand>();
            var offset = 0;
            
            while (offset < commandsBytes.Length)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Enumerable.Empty<NmdcCommand>();

                var commandStopBytePosition = Array.IndexOf(commandsBytes, CommandStopByte, offset);
                if (commandStopBytePosition < 0)
                    break;

                var message = encoding.GetString(commandsBytes, offset, commandStopBytePosition - offset);
                var command = NmdcCommandParser.Parse(message);

                result.Add(command);
                OnCommandReceived(message, command);

                offset = commandStopBytePosition + 1;
            }

            // [3] Save the bytes remained after parsing since they must be used as start of a command on next receive operation.

            if (cancellationToken.IsCancellationRequested)
                return Enumerable.Empty<NmdcCommand>();

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

        public async Task SendAsync(Encoding encoding, IEnumerable<NmdcCommand> commands, CancellationToken cancellationToken)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding), "Encoding of data to send is null.");

            if (commands == null)
                throw new ArgumentNullException(nameof(commands), "Commands to send are not specified.");

            // [0] Check the connection state.

            if (!IsConnected)
                throw new InvalidOperationException("Attempt to send data via dead connection.");

            if (!CanSend)
                throw new InvalidOperationException("Data cannot be sent via this connection.");

            //

            foreach (var command in commands)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var message = command.ToString();
                var commandBytes = encoding.GetBytes(message);

                var bytesToSend = new byte[commandBytes.Length + 1];
                Buffer.BlockCopy(commandBytes, 0, bytesToSend, 0, commandBytes.Length);
                Buffer.SetByte(bytesToSend, commandBytes.Length, CommandStopByte);

                await _networkStream.WriteAsync(bytesToSend, 0, bytesToSend.Length, cancellationToken);

                OnCommandSent(message, command);
            }
        }

        private void OnCommandReceived(string message, NmdcCommand command)
        {
            CommandReceived?.Invoke(this, new NmdcCommandTransferredArgs(DateTime.Now, message, command));
        }

        private void OnCommandSent(string message, NmdcCommand command)
        {
            CommandSent?.Invoke(this, new NmdcCommandTransferredArgs(DateTime.Now, message, command));
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_tcpClient != null)
                    _tcpClient.Dispose();

                if (_networkStream != null)
                    _networkStream.Dispose();
            }
            
            _networkStream = null;
            _tcpClient = null;

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
