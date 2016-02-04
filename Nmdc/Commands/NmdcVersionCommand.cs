using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcVersionCommand : NmdcCommand
    {
        private const string CommandStart = "$Version";

        #region Parse Support

        private const string VersionGroupName = "version";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} (?<{VersionGroupName}>.+)$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcVersionCommand(string version)
        {
            Version = version;
        }

        #endregion

        #region Properties

        public string Version { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {Version ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static NmdcVersionCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var version = match.Groups[VersionGroupName].Value;

            return new NmdcVersionCommand(version);
        }

        #endregion
    }
}
