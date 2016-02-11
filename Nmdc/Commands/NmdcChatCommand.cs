using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcChatCommand : NmdcCommand
    {
        #region Constants

        private const string MessageGroupName = "message";
        private static readonly Regex ParseRegex = new Regex(
            $@"^\<{NmdcCommandParser.NickGroup}\> (?<{MessageGroupName}>.*)$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcChatCommand(string nick, string message)
        {
            Nick = nick;
            Message = message;
        }

        #endregion

        #region Properties

        public string Nick { get; private set; }

        public string Message { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"<{Nick ?? string.Empty}> {Message ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static NmdcChatCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var nick = groups[NmdcCommandParser.NickGroupName].Value;
            var chatMessage = groups[MessageGroupName].Value;

            return new NmdcChatCommand(nick, chatMessage);
        }

        #endregion
    }
}
