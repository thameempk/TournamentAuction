using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TournamentAuction.Application.Services;
using TournamentAuction.Domain.Entities;
using TournamentAuction.Domain.Interfaces;

namespace TournamentAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IUserSubscriptionRepository _subscriptionRepository;
    private readonly ISubscriptionPlanRepository _planRepository;

    public AuthController(
        AuthService authService, 
        IConfiguration configuration, 
        IUserRepository userRepository,
        IUserSubscriptionRepository subscriptionRepository,
        ISubscriptionPlanRepository planRepository)
    {
        _authService = authService;
        _configuration = configuration;
        _userRepository = userRepository;
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _authService.LoginAsync(request.Email, request.Password);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var token = GenerateJwtToken(user);
        return Ok(new
        {
            token,
            user = new
            {
                userId = user.UserId,
                name = user.Name,
                email = user.Email,
                userType = user.UserType
            }
        });
    }

    [HttpPost("login/team")]
    public async Task<IActionResult> LoginWithTeamKey([FromBody] TeamLoginRequest request)
    {
        var team = await _authService.LoginWithTeamKeyAsync(request.TeamKey);
        if (team == null)
            return Unauthorized(new { message = "Invalid team key" });

        var token = GenerateTeamToken(team);
        return Ok(new
        {
            token,
            team = new
            {
                teamId = team.TeamId,
                teamName = team.TeamName,
                tournamentId = team.TournamentId,
                wallet = team.Wallet
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validate subscription plan if provided
            if (request.PlanId.HasValue)
            {
                var plan = await _planRepository.GetByIdAsync(request.PlanId.Value);
                if (plan == null || !plan.IsActive)
                {
                    return BadRequest(new { message = "Invalid or inactive subscription plan" });
                }
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                PasswordHash = request.Password, // Will be hashed in repository
                UserType = request.UserType ?? "TournamentAdmin",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _userRepository.CreateAsync(user);

            // Create subscription if plan is provided
            if (request.PlanId.HasValue)
            {
                var plan = await _planRepository.GetByIdAsync(request.PlanId.Value);
                if (plan != null)
                {
                    var subscription = new UserSubscription
                    {
                        SubscriptionId = Guid.NewGuid(),
                        UserId = created.UserId,
                        PlanId = plan.PlanId,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddDays(plan.DurationInDays),
                        Status = "Active",
                        AmountPaid = plan.Price,
                        PaymentReference = request.PaymentReference,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _subscriptionRepository.CreateAsync(subscription);
                }
            }

            return Ok(new
            {
                message = "User created successfully",
                user = new
                {
                    userId = created.UserId,
                    name = created.Name,
                    email = created.Email,
                    userType = created.UserType
                }
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGeneration12345");
        var issuer = _configuration["Jwt:Issuer"] ?? "TournamentAuctionSystem";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.UserType)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = issuer,
            Audience = issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateTeamToken(Team team)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGeneration12345");
        var issuer = _configuration["Jwt:Issuer"] ?? "TournamentAuctionSystem";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, team.TeamId.ToString()),
            new Claim(ClaimTypes.Name, team.TeamName),
            new Claim(ClaimTypes.Role, "Team"),
            new Claim("TournamentId", team.TournamentId.ToString()),
            new Claim("Wallet", team.Wallet?.ToString() ?? "0")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = issuer,
            Audience = issuer,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class TeamLoginRequest
{
    public string TeamKey { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? UserType { get; set; } = "TournamentAdmin";
    public Guid? PlanId { get; set; }
    public string? PaymentReference { get; set; }
}
