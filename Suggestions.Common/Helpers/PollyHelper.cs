using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Suggestions.Common.Helpers
{
    public static class PollyHelper
    {
        /// <summary>
        /// Возвращает политику, выполняющую действие, поглащая любые ошибки
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="operationName"> Название операции (если null, то название должно передаваться в контекст: new Context("MyApi.GetSomething") </param>
        public static IAsyncPolicy WithLoggingAndSwallowErrors(ILogger logger, string operationName)
        {
            if (operationName == "")
            {
                throw new ArgumentException("operationName должен быть указан или быть null");
            }

            var operationLoggingPolicy = new OperationLoggingPolicy(operationName, logger, null);
            var fallbackForAnyException = Policy
                .Handle<Exception>()
                .FallbackAsync((c, ct) => Task.CompletedTask, (ex, context) =>
                {
                    if (context.GetShouldLog() != false)
                    {
                        var opName = operationName ?? context.OperationKey ?? context.PolicyKey;
                        LogFallbackError(logger, opName, ex);
                    }

                    return Task.CompletedTask;
                });

            return fallbackForAnyException.WrapAsync(operationLoggingPolicy)
                .WithPolicyKey(operationName);
        }

        /// <summary>
        /// Возвращает политику выполняющую действие с возвратом результата по умолчанию в случае любой ошибки
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="operationName"> Название операции (если null, то название должно передаваться в контекст: new Context("MyApi.GetSomething") </param>
        /// <param name="fallback">Значение возвращаемое при возникновении любой ошибки</param>
        public static IAsyncPolicy<T> WithLoggingAndFallback<T>(ILogger logger, string operationName, T fallback)
            where T : class
        {
            if (operationName == "")
            {
                throw new ArgumentException("operationName должен быть указан или быть null");
            }

            var operationLoggingPolicy = new OperationLoggingPolicy(operationName, logger, null);
            var fallbackForAnyException = Policy<T>
                .Handle<Exception>()
                .FallbackAsync(fallback, (result, context) =>
                {
                    if (context.GetShouldLog() != false)
                    {
                        var opName = operationName ?? context.OperationKey ?? context.PolicyKey;
                        LogFallbackError(logger, opName, result.Exception);
                    }

                    return Task.CompletedTask;
                });

            return fallbackForAnyException.WrapAsync(operationLoggingPolicy)
                .WithPolicyKey(operationName);
        }

        /// <summary>
        /// Возвращает политику выполняющую действие с таймаутом
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="operationName"> Название операции (если null, то название должно передаваться в контекст: new Context("MyApi.GetSomething") </param>
        /// <param name="timeout">Таймаут</param>
        /// <param name="fallback">Значение возвращаемое при возникновении таймаута</param>
        public static IAsyncPolicy<T> WithLoggingAndTimeout<T>(ILogger logger, string operationName, TimeSpan timeout,
            T fallback) where T : class
        {
            if (operationName == "")
            {
                throw new ArgumentException("operationName должен быть указан или быть null");
            }

            var operationLoggingPolicy = new OperationLoggingPolicy(operationName, logger, timeout);
            var fallbackForTimeout = Policy<T>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(fallback, (result, context) =>
                {
                    if (context.GetShouldLog() != false)
                    {
                        var opName = operationName ?? context.OperationKey ?? context.PolicyKey;
                        logger.LogWarning(
                            "FallbackPolicy: операция {OperationName} не завершилась вовремя, возврат значения по умолчанию",
                            opName);
                    }

                    return Task.CompletedTask;
                });

            var timeOutPolicy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic,
                (context, span, abandonedTask) =>
                {
                    if (context.GetShouldLog() != false)
                    {
                        var opName = operationName ?? context.OperationKey ?? context.PolicyKey;
                        logger.LogWarning("TimeoutPolicy: таймаут операции {OperationName} - {TimeoutValueMs}мс",
                            opName, timeout.Milliseconds);
                    }

                    return Task.CompletedTask;
                });

            return fallbackForTimeout.WrapAsync(timeOutPolicy).WrapAsync(operationLoggingPolicy)
                .WithPolicyKey(operationName);
        }

        /// <summary>
        /// Возвращает политику выполняющую действие с таймаутом и возвратом результата по умолчанию в случае любой ошибки
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="operationName"> Название операции (если null, то название должно передаваться в контекст: new Context("MyApi.GetSomething") </param>
        /// <param name="timeout">Таймаут</param>
        /// <param name="fallback">Значение возвращаемое при возникновении таймаута или любой ошибки</param>
        public static IAsyncPolicy<T> WithLoggingTimeoutAndFallback<T>(ILogger logger, string operationName, TimeSpan timeout,
            T fallback) where T : class
        {
            if (operationName == "")
            {
                throw new ArgumentException("operationName должен быть указан или быть null");
            }

            var operationLoggingPolicy = new OperationLoggingPolicy(operationName, logger, timeout);

            var fallbackForAnyException = Policy<T>
                .Handle<Exception>()
                .FallbackAsync(fallback, (result, context) =>
                {
                    if (context.GetShouldLog() != false)
                    {
                        var opName = operationName ?? context.OperationKey ?? context.PolicyKey;
                        LogFallbackError(logger, opName, result.Exception);
                    }

                    return Task.CompletedTask;
                });

            var fallbackForTimeout = Policy<T>
                .Handle<TimeoutRejectedException>()
                .FallbackAsync(fallback, (result, context) =>
                {
                    if (context.GetShouldLog() != false)
                    {
                        var opName = operationName ?? context.OperationKey ?? context.PolicyKey;
                        logger.LogWarning(
                            "FallbackPolicy: операция {OperationName} не завершилась вовремя, возврат значения по умолчанию",
                            opName);
                    }

                    return Task.CompletedTask;
                });

            var timeOutPolicy = Policy.TimeoutAsync(timeout, TimeoutStrategy.Pessimistic,
                (context, span, abandonedTask) =>
                {
                    if (context.GetShouldLog() != false)
                    {
                        var opName = operationName ?? context.OperationKey ?? context.PolicyKey;
                        logger.LogWarning("TimeoutPolicy: таймаут операции {OperationName} - {TimeoutValueMs}мс",
                            opName, timeout.Milliseconds);
                    }

                    return Task.CompletedTask;
                });

            return fallbackForAnyException.WrapAsync(fallbackForTimeout).WrapAsync(timeOutPolicy).WrapAsync(operationLoggingPolicy)
                .WithPolicyKey(operationName);
        }
        private static void LogFallbackError(ILogger logger, string opName, Exception ex)
        {
            if (ex is BrokenCircuitException)
            {
                logger.LogError("FallbackPolicy: операция {OperationName} завершилась неудачно вследствие BrokenCircuitException, возврат значения по умолчанию \r\n" + TruncateLongString(ex.ToString(), 300) + "...", opName);
            }
            else
            {
                logger.LogError(ex, "FallbackPolicy: операция {OperationName} завершилась с ошибкой, возврат значения по умолчанию", opName);
            }
        }

        private static string TruncateLongString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
                return str;
            return str.Substring(0, Math.Min(str.Length, maxLength));
        }
    }
}