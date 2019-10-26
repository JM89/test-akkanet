namespace ConsoleApp1.Messages
{
    public class ResultErrorMessage
    {
        public string Reason { get; private set; }

        public ResultErrorMessage(string reason)
        {
            this.Reason = reason;
        }
    }
}
