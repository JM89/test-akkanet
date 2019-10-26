namespace ConsoleApp1.Messages
{
    public class CurrencyMessage
    {
        public string Origin { get; set; }
        public string Currency { get; private set; }

        public CurrencyMessage(string origin, string currency)
        {
            this.Origin = origin;
            this.Currency = currency;
        }
    }
}
