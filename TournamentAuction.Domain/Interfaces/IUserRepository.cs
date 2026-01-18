using TournamentAuction.Domain.Entities;

namespace TournamentAuction.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string email);
    Task<User?> GetByIdAsync(Guid userId);
    Task<User> CreateAsync(User user);
    Task<bool> ValidateCredentialsAsync(string email, string password);
}
