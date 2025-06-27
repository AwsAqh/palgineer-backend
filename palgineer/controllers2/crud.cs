using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using palgineer.services;
using palgineer.models2;
using palgineer.DTO;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;


namespace palgineer.controllers2

{
    [Route("api/[controller]")]
    [ApiController]
    public class CRUD : ControllerBase
    {
        private readonly EngineerService _engineerService;
        private readonly FileServices _fileServices;
        private readonly CloudinaryService _cloudinaryService;
        public CRUD(EngineerService engineerService, FileServices fileServices,CloudinaryService cloudinaryService) { 
        
            _cloudinaryService = cloudinaryService;
            _engineerService = engineerService;
            _fileServices= fileServices;
        }

        [HttpGet]
        public async Task<ActionResult<List<Engineer>>> GetAll()
        {
            var engineers = await _engineerService.GetAllAsync();
            return Ok(engineers);
        }


        [HttpGet("{id}")]
       
        public async Task<ActionResult<Engineer?>> GetEngineerById(string id)
        {
            var engineer = await _engineerService.GetByIdAsync(id);
            if (engineer == null) { return NotFound("No Engineer with that ID!"); }
            return Ok(engineer);

        }

        [Authorize]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdateEngineer(
    string id,
    [FromForm] UpdateEngineerDTO dto)
        {
            var callerId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (callerId != id)
                return BadRequest(new
                {
                    message = "Mismatched IDs",
                    tokenSub = callerId,
                    routeId = id
                });

            var existing = await _engineerService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            // update scalar fields if provided
            existing.name = dto.name ?? existing.name;
            existing.email = dto.email ?? existing.email;
            existing.status = dto.status ?? existing.status;
            existing.experience = dto.experience ?? existing.experience;
            existing.summary = dto.summary ?? existing.summary;
            existing.skills = dto.skills ?? existing.skills;
            existing.role = dto.role ?? existing.role;
            existing.links = dto.links ?? existing.links;

            // if a new avatar file arrived, upload to Cloudinary under palgineer/{id}/avatars
            if (dto.avatar?.Length > 0)
            {
                existing.avatar =
                    await _cloudinaryService
                        .UploadImageAsync(dto.avatar, id);
            }

            // if a new resume file arrived, upload to Cloudinary under palgineer/{id}/documents
            if (dto.resume?.Length > 0)
            {
                existing.resume =
                    await _cloudinaryService
                        .UploadDocumentAsync(dto.resume, id);
            }

            await _engineerService.UpdateEngineerAsync(existing, id);
            var updated = await _engineerService.GetByIdAsync(id);

            return Ok(new
            {
                message = "Engineer updated successfully",
                updated
            });
        }




        // DELETE: api/crud/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var existing = await _engineerService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            await _engineerService.RemoveEngineerAsync(id);
            return NoContent();
        }

    }
}
