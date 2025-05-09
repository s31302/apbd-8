using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<List<ClientTripDTO>> GetClientTrips(int id);
    
    Task<bool> TripsExist(int id);
    Task<bool> ClientExist(int id);
    
    Task<int?> NewClient(ClientDTO client);
    
    Task<bool> MaxPeople(int id);
    
    Task<bool> RegisterClientToTrip(int clientId, int tripId);
    
    Task<bool> ClientOnTripExist(int clientId, int tripId);
    
    Task<bool> DeleteReservation(int clientId, int tripId);
    Task<bool> ClientHasTrips(int id);
}