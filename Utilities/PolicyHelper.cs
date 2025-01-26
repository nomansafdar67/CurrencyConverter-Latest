using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Wrap;

namespace CurrencyConverter.Utilities
{
    public static class PolicyHelper
    {
        public static AsyncPolicyWrap CreateCombinedPolicy(
        ILogger logger,
        int maxRetryAttempts = 3,
        double initialDelayInSeconds = 1,
        int maxParallelization = 10,
        int maxQueuingActions = 50)
        {
            // Retry Policy
            var retryPolicy = Policy
                .Handle<Exception>() // Retry on any exception
                .WaitAndRetryAsync(
                    maxRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(initialDelayInSeconds * Math.Pow(2, retryAttempt - 1)), // Exponential backoff
                    (exception, timeSpan, retryCount, context) =>
                    {
                        logger.LogWarning($"Retry {retryCount} failed. Waiting {timeSpan} before next retry. Exception: {exception.Message}");
                    });

            // Bulkhead Policy
            var bulkheadPolicy = Policy.BulkheadAsync(
                maxParallelization: maxParallelization, // Max concurrent operations
                maxQueuingActions: maxQueuingActions,  // Max queued operations
                onBulkheadRejectedAsync: context =>
                {
                    logger.LogWarning("Bulkhead limit reached. Request rejected.");
                    return Task.CompletedTask;
                });

            // Combine Policies
            return Policy.WrapAsync(retryPolicy, bulkheadPolicy);
        }

    }
}
