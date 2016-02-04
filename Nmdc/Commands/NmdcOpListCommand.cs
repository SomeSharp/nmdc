using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcOpListCommand : NmdcCommand
    {
        private const string CommandStart = "$OpList";

        #region Parse Support

        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} ({NmdcCommandParser.NickGroup}\$\$)+$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcOpListCommand(IEnumerable<string> nicks)
        {
            Nicks = nicks;
        }

        #endregion

        #region Properties

        public IEnumerable<string> Nicks { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {string.Join(string.Empty, (Nicks ?? Enumerable.Empty<string>()).Select(n => $"{n}$$"))}";
        }

        #endregion

        #region Methods

        public static NmdcOpListCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var nickGroup = match.Groups[NmdcCommandParser.NickGroupName];
            if (!nickGroup.Success)
                return null;

            var nickCaptures = nickGroup.Captures;
            if (nickCaptures.Count == 0)
                return null;

            var nicks = Enumerable.Range(0, nickCaptures.Count)
                .Select(i => nickCaptures[i].Value)
                .ToArray();

            return new NmdcOpListCommand(nicks);
        }

        #endregion
    }
}
