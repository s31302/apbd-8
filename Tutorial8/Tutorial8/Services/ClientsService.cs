using Tutorial8.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Server=localhost\\SQLEXPRESS;Database=baza_danych;Trusted_Connection=True;TrustServerCertificate=True;";


    public async Task<List<ClientTripDTO>> GetClientTrips(int id)
    {
        var trips = new List<ClientTripDTO>();

        string command = "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name, clt.RegisteredAt, clt.PaymentDate FROM Client_Trip clt JOIN Trip t ON clt.IdTrip = t.IdTrip JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip JOIN Country c ON c.IdCountry = ct.IdCountry WHERE clt.IdClient = @id";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetOrdinal("IdTrip");
                    trips.Add(new ClientTripDTO()
                    {
                        Id = reader.GetInt32(idOrdinal),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        DateFrom = reader.GetDateTime(3),
                        DateTo = reader.GetDateTime(4),
                        MaxPeople = reader.GetInt32(5),
                        Countries = new List<CountryDTO>()
                        {
                            new CountryDTO()
                            {
                                Name = reader.GetString(6)
                            }
                        },
                        RegisteredAt = reader.GetInt32(7),
                        PaymentDate = reader.GetInt32(8)
                        
                        
                        
                    });
                }
            }
        }
        var groupedTrips = trips
            .GroupBy(t => t.Id)
            .Select(g =>
            {
                var first = g.First();
                first.Countries = g.SelectMany(x => x.Countries).DistinctBy(c => c.Name).ToList();
                return first;
            })
            .ToList();

        return groupedTrips;
    }

    public async Task<bool> TripsExist(int id)
    {
        string command = "SELECT IdTrip FROM Client_Trip WHERE IdClient = @id";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                    return await reader.ReadAsync();
            }
        }
       
    }

    public async Task<bool> ClientExist(int id)
    {
        string command = "SELECT IdClient FROM Client WHERE IdClient = @id";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ReadAsync();
            }
        }
        
    }
}