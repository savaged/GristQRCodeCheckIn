using GristCheckIn.Core.CheckIn;
using GristCheckIn.Core.Configuration;
using GristCheckIn.Core.Grist;
using Microsoft.Extensions.DependencyInjection;

namespace GristCheckIn.Core.DependencyInjection;

/// <summary>
/// Wires up GristCheckIn.Core's services against Microsoft.Extensions.DependencyInjection.
/// This is the one place that knows how the concrete classes map to their
/// interfaces - callers (console app, WPF app, tests) just call
/// AddGristCheckIn and then resolve ICheckInService.
///
/// Deliberately does NOT register IDaySelector - that's UI-specific, so each
/// front-end registers its own implementation (e.g. services.AddSingleton
/// &lt;IDaySelector, ConsoleDaySelector&gt;()) alongside this call.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGristCheckIn(
        this IServiceCollection services,
        GristConfig config,
        IReadOnlyDictionary<DayOfWeek, string> dayColumns)
    {
        services.AddSingleton(config);
        services.AddSingleton<IDayColumnResolver>(new DayColumnResolver(dayColumns));

        // Typed HttpClient: IHttpClientFactory creates and pools the HttpClient,
        // and this delegate configures its BaseAddress/auth header once, up front,
        // using the GristConfig registered above.
        services.AddHttpClient<IGristClient, GristClient>((serviceProvider, client) =>
        {
            var gristConfig = serviceProvider.GetRequiredService<GristConfig>();
            client.BaseAddress = new Uri(gristConfig.ServerUrl);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", gristConfig.ApiKey);
        });

        services.AddScoped<ICheckInService, CheckInService>();

        return services;
    }
}
