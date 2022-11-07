using Polly.CircuitBreaker;
using Polly;
using System.Net;
using System.Text;
using Polly.Fallback;
using Polly.Retry;

namespace DemoPolly.ResiliencePatterns
{
    public static class ResilienceHttpPoliciy
    {
        // Config Retry
        private const int NUMBER_OF_RETRIES = 2;
        private const int SECONDS_WAIT_IN_SECONDS = 5;

        // Config Cicuit Breaker
        private const int EXCEPTION_ALLOWED_BEFORE_BREAKING = 5;
        private const int DURATION_OF_BREAK_IN_SECONDS = 60;

        // Policies
        private static AsyncFallbackPolicy<HttpResponseMessage> FallbackPolicy()
        {
            return Policy<HttpResponseMessage>
                .HandleResult(response => CanActivateFallback(response))
                .Or<HttpRequestException>()
                .Or<BrokenCircuitException>()
                .FallbackAsync(FallbackAction, OnFallbackAsync);
        }

        private static AsyncRetryPolicy<HttpResponseMessage> RetryPolicy()
        {
            return Policy
                .HandleResult<HttpResponseMessage>(response => CanRetryAttempt(response))
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    NUMBER_OF_RETRIES,
                    retryAttempt => TimeSpan.FromSeconds(SECONDS_WAIT_IN_SECONDS),
                    onRetry: (httpResponse, timespan, retryCount, context) =>
                    {
                        ShowMessage($"Attempt: {retryCount}/{NUMBER_OF_RETRIES}", ConsoleColor.Yellow);
                    });
        }

        public static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy()
        {
            return Policy
                .HandleResult<HttpResponseMessage>(response => CanActiveCircuitBreaker(response))
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(EXCEPTION_ALLOWED_BEFORE_BREAKING, TimeSpan.FromSeconds(DURATION_OF_BREAK_IN_SECONDS),
                    onBreak: (ex, timespan, context) =>
                    {
                        ShowMessage("*** Circuit Status: Open ***", ConsoleColor.Red);
                    },

                    onReset: (context) =>
                    {
                        ShowMessage("*** Circuit Status: Closed ***", ConsoleColor.Green);

                    },
                    onHalfOpen: () =>
                    {
                        ShowMessage("*** Circuit Status: Half Open ***", ConsoleColor.Yellow);
                    });
        }


        // Policy Wrap
        public static IAsyncPolicy<HttpResponseMessage> GetAllPolicies()
        {
            return Policy.WrapAsync(FallbackPolicy(),RetryPolicy(),CircuitBreakerPolicy());
        }


        // FallBack
        private static Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> responseToFailedRequest, Context context, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };

            ShowMessage("Fallback => Running the Plan 'B' ", ConsoleColor.Yellow);

            return Task.FromResult(response);
        }

        private async static Task OnFallbackAsync(DelegateResult<HttpResponseMessage> response, Context context)
        {
            var message = new StringBuilder();
            message.Append("Fallback => Generating LOG... ");
            message.Append(response.Exception?.Message ?? "Unknown error");

            await LogFallback(message.ToString());
        }


        // Conditionals
        private static bool CanRetryAttempt(HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.RequestTimeout || (int)response.StatusCode >= 500;
        }

        private static bool CanActivateFallback(HttpResponseMessage response)
        {
            return !response.IsSuccessStatusCode;
        }

        private static bool CanActiveCircuitBreaker(HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.RequestTimeout || (int)response.StatusCode >= 500;
        }


        // Conditionals
        private static Task LogFallback(string message)
        {
            ShowMessage(message, ConsoleColor.Yellow);
            return Task.CompletedTask;
        }

        private static void ShowMessage(string message, ConsoleColor backgroundColor)
        {
            var previousBackgroundColor = Console.BackgroundColor;
            var previousForegroundColor = Console.ForegroundColor;

            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = ConsoleColor.Black;

            Console.Out.WriteLine(message);

            Console.BackgroundColor = previousBackgroundColor;
            Console.ForegroundColor = previousForegroundColor;
        }
    }
}
