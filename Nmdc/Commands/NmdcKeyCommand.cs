using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public sealed class NmdcKeyCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$Key";

        private static readonly Dictionary<string, string> KeyEscapeInfo =
            Encoding.Default.GetChars(new byte[] { 0, 5, 36, 96, 124, 126 })
            .ToDictionary(c => c.ToString(), c => $"/%DCN{(byte)c:000}%/");

        private static readonly Regex KeyEscapeRegex = new Regex($"({string.Join("|", KeyEscapeInfo.Keys.Select(Regex.Escape))})");

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

        public static NmdcKeyCommand Create(string lockCode)
        {
            return new NmdcKeyCommand(LockCodeToKey(lockCode));
        }

        private static string LockCodeToKey(string lockCode)
        {
            var lockCodeLength = lockCode.Length;
            var keyBytes = new byte[lockCodeLength];

            for (int i = 1; i < lockCodeLength; i++)
            {
                keyBytes[i] = (byte)(lockCode[i] ^ lockCode[i - 1]);
            }

            keyBytes[0] = (byte)(lockCode[0] ^ lockCode[lockCodeLength - 1] ^ lockCode[lockCodeLength - 2] ^ 5);

            for (int i = 0; i < lockCodeLength; i++)
            {
                keyBytes[i] = (byte)(((keyBytes[i] << 4) & 0xF0) | ((keyBytes[i] >> 4) & 0x0F));
            }

            var result = string.Empty;
            var key = Encoding.Default.GetString(keyBytes);

            return KeyEscapeRegex.Replace(key, m => KeyEscapeInfo[m.Value]);
        }

        #endregion
    }
}
