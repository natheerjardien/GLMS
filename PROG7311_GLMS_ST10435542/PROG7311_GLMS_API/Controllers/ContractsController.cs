using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_API.Data;
using PROG7311_GLMS_API.Models;
using PROG7311_GLMS_API.Services;

namespace PROG7311_GLMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IContractFactory _contractFactory;
        private readonly IContractStateManager _stateManager;

        public ContractsController(ApplicationDbContext context, IContractFactory contractFactory, IContractStateManager stateManager)
        {
            _context = context;
            _contractFactory = contractFactory;
            _stateManager = stateManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetAll()
        {
            return await _context.Contracts.Include(c => c.Client).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contract>> GetById(int id)
        {
            var contract = await _context.Contracts.Include(c => c.Client).FirstOrDefaultAsync(c => c.Id == id);
            
            if (contract == null)
            {
                return NotFound();
            }
            
            return contract;
        }

        [HttpPost]
        public async Task<ActionResult<Contract>> Create(Contract contract)
        {
            var newContract = _contractFactory.CreateContract(
                (int)contract.ClientId,
                contract.ServiceLevel,
                (DateTime)contract.StartDate,
                (DateTime)contract.EndDate
            );
            
            newContract.SignedAgreementFilePath = contract.SignedAgreementFilePath;

            _context.Contracts.Add(newContract);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetById), new { id = newContract.Id }, newContract);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Contract contract)
        {
            if (id != contract.Id)
            {
                return BadRequest();
            }
            
            _context.Entry(contract).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string action)
        {
            var contract = await _context.Contracts.FindAsync(id);
            
            if (contract == null)
            {
                return NotFound();
            }

            try
            {
                switch (action)
                {
                    case "Activate": 
                        _stateManager.ActivateContract(contract); 
                        break;
                    case "Expire": 
                        _stateManager.ExpireContract(contract); 
                        break;
                    case "Hold": 
                        _stateManager.PutOnHoldContract(contract); 
                        break;
                    default: 
                        return BadRequest("Invalid action");
                }

                await _context.SaveChangesAsync();
                return Ok(contract);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var contract = await _context.Contracts.Include(c => c.ServiceRequests).FirstOrDefaultAsync(c => c.Id == id);
            
            if (contract == null)
            {
                return NotFound();
            }
            
            if (contract.ServiceRequests.Any())
            {
                return BadRequest("Cannot delete contract with active Service Requests.");
            }

            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
    }
}