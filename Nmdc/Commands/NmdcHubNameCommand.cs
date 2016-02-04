using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcHubNameCommand : NmdcCommand
    {
        private const string CommandStart = "$HubName";

        #region Parse Support

        private const string NameGroupName = "name";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} (?<{NameGroupName}>.*)$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcHubNameCommand(string name)
        {
            Name = name;
        }

        #endregion

        #region Properties

        public string Name { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {Name ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static NmdcHubNameCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var name = match.Groups[NameGroupName].Value;

            return new NmdcHubNameCommand(name);
        }

        #endregion
    }
}
