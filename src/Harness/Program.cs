using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Harness
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder<Startup>(args)
                .Build();
    }
}
