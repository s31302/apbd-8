using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientTripDTO>> GetClientTrips(int id);
    
    Task<bool> TripsExist(int id);
    Task<bool> ClientExist(int id);
    
}