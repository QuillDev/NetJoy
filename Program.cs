using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetJoy.Core.Config;
using NetJoy.Core.NetJoy;
using NetJoy.Core.NetJoy.Client;
using NetJoy.Core.NetJoy.Server;
using NetJoy.Core.Utils;


namespace NetJoy
{
    static class Program
    {
        //create the configuration field
        private static readonly Configuration Config = ConfigService.ReadConfig();
        
        private static void Main()
        {
            //set the title of the console
            Console.Title = "NetJoy - QuillDev";
            
            //start in an async context
            StartAsync().GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Start the program in an asynchronous context
        /// </summary>
        /// <returns></returns>
        private static async Task StartAsync()
        {
            //create the service provider
            var services = ConfigureServices();
            
            //get the NetJoy manager
            var netJoyManager = services.GetRequiredService<NetJoyManager>();
            //TODO use this again later
            await netJoyManager.Start(); //start the NetJoy manager
            
            await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
        }

        private static IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(Config)
                .AddSingleton<NetJoyManager>()
                .AddSingleton<NetJoyClient>()
                .AddSingleton<NetJoyServer>()
                .AddSingleton<NgrokUtils>()
                .BuildServiceProvider();
        }
    }
}