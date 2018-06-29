using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CompanyFinderAPI
{

    /// <summary>
    /// Main entry point to the program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The app is run through a console application, it is built here
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        /// <summary>
        /// Method to set the startup class as the main configuration class for the program. Tell the program to use kestral and iss integration
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>().UseDefaultServiceProvider(options =>
                    options.ValidateScopes = false).UseKestrel().UseIISIntegration()
                .Build();
    }
}
