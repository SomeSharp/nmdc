namespace SomeSharp.Nmdc
{
    public sealed class NmdcValidateNickCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$ValidateNick";

        #endregion

        #region Constructors

        public NmdcValidateNickCommand(string nick)
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
    }
}
