namespace SomeSharp.Nmdc
{
    public sealed class NmdcGetNickListCommand : NmdcCommand
    {
        #region Constants

        private const string CommandStart = "$GetNickList";

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{CommandStart}";
        }

        #endregion
    }
}
