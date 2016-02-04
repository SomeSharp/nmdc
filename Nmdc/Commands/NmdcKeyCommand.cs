using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcKeyCommand : NmdcCommand
    {
        private const string CommandStart = "$Key";

        #region Parse Support

        private const string KeyGroupName = "key";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} (?<{KeyGroupName}>.+)$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcKeyCommand(string key)
        {
            Key = key;
        }

        #endregion

        #region Properties

        public string Key { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {Key ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static NmdcKeyCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var key = match.Groups[KeyGroupName].Value;

            return new NmdcKeyCommand(key);
        }

        #endregion
    }
}
