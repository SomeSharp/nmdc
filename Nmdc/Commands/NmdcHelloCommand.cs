using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcHelloCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$Hello";

        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} {NmdcCommandParser.NickGroup}$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcHelloCommand(string nick)
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

        public static NmdcHelloCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var nick = groups[NmdcCommandParser.NickGroupName].Value;

            return new NmdcHelloCommand(nick);
        }

        #endregion
    }
}
