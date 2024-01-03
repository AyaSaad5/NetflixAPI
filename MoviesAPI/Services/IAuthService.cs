using MoviesAPI.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace MoviesAPI.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);

        Task<string> AddRoleAsync(AddRoleMode model);
        Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user);
    }
}
