using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcZOnCommand : NmdcCommand
    {
        private const string CommandStart = "$ZOn";

        #region Parse Support

        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)}$",
            RegexOptions.Singleline);

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart}";
        }

        #endregion

        #region Methods

        public static NmdcZOnCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            return new NmdcZOnCommand();
        }

        #endregion
    }
}
