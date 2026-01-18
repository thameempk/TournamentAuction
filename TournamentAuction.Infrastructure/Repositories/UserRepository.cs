using Dapper;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;
using TournamentAuction.Infrastructure.Data;
using BCrypt.Net;

namespace TournamentAuction.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string email)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Users WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        using var connection = _context.CreateConnection();
        var sql = "SELECT * FROM Users WHERE UserId = @UserId";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = userId });
    }

    public async Task<User> CreateAsync(User user)
    {
        using var connection = _context.CreateConnection();
        if (user.UserId == Guid.Empty)
        {
            user.UserId = Guid.NewGuid();
        }
        
        // Hash password before storing
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
        var sql = @"INSERT INTO Users (UserId, Name, Email, PasswordHash, UserType, CreatedAt, UpdatedAt)
                    VALUES (@UserId, @Name, @Email, @PasswordHash, @UserType, @CreatedAt, @UpdatedAt)";
        await connection.ExecuteAsync(sql, user);
        return user;
    }

    public async Task<bool> ValidateCredentialsAsync(string email, string password)
    {
        var user = await GetByUsernameAsync(email);
        if (user == null) return false;
        
        // Verify password using BCrypt
        return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }
}
