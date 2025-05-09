using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;

        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }
        

        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetTrip(int id)
        {
            var clientExist = await _clientsService.ClientExist(id);
            if (!clientExist)
            {
                return NotFound("Client not found");
            }
            
            
            
            var trips = await _clientsService.GetClientTrips(id);
            
            return Ok(trips);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddNewClient([FromBody] ClientDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newClientId = await _clientsService.NewClient(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Telephone,
                request.Pesel
            );

            if (newClientId == null)
            {
                return BadRequest("Invalid input or failed to create client.");
            }
            //czm nie created wez poszukaj pozamienieaj
            return CreatedAtAction(nameof(AddNewClient), new { id = newClientId }, new { id = newClientId });
        }

        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            var clientExist = await _clientsService.ClientExist(id);
            if (!clientExist)
            {
                return NotFound("Client not found");
            }
            
            var tripsExist = await _clientsService.TripsExist(id);
            if (!tripsExist)
            {
                return NotFound("Trip not found");
            } 
            
            var underMaxPeople = await _clientsService.MaxPeople(tripId);
            if (!underMaxPeople)
            {
                return BadRequest("Amount of people is too big");
            }
            
            var success = await _clientsService.RegisterClientToTrip(id, tripId);
            if (!success)
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to register client to trip.");
            
            return Ok("Client successfully registered for the trip.");
        }

        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> DeleteClientForTrip(int id, int tripId)
        {
            var reservationExist = await _clientsService.ClientOnTripExist(id, tripId);
            if (!reservationExist)
            {
                return NotFound("Reservation not found");
            }
            
            var success = await _clientsService.DeleteReservation(id, tripId);
            if (!success)
            {
                //idk
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete client.");
            }
            
            return Ok("Client successfully deleted from the trip.");
            
        }
    }
}