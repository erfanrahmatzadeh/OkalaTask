namespace OkalaTask.Models
{
    public class CryptoQuoteResponse
    {
        public string CryptoCode { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}
