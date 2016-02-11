using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public enum NmdcUserCommandType : byte
    {
        Separator = 0,
        Raw = 1,
        RawForSingleNick = 2,
        Url = 4,
        Clear = 255
    }

    [Flags]
    public enum NmdcUserCommandContextType : byte
    {
        Hub = 1,
        User = 2,
        Search = 4,
        FileList = 8
    }

    public sealed class NmdcUserCommandCommand : NmdcCommand
    {
        #region Typedefs

        public delegate IEnumerable<NmdcCommand> UserCommandsGenerator(string nickReplacement, string myNickReplacement, IDictionary<string, string> lineReplacements);

        #endregion

        #region Constants

        private const string CommandStart = "$UserCommand";

        public const string MyNickVariable = "%[mynick]";
        public const string NickVariable = "%[nick]";

        private const string TypeGroupName = "type";
        private const string ContextGroupName = "context";
        private const string TitleGroupName = "title";
        private const string CommandsGroupName = "commands";
        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} (?<{TypeGroupName}>\d+) (?<{ContextGroupName}>\d+) ((?<{TitleGroupName}>.+)\$(?<{CommandsGroupName}>.+){Regex.Escape("&#124;")})?$",
            RegexOptions.Singleline);

        private const string LineGroupName = "line";
        private static readonly Regex LineVariableParseRegex = new Regex(
            $@"\[line:(?<{LineGroupName}>.+)\]",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcUserCommandCommand(
            NmdcUserCommandType commandType,
            NmdcUserCommandContextType context,
            string title,
            IEnumerable<string> lines,
            UserCommandsGenerator commandsGenerator)
        {
            CommandType = commandType;
            Context = context;
            Title = title;
            Lines = lines;
            CommandsGenerator = commandsGenerator;
        }

        #endregion

        #region Properties

        public NmdcUserCommandType CommandType { get; private set; }

        public NmdcUserCommandContextType Context { get; private set; }

        public string Title { get; private set; }

        public IEnumerable<string> Lines { get; private set; }

        public UserCommandsGenerator CommandsGenerator { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            var rawPart = string.Empty;

            if ((CommandType == NmdcUserCommandType.Raw || CommandType == NmdcUserCommandType.RawForSingleNick)
                && CommandsGenerator != null)
            {
                var commands = CommandsGenerator(
                    NickVariable,
                    MyNickVariable,
                    (Lines ?? Enumerable.Empty<string>()).ToDictionary(l => l, l => l));
                var commandsGluedMessage = string.Join(string.Empty, commands.Select(c => $"{c}{StopChar}"));
                rawPart = EscapeArgument(commandsGluedMessage);
            }

            var commandsPart = string.IsNullOrEmpty(rawPart)
                ? string.Empty
                : $"{Title ?? string.Empty}${rawPart}";

            return $"{CommandStart} {(byte)CommandType} {(byte)Context} {commandsPart ?? string.Empty}";
        }

        #endregion

        #region Methods

        public static string GetLineVariable(string line)
        {
            return $"%[line:{line ?? string.Empty}]";
        }

        public static NmdcUserCommandCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;

            var commandType = (NmdcUserCommandType)byte.Parse(groups[TypeGroupName].Value);
            var context = (NmdcUserCommandContextType)byte.Parse(groups[ContextGroupName].Value);

            var titleGroup = groups[TitleGroupName];
            var title = titleGroup.Success
                ? titleGroup.Value
                : string.Empty;

            UserCommandsGenerator commandsGenerator = null;
            var lines = Enumerable.Empty<string>();
            var commandsGroup = groups[CommandsGroupName];
            if (commandsGroup.Success)
            {
                var gluedMessages = commandsGroup.Value;

                var lineMatch = LineVariableParseRegex.Match(gluedMessages);
                if (lineMatch.Success)
                    lines = lineMatch.Captures.Cast<Capture>().Select(capture => capture.Value);

                var commandsMessages = UnescapeArgument(gluedMessages).Split(new[] { StopChar }, StringSplitOptions.RemoveEmptyEntries);
                commandsGenerator = (nickReplacement, myNickReplacement, lineReplacements) =>
                {
                    var replacements = (lineReplacements ?? new Dictionary<string, string>())
                        .ToDictionary(line => GetLineVariable(line.Key), line => line.Value);
                    replacements.Add(NickVariable, nickReplacement);
                    replacements.Add(MyNickVariable, myNickReplacement);

                    var regex = new Regex($"({string.Join("|", replacements.Keys.Select(Regex.Escape))})");

                    return (commandsMessages ?? Enumerable.Empty<string>())
                        .Select(m => NmdcCommandParser.Parse(regex.Replace(m, mm => replacements[mm.Value])));
                };
            }

            return new NmdcUserCommandCommand(commandType, context, title, lines, commandsGenerator);
        }

        #endregion
    }
}
