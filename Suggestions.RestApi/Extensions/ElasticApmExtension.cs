using Elastic.Apm.AspNetCore;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Suggestions.RestApi.Extensions
{
    public static class ElasticApmExtension
    {
        public static void UseApm(this IApplicationBuilder app, IConfiguration configuration)
        {
            var options = configuration.GetSection("ElasticApm").Get<ElasticApmOptions>();
            if (!options.UseApm)
            {
                return;
            }

            new ElasticApmOptionsValidator().Validate(options);

            app.UseElasticApm(configuration);
        }
    }

    public class ElasticApmOptionsValidator : AbstractValidator<ElasticApmOptions>
    {
        public ElasticApmOptionsValidator()
        {
            RuleFor(x => x.ServerUrls).NotEmpty();
            RuleFor(x => x.TransactionSampleRate).GreaterThanOrEqualTo(1);
        }
    }

    public class ElasticApmOptions
    {
        public bool UseApm { get; set; }

        public string ServerUrls { get; set; }

        public double TransactionSampleRate { get; set; }
    }
}
