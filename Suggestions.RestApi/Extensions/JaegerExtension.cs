using FluentValidation;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Jaeger.Senders.Thrift;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing;
using OpenTracing.Util;

namespace Suggestions.RestApi.Extensions
{
    public static class JaegerExtension
    {
        public static IServiceCollection AddJaeger(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection("Jaeger").Get<JaegerOptions>();
            if (!options.UseJaeger)
            {
                return services;
            }

            new JaegerOptionsValidator().Validate(options);

            services.AddSingleton<ITracer>(serviceProvider =>
            {
                string serviceName = options.ServiceName;

                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                ISampler sampler = new ConstSampler(sample: true);

                var senderResolver = new SenderResolver(loggerFactory)
                    .RegisterSenderFactory<ThriftSenderFactory>();

                var udpSender = new Configuration.SenderConfiguration(loggerFactory)
                    .WithAgentHost(options.AgentHost)
                    .WithAgentPort(options.AgentPort)
                    .WithSenderResolver(senderResolver)
                    .GetSender();

                var reporter = new RemoteReporter.Builder()
                    .WithSender(udpSender)
                    .Build();

                ITracer tracer = new Tracer.Builder(serviceName)
                    .WithReporter(reporter)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(sampler)
                    .Build();

                if (GlobalTracer.IsRegistered() == false)
                    GlobalTracer.Register(tracer);

                return tracer;
            });

            return services;
        }
    }

    public class JaegerOptionsValidator : AbstractValidator<JaegerOptions>
    {
        public JaegerOptionsValidator()
        {
            RuleFor(x => x.ServiceName).NotEmpty();
            RuleFor(x => x.AgentHost).NotEmpty();
            RuleFor(x => x.AgentPort).GreaterThan(0);
        }
    }

    public class JaegerOptions
    {
        public bool UseJaeger { get; set; }

        public string ServiceName { get; set; }

        public string AgentHost { get; set; }

        public int AgentPort { get; set; }
    }
}
