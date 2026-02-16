using Microsoft.Extensions.DependencyInjection;
using TournamentAuction.Application.Services;

namespace TournamentAuction.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<TournamentService>();
        services.AddScoped<AuctionService>();
        services.AddScoped<MatchService>();

        return services;
    }
}
