using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;
using TournamentAuction.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace TournamentAuction.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Dapper Context
        services.AddSingleton<DapperContext>();

        // Register EF Core DbContext (optional, for migrations)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Repositories
        services.AddScoped<ITournamentRepository, TournamentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IPlayerCategoryRepository, PlayerCategoryRepository>();
        services.AddScoped<IAuctionRepository, AuctionRepository>();
        services.AddScoped<IBidRepository, BidRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IPointsTableRepository, PointsTableRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();

        return services;
    }
}

