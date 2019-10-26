using Akka.Actor;
using ConsoleApp1.Messages;
using Flurl.Http;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleApp1.Actors
{
    public class CurrencyCheckerActor : UntypedActor
    {
        private const string BaseUrl = "https://api.exchangeratesapi.io/latest?symbols=";
        private CancellationTokenSource _cancel;
        private readonly IActorRef _consoleWriter;

        public CurrencyCheckerActor(IActorRef consoleWriter)
        {
            this._consoleWriter = consoleWriter;
            _cancel = new CancellationTokenSource();
        }

        protected override void OnReceive(object message)
        {
            if (message is CurrencyMessage)
            {
                var msg = message as CurrencyMessage;
                try
                {
                    var result = $"{BaseUrl}{msg.Currency}".GetJsonAsync<ExchangeRates>(_cancel.Token).GetAwaiter().GetResult();
                    this._consoleWriter.Tell(new ResultSuccessfulMessage($"{msg.Origin}: Conversion rate ({msg.Currency}->EUR) = {result.rates[msg.Currency]}"));
                }
                catch 
                {
                    this._consoleWriter.Tell(new ResultErrorMessage($"{msg.Origin}: Conversion rate ({msg.Currency}->EUR) failed"));
                }
            }
            else if (message is CancelMessage)
            {
                _cancel.Cancel();
            }
        }

        public class ExchangeRates
        {
            public string @base { get;set; }

            public string date { get; set; }

            public Dictionary<string, decimal> rates { get; set; }
        }
    }
}