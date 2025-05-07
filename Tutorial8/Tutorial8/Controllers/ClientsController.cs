using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            
            var tripsExist = await _clientsService.TripsExist(id);
            if (!tripsExist)
            {
                return NotFound("Trip not found");
            }
            
            var trips = await _clientsService.GetClientTrips(id);
            
            return Ok(trips);
        }
    }
}