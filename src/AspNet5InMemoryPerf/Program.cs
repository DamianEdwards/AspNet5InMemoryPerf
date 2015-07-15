using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Builder.Internal;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Internal;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Runtime;

namespace AspNet5InMemoryPerf
{
    public class Program
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IServiceProvider _serviceProvider;

        public Program(IServiceProvider serviceProvider, ILibraryManager libraryManager)
        {
            _serviceProvider = serviceProvider;
            _libraryManager = libraryManager;
        }

        public async Task Main(string[] args)
        {
            var app = new ApplicationBuilder(_serviceProvider);
            var payload = Encoding.UTF8.GetBytes("HelloWorld");
            app.Run(context =>
            {
                return context.Response.Body.WriteAsync(payload, 0, payload.Length);
            });

            var requestDelegate = app.Build();
#if DNX451
            Console.WriteLine($"Hit Enter to start test, PID {Process.GetCurrentProcess().Id}");
            Console.ReadLine();
#endif

            var requestCount = 5000000;
            var started = DateTime.UtcNow;

            var userAgent = new KeyValuePair<string, string[]>("User-Agent", new[] { "InlineTest" });
            
            for (int i = 0; i < requestCount; i++)
            {
                var httpContext = new DefaultHttpContext();
                //httpContext.Request.Method = "GET";
                httpContext.Request.Headers.Add(userAgent);
                //httpContext.Request.Headers.Add("Accept", new[] { "text/html;text/plain" });

                await requestDelegate(httpContext);
            }

            var elapsed = DateTime.UtcNow - started;

            Console.WriteLine($"Completed {requestCount} requests in {elapsed}");
            Console.WriteLine($"Requests/sec: {requestCount/elapsed.TotalSeconds}");

            Console.WriteLine("Hit Enter to force a GC");
            Console.ReadLine();

            Console.Write("GC started...");
            GC.Collect();
            Console.WriteLine("finished!");
            Console.WriteLine();

            Console.ReadLine();
        }
    }
}
