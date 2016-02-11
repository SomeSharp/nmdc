namespace SomeSharp.Nmdc
{
    public sealed class NmdcVersionCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$Version";

        #endregion

        #region Constructors

        public NmdcVersionCommand(string version)
        {
            Version = version;
        }

        #endregion

        #region Properties

        public string Version { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart} {Version ?? string.Empty}";
        }

        #endregion
    }
}
