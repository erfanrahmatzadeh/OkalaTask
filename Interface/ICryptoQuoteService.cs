namespace OkalaTask.Interface
{
    public interface ICryptoQuoteService
    {
        Task<Dictionary<string, decimal>> GetCryptoQuoteAsync(string cryptoCode);
    }
}
