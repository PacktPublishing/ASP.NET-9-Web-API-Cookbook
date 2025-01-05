using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Books.Options;

namespace Books.Middleware;

public class DatabasePerformanceHealthCheck(
            IDbConnection dbConnection, 
            ILogger<DatabasePerformanceHealthCheck> logger,
            IOptionsMonitor<DatabasePerformanceOptions> options): IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var optionsSnapshot = options.Get(context.Registration.Name);
            var data = new Dictionary<string, object>();

            try
            {
                var stopwatch = Stopwatch.StartNew();
                
                dbConnection.Open();

                using var command = dbConnection.CreateCommand();
                command.CommandText = optionsSnapshot.TestQuery;
                command.CommandTimeout = optionsSnapshot.QueryTimeoutThreshold / 1000; 

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var recordCount = reader.GetInt32(0);
                    data.Add("RecordCount", recordCount);
                }

                stopwatch.Stop();
                var elapsed = stopwatch.ElapsedMilliseconds;

                data.Add("QueryExecutionTime", elapsed);
                data.Add("TestQuery", optionsSnapshot.TestQuery);

                if (elapsed < optionsSnapshot.DegradedThreshold)
                {
                    return Task.FromResult(HealthCheckResult.Healthy(
                        $"Database query completed in {elapsed}ms",
                        data));
                }
                else if (elapsed < optionsSnapshot.QueryTimeoutThreshold)
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
                logger.LogError(ex, "Database health check failed");
                data.Add("ExceptionMessage", ex.Message);
                data.Add("ExceptionStackTrace", ex.StackTrace!);
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Database query failed: {ex.Message}",
                    exception: ex,
                    data: data));
            }
            finally
            {
                if (dbConnection.State == ConnectionState.Open)
                {
                    dbConnection.Close();
                }
            }
        }
    }
