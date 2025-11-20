using Polly.Retry;
using Polly;

namespace ReceiptParserAPI.Resilience
{
    public static class GeminiRetryPolicy
    {
        public static AsyncRetryPolicy<HttpResponseMessage> GetPolicy()
        {
            return Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode) // Hata durumunda
                .WaitAndRetryAsync(3, // 3 kere dene
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2s, 4s, 8s bekle
                    (result, timeSpan, retryCount, context) =>
                    {
                        
                            //$"Hata alındı: {result.Result.StatusCode}. {timeSpan} sonra tekrar denenecek." ;
                    });
        }
    }
}
