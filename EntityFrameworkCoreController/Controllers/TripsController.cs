using System.Linq;
using EntityFrameworkCoreController.Models;
using EntityFrameworkCoreController.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCoreController.Controllers

{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly MasterContext _context;

        public TripsController(MasterContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            try
            {
                var trips = await _context.Trips
                    .OrderByDescending(trip => trip.DateFrom)
                    .Select(t => new
                    {
                        t.Name,
                        t.Description,
                        t.DateFrom,
                        t.DateTo,
                        t.MaxPeople,
                        Countries = t.IdCountries.Select(country => new { country.Name }),
                        Clients = t.ClientTrips.Select(clientTrip => new { clientTrip.IdClientNavigation.FirstName, clientTrip.IdClientNavigation.LastName })
                    })
                    .ToListAsync();

                return Ok(trips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing the request");
            }
        }


        [HttpPost ("{idTrip}/clients")]
        public async Task<IActionResult> AssignClientToTrip(int idTrip, [FromBody] AssignClientToTripDto assignClientToTripDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                var doesPeselExist = await _context.Clients.AnyAsync(c => c.Pesel == assignClientToTripDto.Pesel);
                
                // the task says that if the PESEL doesn't yet exist in our system we should add "it"(assuming client) to our database. This action is described
                // before the next steps of validation so that's what I will do
                Client incomingClient;
                
                if (!doesPeselExist)
                {
                    incomingClient = new Client
                    {
                        IdClient = _context.Clients.Max(c => c.IdClient) + 1,
                        FirstName = assignClientToTripDto.FirstName,
                        LastName = assignClientToTripDto.LastName,
                        Email = assignClientToTripDto.Email,
                        Telephone = assignClientToTripDto.Telephone,
                        Pesel = assignClientToTripDto.Pesel
                    };
                    await _context.Clients.AddAsync(incomingClient);

                    await _context.SaveChangesAsync();
                }
                else
                {
                    incomingClient = await _context.Clients.FirstAsync(c => c.Pesel == assignClientToTripDto.Pesel);
                }

                
                var doesTripExist = await _context.Trips.AnyAsync(t => t.IdTrip == assignClientToTripDto.IdTrip);

                if (!doesTripExist)
                {
                    return StatusCode(400, "The selected trip doesn't exist in the database");
                }

                var isClientSignedUp = await _context.ClientTrips.AnyAsync(ct =>
                    ct.IdClient == incomingClient.IdClient && ct.IdTrip == assignClientToTripDto.IdTrip);

                if (isClientSignedUp)
                {
                    return StatusCode(409, "This client is already signed up for that trip");
                }

                await _context.ClientTrips.AddAsync(new ClientTrip
                {
                    IdClient = incomingClient.IdClient,
                    IdTrip = assignClientToTripDto.IdTrip,
                    RegisteredAt = DateTime.Now,
                    PaymentDate = assignClientToTripDto.PaymentDate
                });
                await _context.SaveChangesAsync();

                return Ok("Client successfully added to the trip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while processing the request");
            }
        }
    }
}