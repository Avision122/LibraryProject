namespace Projekt_studia2
{
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class QueryTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _filePath;

        public QueryTimingMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _filePath = Path.Combine(env.ContentRootPath, "QueryTime.txt");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var watch = Stopwatch.StartNew();

            await _next(context);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            using (var streamWriter = new StreamWriter(_filePath, true))
            {
                await streamWriter.WriteLineAsync($"{context.Request.Path} took {elapsedMs} ms");
            }
        }
    }

}
