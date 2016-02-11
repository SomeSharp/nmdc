using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcToCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$To";

        private const string ReceiverNickGroupName = "rn";
        private const string SenderNickGroupName = "sn";
        private const string MessageGroupName = "msg";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)}: {NmdcCommandParser.GetNickGroup(ReceiverNickGroupName)} From: {NmdcCommandParser.GetNickGroup(SenderNickGroupName)} \$\<{NmdcCommandParser.NickGroup}\> (?<{MessageGroupName}>.*)$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcToCommand(string receiverNick, string senderNick, string nick, string message)
        {
            ReceiverNick = receiverNick;
            SenderNick = senderNick;
            Nick = nick;
            Message = message;
        }

        #endregion

        #region Properties

        public string ReceiverNick { get; private set; }

        public string SenderNick { get; private set; }

        public string Nick { get; private set; }

        public string Message { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart}: {ReceiverNick ?? string.Empty} From: {SenderNick ?? string.Empty} $<{Nick ?? string.Empty}> {Message ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static NmdcToCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var receiverNick = groups[ReceiverNickGroupName].Value;
            var senderNick = groups[SenderNickGroupName].Value;
            var nick = groups[NmdcCommandParser.NickGroupName].Value;
            var chatMessage = groups[MessageGroupName].Value;

            return new NmdcToCommand(receiverNick, senderNick, nick, chatMessage);
        }

        #endregion
    }
}
