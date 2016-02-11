using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcLockCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$Lock";
        private const string PkMarker = "Pk=";
        private const string ExtendedProtocolMarker = "EXTENDEDPROTOCOL";

        private const string CodeGroupName = "code";
        private const string PkGroupName = "pk";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} (?<{CodeGroupName}>[^ ]+) {PkMarker}(?<{PkGroupName}>.+)$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcLockCommand(string code, string pk)
        {
            Code = code;
            Pk = pk;
        }

        #endregion

        #region Properties

        public string Code { get; private set; }

        public string Pk { get; private set; }

        public bool IsExtendedProtocol
        {
            get { return Code?.StartsWith(ExtendedProtocolMarker) == true; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {Code ?? string.Empty} {PkMarker}{Pk ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static NmdcLockCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var code = match.Groups[CodeGroupName].Value;
            var pk = match.Groups[PkGroupName].Value;

            return new NmdcLockCommand(code, pk);
        }

        #endregion
    }
}
