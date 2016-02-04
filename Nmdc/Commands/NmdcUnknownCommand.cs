namespace SomeSharp.Nmdc
{
    public sealed class NmdcUnknownCommand : NmdcCommand
    {
        public NmdcUnknownCommand(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
