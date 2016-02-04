using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcMyInfoCommand : NmdcCommand
    {
        private const string CommandStart = "$MyINFO";

        #region Parse Support

        private const string TagGroupName = "tag";
        private const string DescriptionGroupName = "desc";
        private static readonly string DescriptionGroup = $@"(?<{DescriptionGroupName}>.*)(\<(?<{TagGroupName}>.+)\>)?";

        private const string ConnectionGroupName = "conn";
        private const string MagicByteGroupName = "magb";
        private static readonly string ConnectionGroup = $@"(?<{ConnectionGroupName}>.+)(?<{MagicByteGroupName}>.)?";

        private const string SharedSizeGroupName = "ss";
        private static readonly string SharedSizeGroup = $@"(?<{SharedSizeGroupName}>\d+)";

        private const string EmailGroupName = "email";
        private static readonly string EmailGroup = $@"(?<{EmailGroupName}>.+)";

        private static readonly Regex ParseRegex = new Regex(
            $@"^{Regex.Escape(CommandStart)} \$ALL {NmdcCommandParser.NickGroup}( {DescriptionGroup}\$)? \${ConnectionGroup}?\${EmailGroup}?\${SharedSizeGroup}?\$",
            RegexOptions.Singleline);

        #endregion

        #region Constructors

        public NmdcMyInfoCommand(string nick, string description, string tag, string connection, byte magicByte, string email, long sharedSize)
        {
            Nick = nick;
            Description = description;
            Tag = tag;
            Connection = connection;
            MagicByte = magicByte;
            Email = email;
            SharedSize = sharedSize;
        }

        #endregion

        #region Properties

        public string Nick { get; private set; }

        public string Description { get; private set; }

        public string Tag { get; private set; }

        public string Connection { get; private set; }

        public byte MagicByte { get; private set; }

        public string Email { get; private set; }

        public long SharedSize { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            var tagPart = string.IsNullOrEmpty(Tag)
                ? string.Empty
                : $"<{Tag}>";

            return $"{CommandStart} $ALL {Nick ?? string.Empty} {Description ?? string.Empty}{tagPart}$ ${Connection ?? string.Empty}{(char)MagicByte}${Email ?? string.Empty}${SharedSize}$";
        }

        #endregion

        #region Methods

        public static NmdcMyInfoCommand Parse(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var match = ParseRegex.Match(message);
            if (match == null || !match.Success)
                return null;

            var groups = match.Groups;
            if (groups == null || groups.Count == 0)
                return null;

            // Nick

            var nick = groups[NmdcCommandParser.NickGroupName].Value;

            // Description

            var descriptionGroup = groups[DescriptionGroupName];
            var description = descriptionGroup.Success
                ? descriptionGroup.Value
                : string.Empty;

            // Tag

            var tagGroup = groups[TagGroupName];
            var tag = tagGroup.Success
                ? tagGroup.Value
                : string.Empty;

            // Connection

            var connectionGroup = groups[ConnectionGroupName];
            var connection = connectionGroup.Success
                ? connectionGroup.Value
                : string.Empty;

            // Magic byte

            var magicByteGroup = groups[MagicByteGroupName];
            var magicByte = magicByteGroup.Success
                ? (byte)magicByteGroup.Value[0]
                : (byte)0;

            // Email

            var emailGroup = groups[EmailGroupName];
            var email = emailGroup.Success
                ? emailGroup.Value
                : string.Empty;

            // Shared size

            long sharedSize = 0;
            var sharedSizeGroup = groups[SharedSizeGroupName];
            if (sharedSizeGroup.Success && !long.TryParse(sharedSizeGroup.Value, out sharedSize))
                sharedSize = 0;

            //

            return new NmdcMyInfoCommand(nick, description, tag, connection, magicByte, email, sharedSize);
        }

        #endregion
    }
}
