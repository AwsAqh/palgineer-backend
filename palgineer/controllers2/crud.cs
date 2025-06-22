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
        public CRUD(EngineerService engineerService, FileServices fileServices) { 
        
            
            _engineerService = engineerService;
            _fileServices= fileServices;
        }

        [HttpGet]
        public async Task<ActionResult<List<Engineer>>> GetAll()
        {
            var engineers = await _engineerService.GetAllAsync();
            return Ok(engineers);
        }




        [Authorize]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdateEngineer(string id,[FromForm] UpdateEngineerDTO updatedEngineer)
        {



            var callerId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (callerId != id)
            {
                return BadRequest(new
                {
                    message = "Mismatched IDs",
                    tokenSub = callerId,
                    routeId = id
                });
            }

            // 2) perform the update
            var existing = await _engineerService.GetByIdAsync(id);
            if (existing == null)
                return NotFound();

            existing.name = updatedEngineer.name ??existing.name;
            existing.email = updatedEngineer.email?? existing.email;
            existing.status= updatedEngineer.status ?? existing.status;
            existing.experience= updatedEngineer.experience ?? existing.experience;
            existing.summary = updatedEngineer.summary ?? existing.summary ;
            existing.skills = updatedEngineer.skills ?? existing.skills;
            existing.role=updatedEngineer.role??existing.role;
            existing.links = updatedEngineer.links ?? existing.links;
            if(updatedEngineer.avatar?.Length>0)
            existing.avatar= await _fileServices.saveFileAsync( id,updatedEngineer.avatar);
            if(updatedEngineer.resume?.Length>0)
            existing.resume = await _fileServices.saveFileAsync(id,updatedEngineer.resume);

            await _engineerService.UpdateEngineerAsync(existing, id);
            var updated = await _engineerService.GetByIdAsync(id);
            return Ok(new
            {
                message="Engineer updated successfully",
                updated,
            }
                
                );
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
