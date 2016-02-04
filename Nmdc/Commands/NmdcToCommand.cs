using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcToCommand : NmdcCommand
    {
        private const string CommandStart = "$To";

        #region Parse Support

        private const string RecieverNickGroupName = "rn";
        private const string SenderNickGroupName = "sn";
        private const string MessageGroupName = "msg";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)}: {NmdcCommandParser.GetNickGroup(RecieverNickGroupName)} From: {NmdcCommandParser.GetNickGroup(SenderNickGroupName)} \$\<{NmdcCommandParser.NickGroup}\> (?<{MessageGroupName}>.*)$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcToCommand(string recieverNick, string senderNick, string nick, string message)
        {
            RecieverNick = recieverNick;
            SenderNick = senderNick;
            Nick = nick;
            Message = message;
        }

        #endregion

        #region Properties

        public string RecieverNick { get; private set; }

        public string SenderNick { get; private set; }

        public string Nick { get; private set; }

        public string Message { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart}: {RecieverNick ?? string.Empty} From: {SenderNick ?? string.Empty} $<{Nick ?? string.Empty}> {Message ?? string.Empty}";
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

            var recieverNick = groups[RecieverNickGroupName].Value;
            var senderNick = groups[SenderNickGroupName].Value;
            var nick = groups[NmdcCommandParser.NickGroupName].Value;
            var chatMessage = groups[MessageGroupName].Value;

            return new NmdcToCommand(recieverNick, senderNick, nick, chatMessage);
        }

        #endregion
    }
}
