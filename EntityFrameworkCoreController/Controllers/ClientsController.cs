using EntityFrameworkCoreController.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EntityFrameworkCoreController.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly MasterContext _context;
        
        public ClientsController(MasterContext context)
        {
            _context = context;
        }

        [HttpDelete("{idClient}")]
        public async Task<IActionResult> DeleteClient(int idClient)
        {
            var client = await _context.Clients.FindAsync(idClient);

            if (client == null)
            {
                return StatusCode(404, "No client with such id");
            }
            
            var anyTrips = _context.ClientTrips.Any(trip => trip.IdClient == client.IdClient);

            if (anyTrips)
            {
                return StatusCode(404, "Cannot remove a client with assigned trips");
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return Ok("Client " + idClient + " successfully removed");
        }
    }
}