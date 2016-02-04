using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public static class NmdcUtilities
    {
        public const char VerticalBarChar = '|';
        public const char DollarChar = '$';

        public const byte VerticalBarByte = 124;
        public const byte DollarByte = 36;

        private static readonly Dictionary<string, string> EscapeInfo =
            Encoding.Default.GetChars(new byte[] { 0, 5, 36, 96, 124, 126 })
            .ToDictionary(c => c.ToString(), c => $"/%DCN{(byte)c:000}%/");

        private static readonly Regex EscapeRegex = new Regex($"({string.Join("|", EscapeInfo.Keys.Select(Regex.Escape))})");

        public static string LockCodeToKey(string lockCode)
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

            return EscapeRegex.Replace(key, m => EscapeInfo[m.Value]);
        }
    }
}
