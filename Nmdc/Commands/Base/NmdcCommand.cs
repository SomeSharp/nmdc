using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SomeSharp.Nmdc
{
    public abstract class NmdcCommand
    {
        #region Constants

        public const char StopChar = '|';
        public const byte StopByte = 124; // |

        private static readonly Dictionary<string, string> EscapeCharacters = new Dictionary<string, string>()
        {
            { "|", "&#124;" },
            { "&", "&#38;" },
            { "$", "&#36;" }
        };

        private static readonly Dictionary<string, string> UnescapeCharacters = EscapeCharacters.ToDictionary(kv => kv.Value, kv => kv.Key);

        private static readonly Regex EscapeRegex = new Regex($"({string.Join("|", EscapeCharacters.Keys.Select(Regex.Escape))})");
        private static readonly Regex UnescapeRegex = new Regex($"({string.Join("|", UnescapeCharacters.Keys.Select(Regex.Escape))})");

        #endregion

        #region Methods

        protected static string EscapeArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
                return argument;

            return EscapeRegex.Replace(argument, m => EscapeCharacters[m.Value]);
        }

        protected static string UnescapeArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
                return argument;

            return UnescapeRegex.Replace(argument, m => UnescapeCharacters[m.Value]);
        }

        #endregion
    }
}
