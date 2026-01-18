using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITeamRepository _teamRepository;

    public AuthService(IUserRepository userRepository, ITeamRepository teamRepository)
    {
        _userRepository = userRepository;
        _teamRepository = teamRepository;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(email);
        if (user == null) return null;

        var isValid = await _userRepository.ValidateCredentialsAsync(email, password);
        return isValid ? user : null;
    }

    public async Task<Team?> LoginWithTeamKeyAsync(string teamKey)
    {
        return await _teamRepository.GetByTeamKeyAsync(teamKey);
    }
}

