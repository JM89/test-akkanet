using Akka.Actor;
using ConsoleApp1.Errors;
using ConsoleApp1.Messages;
using System.IO;

namespace ConsoleApp1.Actors
{
    class FileReaderActor : UntypedActor
    {
        private readonly IActorRef _currencyChecker;
        private readonly string _filePath;
        private StreamReader _fileStreamReader;

        public FileReaderActor(IActorRef currencyChecker, string filePath)
        {
            this._currencyChecker = currencyChecker;
            this._filePath = filePath;
        }

        protected override void PreStart()
        {
            _fileStreamReader = new StreamReader(_filePath);

            Self.Tell(new TextFileToProcessMessage(_filePath));
        }

        protected override void OnReceive(object message)
        {
            if (!_filePath.EndsWith(".txt"))
            {
                throw new FileExtensionUnhandled(_filePath);
            }

            string ln;
            while ((ln = _fileStreamReader.ReadLine()) != null)
            {
                _currencyChecker.Tell(new CurrencyMessage(_filePath, ln));
            }
        }

        protected override void PostStop()
        {
            _fileStreamReader.Close();
            _fileStreamReader.Dispose();
            base.PostStop();
        }
    }
}
