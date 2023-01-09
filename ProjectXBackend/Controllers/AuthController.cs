using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectXBackend.DTOs;
using ProjectXBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ProjectXBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ApiDbContext dbContext;

        public AuthController(IConfiguration configuration, ApiDbContext dbContext)
        {
            this.configuration = configuration;
            this.dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Account>> Register(AccountDTO request)
        {
            // Checks if the username input already exists in the database.
            if (dbContext.Accounts.Any(a => a.Username == request.Username))
            {
                return BadRequest("Account already exists.");
            }

            // Takes the password input, turns it into encrypted Hash and Salt.
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var account = new Account
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            dbContext.Accounts.Add(account);
            await dbContext.SaveChangesAsync();

            return Ok("User successfully created.");
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(AccountDTO request)
        {
            var account = await dbContext.Accounts.FirstOrDefaultAsync(u => u.Username == request.Username);

            // Check if account with the inputted username exists
            if (account is null)
            {
                return BadRequest("User not found.");
            }

            // Check if password is correct
            if (!VerifyPasswordHash(request.Password, account.PasswordHash, account.PasswordSalt))
            {
                return BadRequest("Incorrect Password.");
            }

            // Generates a JWT token
            string token = CreateToken(account);

            // Generates a refreshToken
            var refreshToken = GetRefreshToken();

            // Sets the refreshToken in browser's Cookies
            var newRefreshToken = SetRefreshToken(refreshToken);

            account.RefreshToken = newRefreshToken.Token;
            account.TokenCreated = newRefreshToken.Created;
            account.TokenExpires = newRefreshToken.Expires;

            await dbContext.SaveChangesAsync();

            return Ok(token);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            // This will mostly end up being null when a token has expired causing the first if statement to be hit instead of the second.
            // Most browsers clear expired cookies, Firefox doesn't but the line below still results in null. Idk wat do.
            var refreshToken = Request.Cookies["refreshToken"];

            var account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.RefreshToken == refreshToken);

            if (account is null)
            {
                return Unauthorized("Invalid Refresh Token.");
            }
            else if (account.TokenExpires < DateTime.Now || refreshToken is null)
            {
                return Unauthorized("Token expired.");
            }

            // Creates a new JWT token with an updated expiration time.
            string token = CreateToken(account);

            // Generates a new RefreshToken
            var newRefreshToken = GetRefreshToken();

            account.RefreshToken = newRefreshToken.Token;
            account.TokenCreated = newRefreshToken.Created;
            account.TokenExpires = newRefreshToken.Expires;

            await dbContext.SaveChangesAsync();

            // Replaces the existing refreshToken in Cookies with a new one and a new expiration time.
            SetRefreshToken(newRefreshToken);

            return Ok(token);
        }

        private RefreshToken GetRefreshToken()
        {
            // Creates a Refresh Token, keep 'Expires' longer than the JWT 'expires' in CreateToken()
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddHours(1),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private RefreshToken SetRefreshToken(RefreshToken newRefreshToken)
        {
            // Sets the refresh token in browser's cookies
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            return newRefreshToken;
        }

        private string CreateToken(Account account)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, account.Username)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            // Creates the token, keep 'expires' shorter than GetRefreshToken()'s Expires
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

    }
}
