using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SomeSharp.Nmdc
{
    public class NmdcConnection : IDisposable
    {
        private const byte CommandStopByte = NmdcUtilities.VerticalBarByte;

        public event EventHandler<NmdcCommandTransferredArgs> CommandRecieved;
        public event EventHandler<NmdcCommandTransferredArgs> CommandSent;
        
        private TcpClient _tcpClient = new TcpClient();
        private NetworkStream _networkStream;
        private bool _disposed;

        private byte[] _recievedBytesRemainder;
        private byte[] _recieveBuffer;
        
        public int ReceiveBufferSize
        {
            get { return _tcpClient.ReceiveBufferSize; }
            set
            {
                _tcpClient.ReceiveBufferSize = value;
                _recieveBuffer = new byte[value];
            }
        }

        public int SendBufferSize
        {
            get { return _tcpClient.SendBufferSize; }
            set { _tcpClient.SendBufferSize = value; }
        }

        public bool IsConnected
        {
            get { return _tcpClient?.Client?.Connected == true; }
        }

        public bool CanRecieve
        {
            get { return _networkStream?.CanRead == true; }
        }

        public bool CanSend
        {
            get { return _networkStream?.CanWrite == true; }
        }

        public async Task ConnectAsync(string host, int port)
        {
            await _tcpClient.ConnectAsync(host, port);
            _networkStream = _tcpClient.GetStream();
            _recieveBuffer = new byte[ReceiveBufferSize];
        }

        public void Disconnect()
        {
            _tcpClient.Close();
        }

        public async Task<IEnumerable<NmdcCommand>> RecieveAsync(Encoding encoding, CancellationToken cancellationToken)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding), "Encoding of data to be recieved is not specified.");
            }

            // [0] Check the connection state.

            if (!IsConnected)
            {
                throw new InvalidOperationException("Attempt to recieve data via dead connection.");
            }

            if (!CanRecieve)
            {
                throw new InvalidOperationException("Data cannot be recieved via this connection.");
            }

            // [1] Recieve bytes via connection until the command stop byte (|) is encountered.

            if (cancellationToken.IsCancellationRequested)
            {
                return Enumerable.Empty<NmdcCommand>();
            }

            var buffersList = new List<byte[]>();
            if (_recievedBytesRemainder != null && _recievedBytesRemainder.Length > 0)
            {
                buffersList.Add(_recievedBytesRemainder);
            }

            while (!buffersList.Any() || Array.IndexOf(buffersList.Last(), CommandStopByte) < 0)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Enumerable.Empty<NmdcCommand>();
                }

                var recievedBytesCount = await _networkStream.ReadAsync(_recieveBuffer, 0, _recieveBuffer.Length, cancellationToken);
                if (recievedBytesCount <= 0)
                {
                    continue;
                }

                var recievedBytes = new byte[recievedBytesCount];
                Buffer.BlockCopy(_recieveBuffer, 0, recievedBytes, 0, recievedBytes.Length);

                buffersList.Add(recievedBytes);

                //

                if (Array.IndexOf(buffersList.Last(), (byte)26) >= 0)
                    break;
            }

            // [2] Parse commands presented in the recieved bytes.

            if (cancellationToken.IsCancellationRequested)
            {
                return Enumerable.Empty<NmdcCommand>();
            }

            var commandsBytes = buffersList.SelectMany(b => b).ToArray();
            var result = new List<NmdcCommand>();
            var offset = 0;
            
            while (offset < commandsBytes.Length)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Enumerable.Empty<NmdcCommand>();
                }

                var commandStopBytePosition = Array.IndexOf(commandsBytes, CommandStopByte, offset);
                if (commandStopBytePosition < 0)
                {
                    break;
                }

                var message = encoding.GetString(commandsBytes, offset, commandStopBytePosition - offset);
                var command = NmdcCommandParser.Parse(message);

                result.Add(command);

                var commandRecievedHandler = CommandRecieved;
                if (commandRecievedHandler != null)
                {
                    commandRecievedHandler(this, new NmdcCommandTransferredArgs(DateTime.Now, message, command));
                }

                offset = commandStopBytePosition + 1;
            }

            // [3] Save the bytes remained after parsing since they must be used as start of a command on next recieve operation.

            if (cancellationToken.IsCancellationRequested)
            {
                return Enumerable.Empty<NmdcCommand>();
            }

            _recievedBytesRemainder = null;
            var recievedDataTailLength = commandsBytes.Length - offset;
            if (recievedDataTailLength > 0)
            {
                _recievedBytesRemainder = new byte[recievedDataTailLength];
                Buffer.BlockCopy(commandsBytes, offset, _recievedBytesRemainder, 0, recievedDataTailLength);
            }

            //

            return result;
        }

        public async Task SendAsync(Encoding encoding, IEnumerable<NmdcCommand> commands, CancellationToken cancellationToken)
        {
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding), "Encoding of data to send is not specified.");

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

                if (cancellationToken.IsCancellationRequested)
                    return;

                var commandSentHandler = CommandSent;
                if (commandSentHandler != null)
                    commandSentHandler(this, new NmdcCommandTransferredArgs(DateTime.Now, message, command));
            }
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
