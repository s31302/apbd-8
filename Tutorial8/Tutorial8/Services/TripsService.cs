using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Server=localhost\\SQLEXPRESS;Database=baza_danych;Trusted_Connection=True;TrustServerCertificate=True;";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new Dictionary<int, TripDTO>();

        string command = "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName FROM Trip t JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip JOIN Country c ON c.IdCountry = ct.IdCountry ";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal = reader.GetInt32(reader.GetOrdinal("IdTrip"));

                    if (!trips.ContainsKey(idOrdinal))
                    {
                        trips[idOrdinal] = new TripDTO()
                        {
                            Id = idOrdinal,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            Countries = new List<CountryDTO>()
                        };
                    }
                    var countryName = reader.GetString(reader.GetOrdinal("CountryName"));
                    trips[idOrdinal].Countries.Add(new CountryDTO { Name = countryName });
                }
                
            }
        }
        return trips.Values.ToList();
    }
}