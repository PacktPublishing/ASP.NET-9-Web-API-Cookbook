namespace books.Options;

public class DatabasePerformanceOptions
{
        public int QueryTimeoutThreshold { get; set; } = 1000; 
        public int DegradedThreshold { get; set; } = 500; 
        public string TestQuery { get; set; } = "SELECT COUNT(*) FROM Books";
}
