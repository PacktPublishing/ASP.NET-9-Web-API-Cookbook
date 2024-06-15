using Dapper;
using events.Models;
using Microsoft.Data.Sqlite;

namespace events.Repositories
{
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
                var query = @"
                    SELECT *
                    FROM EventRegistrations
                    WHERE Id > @LastId
                    ORDER BY Id
                    LIMIT @PageSize + 1";

                var result = (await connection.QueryAsync<EventRegistration>(query, new { LastId = lastId, PageSize = pageSize })).ToList();

                var items = result.Take(pageSize).ToList().AsReadOnly();
                var hasNextPage = result.Count > pageSize;

                return (items, hasNextPage);
            }
        }

        public async Task<EventRegistration?> GetEventRegistrationByIdAsync(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                var query = "SELECT * FROM EventRegistrations WHERE Id = @Id";
                return await connection.QueryFirstOrDefaultAsync<EventRegistration>(query, new { Id = id });
            }
        }

        public async Task<EventRegistration> CreateEventRegistrationAsync(EventRegistration eventRegistration)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                var query = @"
                    INSERT INTO EventRegistrations (GUID, FullName, Email, EventName, EventDate, DaysAttending, Notes, PhoneNumber, Address)
                    VALUES (@GUID, @FullName, @Email, @EventName, @EventDate, @DaysAttending, @Notes, @PhoneNumber, @Address);
                    SELECT last_insert_rowid();";

                var id = await connection.ExecuteScalarAsync<int>(query, new
                {
                    eventRegistration.GUID,
                    eventRegistration.FullName,
                    eventRegistration.Email,
                    eventRegistration.EventName,
                    eventRegistration.EventDate,
                    eventRegistration.DaysAttending,
                    eventRegistration.Notes,
                    eventRegistration.AdditionalContact.PhoneNumber,
                    eventRegistration.AdditionalContact.Address
                });

                eventRegistration.Id = id;
                return eventRegistration;
            }
        }

        public async Task UpdateEventRegistrationAsync(EventRegistration eventRegistration)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                var query = @"
                    UPDATE EventRegistrations
                    SET FullName = @FullName,
                        Email = @Email,
                        EventName = @EventName,
                        EventDate = @EventDate,
                        DaysAttending = @DaysAttending,
                        Notes = @Notes,
                        PhoneNumber = @PhoneNumber,
                        Address = @Address
                    WHERE Id = @Id";

                await connection.ExecuteAsync(query, new
                {
                    eventRegistration.FullName,
                    eventRegistration.Email,
                    eventRegistration.EventName,
                    eventRegistration.EventDate,
                    eventRegistration.DaysAttending,
                    eventRegistration.Notes,
                    eventRegistration.AdditionalContact.PhoneNumber,
                    eventRegistration.AdditionalContact.Address,
                    eventRegistration.Id
                });
            }
        }
    }
}
