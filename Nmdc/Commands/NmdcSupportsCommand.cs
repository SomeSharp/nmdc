using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public enum NmdcSupportsOptionType
    {
        BotList,
        ClientID,
        Feed,
        HubTopic,
        IN,
        MCTo,
        NoGetINFO,
        NoHello,
        OpPlus,
        QuickList,
        TTHSearch,
        UserCommand,
        UserIP2,
        ZLine,
        ZPipe0,
        ADCGet,
        BZList,
        CHUNK,
        GetCID,
        GetTestZBlock,
        GetZBlock,
        MiniSlots,
        TTHL,
        TTHF,
        XmlBZList,
        ZLIG,
        HubINFO,
        ZPipe
    }

    public sealed class NmdcSupportsCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$Supports";

        private const string OptionGroupName = "opt";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)}( (?<{OptionGroupName}>\w+))+",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcSupportsCommand(IEnumerable<NmdcSupportsOptionType> options)
        {
            Options = options;
        }

        #endregion

        #region Properties

        public IEnumerable<NmdcSupportsOptionType> Options { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {string.Join(" ", Options ?? Enumerable.Empty<NmdcSupportsOptionType>())}";
        }

        #endregion

        #region Methods

        public static NmdcSupportsCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var optionGroup = match.Groups[OptionGroupName];
            if (!optionGroup.Success)
                return null;

            var optionCaptures = optionGroup.Captures;
            if (optionCaptures.Count == 0)
                return null;

            var options = optionCaptures.Cast<Capture>()
                .Select(capture => Enum.Parse(typeof(NmdcSupportsOptionType), capture.Value))
                .Cast<NmdcSupportsOptionType>()
                .ToArray();

            return new NmdcSupportsCommand(options);
        }

        #endregion
    }
}
