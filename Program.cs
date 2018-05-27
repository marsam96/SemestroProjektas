using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Kursinis
{
    class Program
    {
        static void Main(string[] args) {
            WebHost.CreateDefaultBuilder(args)
               .UseUrls("http://0.0.0.0:5005")
               .UseStartup<Startup>()
               .Build()
               .Run();
        }
    }
}
