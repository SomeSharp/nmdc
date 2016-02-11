namespace SomeSharp.Nmdc
{
    public sealed class NmdcUnknownCommand : NmdcCommand
    {
        #region Constructor

        public NmdcUnknownCommand(string message)
        {
            Message = message;
        }

        #endregion

        #region Properties

        public string Message { get; private set; }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Message;
        }

        #endregion
    }
}
