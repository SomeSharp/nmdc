using System;

namespace SomeSharp.Nmdc
{
    /// <summary>
    /// Represents the class that contains data for <see cref="NmdcHubConnection.CommandSent"/> or
    /// <see cref="NmdcHubConnection.CommandReceived"/> events.
    /// </summary>
    public sealed class NmdcCommandTransferredArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NmdcCommandTransferredArgs"/> class to the specified time,
        /// message and command.
        /// </summary>
        /// <param name="time">The time when the command was received or sent.</param>
        /// <param name="message">In case of the <see cref="NmdcHubConnection.CommandReceived"/> event - original NMDC-message
        /// the <see cref="Command"/> was constructed from; in case of the <see cref="NmdcHubConnection.CommandSent"/> event -
        /// NMDC-message that represents the <see cref="Command"/>.</param>
        /// <param name="command">The command was received or sent via an instance of the <see cref="NmdcHubConnection"/> class.</param>
        public NmdcCommandTransferredArgs(DateTime time, string message, NmdcCommand command)
        {
            Time = time;
            Message = message;
            Command = command;
        }

        /// <summary>
        /// Gets the time when the <see cref="Command"/> was received or sent.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// In case of the <see cref="NmdcHubConnection.CommandReceived"/> event gets the original NMDC-message the <see cref="Command"/>
        /// was constructed from; in case of the <see cref="NmdcHubConnection.CommandSent"/> event gets NMDC-message that
        /// represents the <see cref="Command"/>.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the command was received or sent via an instance of the <see cref="NmdcHubConnection"/> class.
        /// </summary>
        public NmdcCommand Command { get; private set; }
    }
}
