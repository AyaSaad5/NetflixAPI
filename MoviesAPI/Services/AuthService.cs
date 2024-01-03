using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoviesAPI.Helpers;
using MoviesAPI.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MoviesAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManger;
        private readonly RoleManager<IdentityRole> _roleManger;
        private readonly IMapper _mapper;
        private readonly JWT _jwt;

        public AuthService(UserManager<ApplicationUser> userManger, RoleManager<IdentityRole> roleManger,
                           IMapper mapper,IOptions<JWT> jwt)
        {
            _userManger = userManger;
            _roleManger = roleManger;
            _mapper = mapper;
            _jwt = jwt.Value;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManger.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is Already Exists" };

            if (await _userManger.FindByNameAsync(model.UserName) is not null)
                return new AuthModel { Message = "Username is Already Used" };

            var user = _mapper.Map<RegisterModel, ApplicationUser>(model);

            var result = await _userManger.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description}, ";
                }
                return new AuthModel { Message = errors };
            }

            await _userManger.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                ExpiredOn = jwtSecurityToken.ValidTo,
                IsAuthentcated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                UserName = user.UserName
            };
        }

        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();
            var user =await _userManger.FindByEmailAsync(model.Email);

            if (user is null || !await _userManger.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }

            var roleList = await _userManger.GetRolesAsync(user);
            var jwtSecurityToken = await CreateJwtToken(user);

            authModel.Email = user.Email;
            authModel.ExpiredOn = jwtSecurityToken.ValidTo;
            authModel.IsAuthentcated = true;
            authModel.Roles = roleList.ToList();
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.UserName = user.UserName;

            return authModel;
        }

        public async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManger.GetClaimsAsync(user);
            var roles = await _userManger.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var roleClaim in roles)
            {
                roleClaims.Add(new Claim("roles", roleClaim));
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }.Union(userClaims).Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signCreditionals = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signCreditionals);

            return jwtSecurityToken;
        }

        public async Task<string> AddRoleAsync(AddRoleMode model)
        {
            var user = await _userManger.FindByIdAsync(model.UserId);
            if (user == null || !await _roleManger.RoleExistsAsync(model.Role))
                return "Invalid User Id or Role";

            if (await _userManger.IsInRoleAsync(user, model.Role))
                return "User already assigned to Role!";

            var result = await _userManger.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Something went wrong";

        }
    }
}
