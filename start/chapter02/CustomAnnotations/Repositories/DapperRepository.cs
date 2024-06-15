using Dapper;
using Microsoft.Data.Sqlite;
using events.Models;

namespace events.Repositories;

public class DapperRepository : IDapperRepository
{
    private readonly string _connectionString;

    public DapperRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<(IReadOnlyCollection<EventRegistration> Items, bool HasNextPage)> GetEventRegistrationsAsync(int pageSize, int lastId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();

            string sql = @"
                SELECT Id, GUID, FullName, Email,
                EventName, EventDate, DaysAttending, Notes 
                FROM EventRegistrations
                WHERE Id > @LastId
                ORDER BY Id
                LIMIT @PageSize + 1";  // Fetch one more record to determine HasNextPage

            var parameters = new { LastId = lastId, PageSize = pageSize };
            var result = await connection.QueryAsync<EventRegistration>(sql, parameters);

            var items = result.Take(pageSize).ToList().AsReadOnly();
            var hasNextPage = result.Count() > pageSize;

            return (items, hasNextPage);
        }
    }

    
    public async Task<EventRegistration?> GetEventRegistrationByIdAsync(int id)
    {
        using (var connection = new SqliteConnection(_connectionString)) {
            await connection.OpenAsync();

            string sql = 
            @"SELECT Id, GUID, FullName, Email, 
            EventName, EventDate, DaysAttending, Notes 
            FROM EventRegistrations 
            WHERE Id = @Id";

            var parameters = new { Id = id };
            return await connection.QueryFirstOrDefaultAsync<EventRegistration>(sql, parameters);
        }

    }

    public async Task<EventRegistration> CreateEventRegistrationAsync(EventRegistration eventRegistration)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO EventRegistrations (GUID, FullName, Email, EventName, EventDate, DaysAttending, Notes)
                VALUES (@GUID, @FullName, @Email, @EventName, @EventDate, @DaysAttending, @Notes);
                SELECT last_insert_rowid();";

            var parameters = new
            {
                GUID = eventRegistration.GUID,
                FullName = eventRegistration.FullName,
                Email = eventRegistration.Email,
                EventName = eventRegistration.EventName,
                EventDate = eventRegistration.EventDate,
                DaysAttending = eventRegistration.DaysAttending,
                Notes = eventRegistration.Notes
            };

            var id = await connection.QuerySingleAsync<int>(sql, parameters);

            eventRegistration.Id = id;

            return eventRegistration;
        }
    }
}
