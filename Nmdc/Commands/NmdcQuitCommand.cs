﻿using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcQuitCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$Quit";

        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} {NmdcCommandParser.NickGroup}$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcQuitCommand(string nick)
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

        public static NmdcQuitCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var nick = groups[NmdcCommandParser.NickGroupName].Value;

            return new NmdcQuitCommand(nick);
        }

        #endregion
    }
}
