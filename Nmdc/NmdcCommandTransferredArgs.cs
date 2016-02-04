using System;

namespace SomeSharp.Nmdc
{
    /// <summary>
    /// Represents the class that contains data for <see cref="NmdcConnection.CommandSent"/> or <see cref="NmdcConnection.CommandRecieved"/> events.
    /// </summary>
    public sealed class NmdcCommandTransferredArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NmdcCommandTransferredArgs"/> class to the specified time,
        /// message and command.
        /// </summary>
        /// <param name="time">The time when the command was recieved or sent.</param>
        /// <param name="message">In case of the <see cref="NmdcConnection.CommandRecieved"/> event - original NMDC-message
        /// the <see cref="Command"/> was constructed from; in case of the <see cref="NmdcConnection.CommandSent"/> event -
        /// NMDC-message that represents the <see cref="Command"/>.</param>
        /// <param name="command">The command was recieved or sent via an instance of the <see cref="NmdcConnection"/> class.</param>
        public NmdcCommandTransferredArgs(DateTime time, string message, NmdcCommand command)
        {
            Time = time;
            Message = message;
            Command = command;
        }

        /// <summary>
        /// Gets the time when the <see cref="Command"/> was recieved or sent.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// In case of the <see cref="NmdcConnection.CommandRecieved"/> event gets the original NMDC-message the <see cref="Command"/> was constructed from;
        /// in case of the <see cref="NmdcConnection.CommandSent"/> event gets NMDC-message that represents the <see cref="Command"/>.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the command was recieved or sent via an instance of the <see cref="NmdcConnection"/> class.
        /// </summary>
        public NmdcCommand Command { get; private set; }
    }
}
