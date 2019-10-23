using Akka.Actor;
using ConsoleApp1.Messages;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
                    this._consoleWriter.Tell(new ResultSuccessfulMessage($"Conversion rate ({msg.Currency}->EUR) = {result.rates[msg.Currency]}"));
                }
                catch (FlurlHttpException ex)
                {
                    this._consoleWriter.Tell(new ResultErrorMessage($"Status Code = {ex.Message}"));
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