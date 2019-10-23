using Akka.Actor;
using ConsoleApp1.Messages;
using System.IO;

namespace ConsoleApp1.Actors
{
    class FileReaderActor : UntypedActor
    {
        private readonly IActorRef _currencyChecker;

        public FileReaderActor(IActorRef currencyChecker)
        {
            this._currencyChecker = currencyChecker;
        }

        protected override void OnReceive(object message)
        {
            if (message is TextFileToProcessMessage)
            {
                var msg = message as TextFileToProcessMessage;

                // Read file using StreamReader. Reads file line by line  
                using (var file = new StreamReader(msg.FilePath))
                {
                    string ln;
                    while ((ln = file.ReadLine()) != null)
                    {
                        _currencyChecker.Tell(new CurrencyMessage(ln));
                    }
                    file.Close();
                }
            }
        }
    }
}
