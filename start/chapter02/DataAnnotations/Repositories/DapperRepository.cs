using Dapper;
using Microsoft.Data.Sqlite;
using events.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
}
