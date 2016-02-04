using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcMyPassCommand : NmdcCommand
    {
        private const string CommandStart = "$MyPass";

        #region Parse Support

        private const string PasswordGroupName = "pass";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} (?<{PasswordGroupName}>.*)$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcMyPassCommand(string password)
        {
            Password = password;
        }

        #endregion

        #region Properties

        public string Password { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {Password ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static NmdcMyPassCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var password = match.Groups[PasswordGroupName].Value;

            return new NmdcMyPassCommand(password);
        }

        #endregion
    }
}
