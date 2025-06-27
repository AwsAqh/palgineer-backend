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
        private readonly CloudinaryService _cloudinaryService;
        private readonly FileServices _fileServices;


        public AuthController(IOptions<JwtSettings> jwtSettings, EngineerService engService, CloudinaryService cloudinaryService,FileServices fileServices)
        {

            _engineerService = engService;
            _jwtSettings = jwtSettings.Value;
            _cloudinaryService=cloudinaryService;
            _fileServices = fileServices;
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDTO>> RegisterEngineer([FromForm] EngineerFormDataDto dto)
        {
            // 1) Check for existing email
            if (await _engineerService.GetByEmail(dto.email) != null)
                return BadRequest("Email already registered");

            // 2) Create your domain object
            var newEngineer = new Engineer
            {
                _id = ObjectId.GenerateNewId().ToString(),
                name = dto.name,
                email = dto.email,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.password),
                summary = dto.summary,
                skills = dto.skills,
                links = dto.links,
                role = dto.role,
                status = dto.status,
                experience = dto.experience
            };

            // 3) If an avatar was uploaded, send it to Cloudinary and store the secure URL
            if (dto.avatar is { Length: > 0 })
            {
                newEngineer.avatar = await _cloudinaryService
                    .UploadImageAsync(dto.avatar, newEngineer._id);
            }

            // 4) If a resume was uploaded, send it as a “raw” file and store that URL
            if (dto.resume is { Length: > 0 })
            {
                newEngineer.resume = await _cloudinaryService
                    .UploadDocumentAsync(dto.resume, newEngineer._id);
            }

            // 5) Persist the new engineer (with cloud URLs for avatar & resume)
            await _engineerService.AddEngineerAsync(newEngineer);

            // 6) Issue JWT as before…
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub,   newEngineer._id!),
        new Claim(JwtRegisteredClaimNames.Email, newEngineer.email),
        new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
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

            // 7) Shape your response DTO
            var resEng = new EngineerResponseDTO
            {
                _id = newEngineer._id,
                name = newEngineer.name,
                email = newEngineer.email,
                skills = newEngineer.skills,
                links = newEngineer.links,
                role = newEngineer.role,
                status = newEngineer.status,
                experience = newEngineer.experience,
                summary = newEngineer.summary,
                avatar = newEngineer.avatar,   // <-- Cloudinary URL
                resume = newEngineer.resume    // <-- Cloudinary URL
            };

            return Ok(new AuthResponseDTO
            {
                Token = jwt,
                Expires = expires,
                Engineer = resEng
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

            
            EngineerResponseDTO resEng=new EngineerResponseDTO();
            resEng.name=newEngineer.name;
            resEng.email=newEngineer.email;
            resEng._id=newEngineer._id;
            resEng.skills=newEngineer.skills;
            resEng.links=newEngineer.links;
            resEng.role=newEngineer.role;
            resEng.status=newEngineer.status;
            resEng.avatar=newEngineer.avatar;
            resEng.resume=newEngineer.resume;
            resEng.experience=newEngineer.experience;
            resEng.summary=newEngineer.summary;

            return Ok(new AuthResponseDTO
            {
                Token = jwt,
                Expires = expires,
                Engineer = resEng
            });

        }

    }
}
