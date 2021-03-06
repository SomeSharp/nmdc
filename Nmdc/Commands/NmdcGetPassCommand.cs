﻿using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcGetPassCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$GetPass";

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

        public static NmdcGetPassCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            return new NmdcGetPassCommand();
        }

        #endregion
    }
}
