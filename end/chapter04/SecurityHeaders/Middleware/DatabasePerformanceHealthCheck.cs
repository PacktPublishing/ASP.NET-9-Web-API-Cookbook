using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using books.Options;

namespace books.Middleware;

public class DatabasePerformanceHealthCheck : IHealthCheck
    {
        private readonly IDbConnection _dbConnection;
        private readonly ILogger<DatabasePerformanceHealthCheck> _logger;
        private readonly IOptionsMonitor<DatabasePerformanceOptions> _options;

        public DatabasePerformanceHealthCheck(
            IDbConnection dbConnection, 
            ILogger<DatabasePerformanceHealthCheck> logger,
            IOptionsMonitor<DatabasePerformanceOptions> options)
        {
            _dbConnection = dbConnection;
            _logger = logger;
            _options = options;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var options = _options.Get(context.Registration.Name);
            var data = new Dictionary<string, object>();

            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                _dbConnection.Open();

                using var command = _dbConnection.CreateCommand();
                command.CommandText = options.TestQuery;
                command.CommandTimeout = options.QueryTimeoutThreshold / 1000; 

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var recordCount = reader.GetInt32(0);
                    data.Add("RecordCount", recordCount);
                }

                stopwatch.Stop();
                var elapsed = stopwatch.ElapsedMilliseconds;

                data.Add("QueryExecutionTime", elapsed);
                data.Add("TestQuery", options.TestQuery);

                if (elapsed < options.DegradedThreshold)
                {
                    return Task.FromResult(HealthCheckResult.Healthy(
                        $"Database query completed in {elapsed}ms",
                        data));
                }
                else if (elapsed < options.QueryTimeoutThreshold)
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        $"Database query took {elapsed}ms, which is slower than expected",
                        null));
                }
                else
                {
                    return Task.FromResult(HealthCheckResult.Unhealthy(
                        $"Database query took {elapsed}ms, indicating severe performance issues",
                        null));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed");
                data.Add("ExceptionMessage", ex.Message);
                data.Add("ExceptionStackTrace", ex.StackTrace!);
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Database query failed: {ex.Message}",
                    exception: ex,
                    data: data));
            }
            finally
            {
                if (_dbConnection.State == ConnectionState.Open)
                {
                    _dbConnection.Close();
                }
            }
        }
    }
