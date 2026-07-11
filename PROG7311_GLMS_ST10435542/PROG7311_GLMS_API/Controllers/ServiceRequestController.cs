using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_API.Data;
using PROG7311_GLMS_API.Models;

namespace PROG7311_GLMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceRequestsController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetAll() => 
            await _context.ServiceRequests.Include(s => s.Contract).ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequest>> GetById(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            return sr == null ? NotFound() : sr;
        }

        [HttpPost]
        public async Task<ActionResult<ServiceRequest>> Create(ServiceRequest sr)
        {
            _context.ServiceRequests.Add(sr);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = sr.Id }, sr);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ServiceRequest sr)
        {
            if (id != sr.Id) return BadRequest();
            _context.Entry(sr).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sr = await _context.ServiceRequests.FindAsync(id);
            if (sr == null) return NotFound();
            _context.ServiceRequests.Remove(sr);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}