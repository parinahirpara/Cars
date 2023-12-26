using Cars.Data;
using Cars.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cars.Controllers
{
    [ApiController]
    //[Route("api/[controller]")]
    public class LogInController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public LogInController(
            IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration; 
        }

        #region HTTP Methods
        [HttpPost]
		[Route("/api/Login")]
		public ActionResult Login([FromBody] Login login)
        {
            var result = Authenticate(login);
            return Ok(result);
        }

        #endregion

        #region Helper Methods
        private ActionResult Authenticate(Login login)
        {
            var user = _unitOfWork.GetRepository<User>().GetAll().Result.Where(x => x.UserName == login.Username).SingleOrDefault();
            var userExistFlag = _unitOfWork.GetRepository<User>().GetAll().Result.Where(x => x.UserName == login.Username).SingleOrDefault();

            if (user == null)
            {
                return Ok("Invalid Username and Password");
            }


            var passwordhasher = new PasswordHasher<User>();
            var password = passwordhasher.VerifyHashedPassword(new User(), user.PasswordHash, login.Password);

            if (password == PasswordVerificationResult.Failed)
            {
                return Ok("Invalid Username and Password");
            }
            var tokenString = GenerateJSONWebToken(user);
            var Token = new JwtSecurityTokenHandler().WriteToken(tokenString);

            return Ok(new { Token = Token });
        }
        private JwtSecurityToken GenerateJSONWebToken(User userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim("UserId", userInfo.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, userInfo.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["Jwt:ExpiryTime"])),
                signingCredentials: credentials);

            return token;
        }
       
        #endregion
    }
}
