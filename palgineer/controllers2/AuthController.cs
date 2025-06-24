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
using MongoDB.Bson;

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
            var newEngineer = new Engineer
            {
                _id = ObjectId.GenerateNewId().ToString(),
                name = dto.name,
                email = dto.email,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.password),
                summary = dto.summary,
                skills = dto.skills,
                links = dto.links,
                role=dto.role,
                status=dto.status,
                experience=dto.experience,
               
            };

            
            if (dto.avatar?.Length > 0)
                newEngineer.avatar = await _fileServices.saveFileAsync(newEngineer._id, dto.avatar);

            if (dto.resume?.Length > 0)
                newEngineer.resume = await _fileServices.saveFileAsync(newEngineer._id, dto.resume);

            await _engineerService.AddEngineerAsync(newEngineer);


            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, newEngineer._id!),
        new Claim(JwtRegisteredClaimNames.Email, newEngineer.email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            
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
                    new Claim(JwtRegisteredClaimNames.Sub, newEngineer._id!),
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

            
            return Ok(new AuthResponseDTO
            {
                Token = jwt,
                Expires = expires,
                Engineer = newEngineer
            });

        }

    }
}
