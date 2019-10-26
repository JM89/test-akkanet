namespace ConsoleApp1.Messages
{
    public class TextFileToProcessMessage
    {
        public string FilePath { get; private set; }

        public TextFileToProcessMessage(string filePath)
        {
            this.FilePath = filePath;
        }
    }
}
