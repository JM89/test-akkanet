namespace ConsoleApp1.Messages
{
    public class CurrencyMessage
    {
        public string Currency { get; private set; }

        public CurrencyMessage(string currency)
        {
            this.Currency = currency;
        }
    }
}
