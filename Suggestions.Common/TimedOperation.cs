using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using Serilog.Context;

namespace Suggestions.Common
{
    /// <summary>
    /// Операция, время выполнения которой логируется
    /// </summary>
    public class TimedOperation : IDisposable
    {
        public const string BeginningOperationTemplate =
            "Начало операции: {TimedOperationDescription}, аргументы: {TimedOperationArgs}";

        public const string BeginningOperationNoArgsTemplate = "Начало операции: {TimedOperationDescription}";

        public const string CompletedOperationTemplate =
            "Завершение операции {TimedOperationDescription} (время выполнения: {TimedOperationElapsedInMs} мс, аргументы: {TimedOperationArgs}, лимит: {TimedOperationLimit} мс, превышение лимита: {TimedOperationLimitExceeded})";

        public const string CompletedOperationNoLimitTemplate =
            "Завершение операции: {TimedOperationDescription} (время выполнения: {TimedOperationElapsedInMs} мс, аргументы: {TimedOperationArgs})";

        private readonly string _args;
        private readonly string _description;
        private readonly TimeSpan? _limit;

        private readonly ILogger _logger;
        private readonly bool _shouldLog;
        private readonly Stopwatch _sw;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="description">Описание операции</param>
        /// <param name="limit">Лимит выполнения</param>
        /// <param name="logBegining">Логировать начало операции</param>
        /// <param name="shouldLog">
        /// Следует ли логировать операцию. Признаком можно осуществлять выборочное логирование потока
        /// данных.
        /// </param>
        public TimedOperation(ILogger logger, string description, TimeSpan? limit, bool logBegining, object args = null,
            bool shouldLog = true)
        {
            _logger = logger;
            _limit = limit;
            _description = description;
            _args = args == null ? "" : ToJson(args);
            _sw = Stopwatch.StartNew();
            _shouldLog = shouldLog;

            if (logBegining && _shouldLog)
            {
                _logger.LogInformation(GetBeginingTemplate(), description, _args);
            }
        }

        public virtual void Dispose()
        {
            _sw.Stop();
            if (!_shouldLog)
            {
                return;
            }

            if (_limit.HasValue && _sw.Elapsed > _limit.Value)
            {
                _logger.LogWarning(GetCompletedTemplate(), _description, _sw.ElapsedMilliseconds, _args,
                    _limit.Value.TotalMilliseconds, true, _args);
            }
            else
            {
                _logger.LogInformation(GetCompletedTemplate(), _description, _sw.ElapsedMilliseconds, _args,
                    _limit?.TotalMilliseconds, false);
            }
        }

        private string ToJson(object obj, bool ignoreLoopHandling = false)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling =
                    ignoreLoopHandling ? ReferenceLoopHandling.Serialize : ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(obj, settings).Replace("\\\"", string.Empty).Replace("\"{", "{").Replace("}\"", "}").Trim();
        }

        private string GetCompletedTemplate()
        {
            return _limit.HasValue ? CompletedOperationTemplate : CompletedOperationNoLimitTemplate;
        }

        private string GetBeginingTemplate()
        {
            return string.IsNullOrEmpty(_args) ? BeginningOperationNoArgsTemplate : BeginningOperationTemplate;
        }

        /// <summary>
        /// Выполняет операцию с таймаутом, при превышении которого возвращается указанное значение по умолчанию. При таймауте не
        /// возникает никаких исключений. Дополнительно логируется время выполнения целевого метода, даже если оно превышает
        /// таймаут.
        /// </summary>
        /// <param name="task">Таск</param>
        /// <param name="timeout">Таймаут</param>
        /// <param name="defaultResult">Значение возвращаемое при превышении таймаута</param>
        /// <param name="logger">Логгер</param>
        /// <param name="operationName">Название операции в логе</param>
        /// <param name="additionalParams">Дополнительные параметры</param>
        public static async Task<T> ExecuteWithTimeout<T>(Func<Task<T>> task, TimeSpan timeout, T defaultResult,
            ILogger logger, string operationName, object additionalParams = null) where T : class
        {
            var t = ExecuteTaskWithLogging(task);

            // Ограничиваем время на попытку получения данных от ABCP
            if (await Task.WhenAny(t, Task.Delay(timeout)) == t)
            {
                // Даем возможность выбросить исходное исключение вместо AggregateException
                return await t;
            }

            logger.LogWarning(
                "ExecuteWithTimeoutAndMetrics: таймаут операции {OperationName}, возврат значения по умолчанию",
                operationName);
            return defaultResult;

            async Task<T> ExecuteTaskWithLogging(Func<Task<T>> target)
            {
                using (logger.BeginTimedOperation(operationName, timeout, args: additionalParams))
                {
                    return await target();
                }
            }
        }
    }

    public static class LoggerExtension
    {
        /// <summary>
        /// Начинает операцию, код которой помещается в блок using и время выполнения которой логируется с уровнем Information.
        /// Опционально можно указать лимит выполнения, в этом лучае сообщение об окончании будет залогировано как Warning
        /// <para />
        /// Параметры для фильтрации в Kibana:
        /// TimedOperationDescription - название операции
        /// TimedOperationElapsedInMs - время выполнения в мс
        /// TimedOperationLimit - лимит
        /// TimedOperationLimitExceeded - признак превышения лимита
        /// </summary>
        /// <param name="logger">Логгер</param>
        /// <param name="description">Описание операции</param>
        /// <param name="limit">Лимит выполнения</param>
        /// <param name="logBegining">Логировать начало операции</param>
        /// <param name="args">Аргументы операции, которые следует залогировать</param>
        /// <param name="shouldLog">
        /// Следует ли логировать операцию. Признаком можно осуществлять выборочное логирование потока
        /// данных.
        /// </param>
        /// <example>
        /// Пример использования
        /// <code>
        /// using (logger.BeginTimedOperation("Остановка потока на 2 сек."))
        /// {
        ///  	Thread.Sleep(2000);
        /// }
        /// </code>
        /// </example>
        public static IDisposable BeginTimedOperation(this ILogger logger, string description, TimeSpan? limit = null,
            bool logBegining = true, object args = null, bool shouldLog = true)
        {
            return new TimedOperation(logger, description, limit, logBegining, args, shouldLog);
        }

        public static IDisposable WithContextProperty(this ILogger logger, string property, object propertyValue)
        {
            return LogContext.PushProperty(property, propertyValue);
        }
    }

    public static class ContextExtensions
    {
        private static readonly string LoggerKey = "LoggerKey";
        private static readonly string ExecutionArgsKey = "TimedOperation.ExecutionArgsKey";
        private static readonly string ShouldLogBeginingKey = "TimedOperation.ShouldLogBeginingKey";
        private static readonly string ShouldLogKey = "TimedOperation.ShouldLogKey";

        public static Context WithLogger(this Context context, ILogger logger)
        {
            context[LoggerKey] = logger;
            return context;
        }

        public static Context WithArgs(this Context context, object args)
        {
            context[ExecutionArgsKey] = ToJson(args);
            return context;
        }

        public static object GetArgs(this Context context)
        {
            if (context.TryGetValue(ExecutionArgsKey, out var args))
            {
                return args;
            }

            return null;
        }

        /// <summary>
        /// Нужно ли логировать начало операции, если нет то залогируется только окончание
        /// </summary>
        public static Context WithShouldLogBegining(this Context context, bool shouldLogBegining)
        {
            context[ShouldLogBeginingKey] = shouldLogBegining;
            return context;
        }

        public static bool? GetShouldLogBegining(this Context context)
        {
            return context.ContainsKey(ShouldLogBeginingKey) ? (bool) context[ShouldLogBeginingKey] : (bool?) null;
        }

        /// <summary>
        /// Следует ли логировать данную операцию
        /// </summary>
        public static Context WithShouldLog(this Context context, bool shouldLog)
        {
            context[ShouldLogKey] = shouldLog;
            return context;
        }

        public static bool? GetShouldLog(this Context context)
        {
            return context.ContainsKey(ShouldLogKey) ? (bool) context[ShouldLogKey] : (bool?) null;
        }

        public static ILogger GetLogger(this Context context)
        {
            if (context.TryGetValue(LoggerKey, out var logger))
            {
                return logger as ILogger;
            }

            return null;
        }

        private static string ToJson(object obj, bool ignoreLoopHandling = false)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling =
                    ignoreLoopHandling ? ReferenceLoopHandling.Serialize : ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(obj, settings);
        }
    }

    /// <summary>
    /// Логирует время выполнения операции
    /// </summary>
    public class OperationLoggingPolicy : AsyncPolicy
    {
        private readonly ILogger _logger;
        private readonly string _operationName;
        private readonly TimeSpan? _timeout;

        public OperationLoggingPolicy(string operationName, ILogger logger, TimeSpan? timeout)
        {
            if (operationName == "")
            {
                throw new ArgumentException("operationName должен быть указан или равняться null");
            }

            _operationName = operationName;
            _logger = logger;
            _timeout = timeout;
        }

        protected override async Task<TResult> ImplementationAsync<TResult>(
            Func<Context, CancellationToken, Task<TResult>> action, Context context,
            CancellationToken cancellationToken, bool continueOnCapturedContext)
        {
            var opName = _operationName ?? context.OperationKey ?? context.PolicyKey;
            using (_logger.BeginTimedOperation(opName, args: context.GetArgs(), limit: _timeout,
                shouldLog: context.GetShouldLog() ?? true, logBegining: context.GetShouldLogBegining() ?? true))
            {
                return await action(context, cancellationToken);
            }
        }
    }
}