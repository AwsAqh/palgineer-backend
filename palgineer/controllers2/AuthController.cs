using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using palgineer.DTO;
using palgineer.models2;
using palgineer.services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;



using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;

namespace palgineer.controllers2
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase

    {
        private readonly EngineerService _engineerService;
        private readonly JwtSettings _jwtSettings;
        
        private readonly FileServices _fileServices;


        public AuthController(IOptions<JwtSettings> jwtSettings, EngineerService engService, FileServices fileServices)
        {

            _engineerService = engService;
            _jwtSettings = jwtSettings.Value;
            
            _fileServices = fileServices;
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> RegisterEngineer([FromForm] EngineerFormDataDto dto)
        {

            if (await _engineerService.GetByEmail(dto.email) != null)
                return BadRequest("Email already registered");



            string avatarFileName = null;
            string resumeFileName = null;
            if (dto.avatar != null && dto.avatar.Length > 0)
            {
                avatarFileName = await _fileServices.saveFileAsync(dto.avatar);
            }


            if (dto.resume != null && dto.resume.Length > 0)
            {
                resumeFileName = await _fileServices.saveFileAsync(dto.resume);
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.password);

            var newEngineer = new Engineer
            {

                name = dto.name,
                email = dto.email,
                passwordHash = hashedPassword,
                summary = dto.summary,
                skills = dto.skills,
                links = dto.links,
                resume = resumeFileName,
                avatar = avatarFileName,

            };

            await _engineerService.AddEngineerAsync(newEngineer);

            
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, newEngineer.Id!),
        new Claim(JwtRegisteredClaimNames.Email, newEngineer.email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // 5) sign & issue token
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            
            return Ok(new AuthResponseDTO
            {
                Token = jwt,
                Expires = expires,
                Engineer = newEngineer
            });

        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>?> LoginEngineer([FromBody] LoginDTO dto) {

            Engineer newEngineer = await _engineerService.GetByEmail(dto.email);
            if (newEngineer == null) 
                return BadRequest(new {
                    message="Email not found"
                
                });

            if (!BCrypt.Net.BCrypt.Verify(dto.password, newEngineer.passwordHash))
                return BadRequest(new
                {
                    message = "Invalid password"

                });

            var claims = new[]
                 {
                    new Claim(JwtRegisteredClaimNames.Sub, newEngineer.Id!),
                    new Claim(JwtRegisteredClaimNames.Email, newEngineer.email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };
            // 4) sign token
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key),
                                                   SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(_jwtSettings.DurationInDays);
            var tokenDesc = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = creds,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDesc);
            var jwt = handler.WriteToken(token);

            // 5) return token + user
            return Ok(new AuthResponseDTO
            {
                Token = jwt,
                Expires = expires,
                Engineer = newEngineer
            });

        }

    }
}
