using System;
using System.Linq;

namespace SomeSharp.Nmdc
{
    public static class NmdcCommandParser
    {
        private static readonly Func<string, NmdcCommand>[] Parsers = new Func<string, NmdcCommand>[]
        {
            NmdcChatCommand.Parse,
            
            NmdcHelloCommand.Parse,
            NmdcHubNameCommand.Parse,
            NmdcLockCommand.Parse,
            NmdcMyInfoCommand.Parse,
            NmdcNickListCommand.Parse,
            NmdcOpListCommand.Parse,
            NmdcQuitCommand.Parse,
            NmdcSupportsCommand.Parse,
            NmdcToCommand.Parse,
            NmdcUserCommandCommand.Parse,
            NmdcBadPassCommand.Parse,
            NmdcGetPassCommand.Parse,
            NmdcZOnCommand.Parse
        };

        public const string NickParsePattern = "[^ ]+?";
        public const string NickGroupName = "nick";
        public static readonly string NickGroup = GetNickGroup(NickGroupName);

        public static NmdcCommand Parse(string message)
        {
            return Parsers.Select(p => p(message)).FirstOrDefault(c => c != null) ?? new NmdcUnknownCommand(message);
        }
        
        public static string GetNickGroup(string groupName)
        {
            return $"(?<{groupName}>{NickParsePattern})";
        }
    }
}
