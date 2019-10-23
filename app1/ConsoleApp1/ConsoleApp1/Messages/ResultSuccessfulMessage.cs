namespace ConsoleApp1.Messages
{
    public class ResultSuccessfulMessage
    {
        public string Content { get; private set; }

        public ResultSuccessfulMessage(string content)
        {
            this.Content = content;
        }
    }
}
