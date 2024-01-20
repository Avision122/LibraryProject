namespace Projekt_studia2.Services
{
    public class LoggingService
    {
        private readonly LibraryContext _context;
        private readonly string _logFilePath;

        public LoggingService(LibraryContext context, IWebHostEnvironment env)
        {
            _context = context;
            _logFilePath = Path.Combine(env.ContentRootPath, "logs.txt");
        }

        public async Task LogEvent(string user, string action)
        {
            var logEvent = new LogEvent
            {
                Date = DateTime.Now,
                User = user,
                Action = action
            };

            _context.LogEvents.Add(logEvent);
            await _context.SaveChangesAsync();

            var logMessage = $"{logEvent.Date}: {logEvent.User} - {logEvent.Action}\n";
            await File.AppendAllTextAsync(_logFilePath, logMessage);
        }
    }
}
