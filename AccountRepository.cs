using Core_Web_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using UserDTO = Core_Web_API.Models.UserDTO;
using UserModel = Core_Web_API.Models.UserModel;
using ValidationModel = Core_Web_API.Models.ValidationModel;

namespace Core_Web_API.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<UserModel> userManager;
        private readonly SignInManager<UserModel> signInManager;
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private string generatedToken = null;
        private const double EXPIRY_DURATION_MINUTES = 30;
        public AccountRepository(IConfiguration config, ITokenService tokenService, IUserRepository userRepository, UserManager<UserModel> userManager, SignInManager<UserModel> signInManager)
        {
            _config = config;
            _tokenService = tokenService;
            _userRepository = userRepository;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public async Task<ValidationModel> Login(UserModel userModel)
        {
            
            if (string.IsNullOrEmpty(userModel.Email) || string.IsNullOrEmpty(userModel.PasswordHash))
            {
                return new ValidationModel
                {
                    IsSuccess = false,
                };
            }
            var validUser = await userManager.FindByEmailAsync(userModel.Email);

            if (validUser != null)
            {
                if (await userManager.CheckPasswordAsync(validUser, userModel.PasswordHash) == false)
                {
                    return new ValidationModel
                    {
                        IsSuccess = false,
                    };
                }
                var result = await signInManager.PasswordSignInAsync(validUser.UserName, userModel.PasswordHash, false, true);

                if (result.Succeeded)
                {
                    var userroles = await userManager.GetRolesAsync(validUser);
                    string role = userroles.FirstOrDefault();
                    UserDTO currentUser = new UserDTO
                    {
                        UserName = validUser.UserName,
                        Password = userModel.PasswordHash,
                        Role = role,
                    };
                    await userManager.AddClaimAsync(validUser, new Claim("UserRole", role));
                    string key = _config["Jwt:Key"].ToString();
                    string issuer = _config["Jwt:Issuer"].ToString();
                    var claims = new[] {
                        new Claim(ClaimTypes.Name, currentUser.UserName),
                        new Claim(ClaimTypes.Role, currentUser.Role),
                        new Claim(ClaimTypes.NameIdentifier,
                        Guid.NewGuid().ToString())
                    };

                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
                    var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
                        expires: DateTime.Now.AddMinutes(EXPIRY_DURATION_MINUTES), signingCredentials: credentials);
                    generatedToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
                    //HttpContext.Session.SetString("Token", generatedToken);
                    //HttpContext.Session.SetString("userid", validUser.Id);
                    //HttpContext.Session.SetString("Username", validUser.UserName);
                    //HttpContext.Session.SetString("UserRole", role);
                    if (generatedToken != null)
                    {
                        
                        return new ValidationModel
                        {
                            Id = validUser.Id,
                            IsSuccess = true,
                            Username = validUser.UserName,
                            Role = role,
                            Token = generatedToken
                        };
                    }
                }
            }
            else
            {
                return new ValidationModel { IsSuccess = false };
            }
            return new ValidationModel { IsSuccess = false };
        }

        public async Task<ValidationModel> SignUp(Models.SignUpTemp model)
        {
            var userCheck = await userManager.FindByEmailAsync(model.Email);
            if (userCheck == null)
            {
                UserModel user = new UserModel
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    NormalizedEmail = model.Email,
                    NormalizedUserName = model.UserName,
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var result1 = await userManager.AddToRoleAsync(user, "User");
                    if (result1.Succeeded)
                    {
                        return new ValidationModel { IsSuccess = true, Username=model.UserName, Role="User" };
                    }
                    else
                    {
                        return new ValidationModel { IsSuccess=false };
                    }
                }
            }
            return new ValidationModel { IsSuccess = false };
        }

        
    }
}
