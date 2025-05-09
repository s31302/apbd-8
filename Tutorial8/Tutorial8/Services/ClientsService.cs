using Tutorial8.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Server=localhost\\SQLEXPRESS;Database=baza_danych;Trusted_Connection=True;TrustServerCertificate=True;";


    public async Task<List<ClientTripDTO>> GetClientTrips(int id)
    {
        var trips = new Dictionary<int, ClientTripDTO>();

        string command = "SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.Name AS CountryName, clt.RegisteredAt, clt.PaymentDate FROM Client_Trip clt JOIN Trip t ON clt.IdTrip = t.IdTrip JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip JOIN Country c ON c.IdCountry = ct.IdCountry WHERE clt.IdClient = @id";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idOrdinal =  reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    if (!trips.ContainsKey(idOrdinal))
                    {
                        trips[idOrdinal] = new ClientTripDTO()
                        {
                            Id = idOrdinal,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            Countries = new List<CountryDTO>(),
                            RegisteredAt = reader.GetInt32(7),
                            PaymentDate = reader.IsDBNull(reader.GetOrdinal("PaymentDate")) ? null :  reader.GetInt32(reader.GetOrdinal("PaymentDate")),
                        };
                    }
                    var countryName = reader.GetString(reader.GetOrdinal("CountryName"));
                    trips[idOrdinal].Countries.Add(new CountryDTO { Name = countryName });
                }
            }
        }
        return trips.Values.ToList();
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

    public async Task<int?> NewClient(string firstName, string lastName, string email, string telephone,
        string pesel)
    {

        string command = "INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) OUTPUT INSERTED.IdClient VALUES (@name, @lastName, @email, @telephone, @pesel)";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@name", firstName);
            cmd.Parameters.AddWithValue("@lastName", lastName);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@telephone", telephone);
            cmd.Parameters.AddWithValue("@pesel", pesel);
            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : null;

        }
    }

    public async Task<bool> MaxPeople(int id)
    {
        string command = @"
        SELECT t.IdTrip
        FROM Trip t
        LEFT JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
        WHERE t.IdTrip = @id
        GROUP BY t.IdTrip, t.MaxPeople
        HAVING COUNT(ct.IdClient) < t.MaxPeople";  
        
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
    
    public async Task<bool> RegisterClientToTrip(int clientId, int tripId)
    {
        const string query = @"
        INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
        VALUES (@clientId, @tripId, @registeredAt)";
    
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@clientId", clientId);
        cmd.Parameters.AddWithValue("@tripId", tripId);
        cmd.Parameters.AddWithValue("@registeredAt", int.Parse(DateTime.Now.ToString("yyyyMMdd")));
    
        await conn.OpenAsync();
        var result = await cmd.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<bool> ClientOnTripExist(int clientId, int tripId)
    {
        string command = "SELECT IdClient FROM Client_Trip WHERE IdClient = @idClient and IdTrip = @idTrip";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@idClient", clientId);
            cmd.Parameters.AddWithValue("@idTrip", tripId);
            await conn.OpenAsync();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                return await reader.ReadAsync();
            }    
        }
    }

    public async Task<bool> DeleteReservation(int clientId, int tripId)
    {
        string query = "DELETE FROM Client_Trip WHERE IdClient = @idClient and IdTrip = @idTrip";
        using (SqlConnection connection = new SqlConnection(_connectionString))
        using (SqlCommand command = new SqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@idClient", clientId);
            command.Parameters.AddWithValue("@idTrip", tripId);
            await connection.OpenAsync();

            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }

}