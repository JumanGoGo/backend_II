namespace backend_II.Authentication
{
    public interface IApiKeyValidator
    {
        Task<bool> ValidateApiKeyAsync(string apiKey);
    }
}
