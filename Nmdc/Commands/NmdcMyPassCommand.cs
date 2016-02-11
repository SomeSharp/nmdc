using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcMyPassCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$MyPass";

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
    }
}
