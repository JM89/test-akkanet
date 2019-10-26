using System;

namespace ConsoleApp2.Messages
{
    public class Message
    {
        public string Content { get; private set; }
        public Guid Id { get; private set; }

        public Message(Guid id, string content)
        {
            this.Id = id;
            this.Content = content;
        }
    }
}
