using Cars.Data;
using Cars.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace Cars.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        public UserController(IConfiguration configuration,
            IUnitOfWork unitOfWork, IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        #region HTTP Methods

        [HttpPost]
        [Route("register")]

        public async Task<ActionResult> Register(User user)
        {
            var userExistFlag = _unitOfWork.GetRepository<User>().GetAll().Result.Where(x => x.Email == user.Email).SingleOrDefault();
            if (userExistFlag != null)
            {
                return Ok("User Already Exist");
            }
            var passwordHasher = new PasswordHasher<User>();
            var hashedPassword = passwordHasher.HashPassword(user, user.PasswordHash);

            user.PasswordHash = hashedPassword;
            await _unitOfWork.GetRepository<User>().Add(user);
            await _unitOfWork.SaveChangesAsync();
            return Ok(HttpStatusCode.Created);
        }
       
        [HttpGet("current")]
        public IActionResult GetCurrentLoggedInUser()
        {
            // Retrieve the token from the Authorization header
            var token = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
            {
                return BadRequest("Token is missing");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return BadRequest("Invalid token");
            }

            // Extract user details from the token's claims
            var userId = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "UserId")?.Value;
            var Email = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Email)?.Value;

            // You can access other user-related claims as needed

            // Return the user information
            return Ok(new { Email = Email, UserId = userId });
        }
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok();


        }
        #endregion
    }
}
