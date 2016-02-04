using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcValidateNickCommand : NmdcCommand
    {
        private const string CommandStart = "$ValidateNick";

        #region Parse Support

        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} {NmdcCommandParser.NickGroup}$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcValidateNickCommand(string nick)
        {
            Nick = nick;
        }

        #endregion

        #region Properties

        public string Nick { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {Nick ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static NmdcValidateNickCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var nick = groups[NmdcCommandParser.NickGroupName].Value;

            return new NmdcValidateNickCommand(nick);
        }

        #endregion
    }
}
