using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microphone.Core;
using Microphone.Core.ClusterProviders;
using Microphone.Nancy;
using Nancy;
using Nancy.Hosting.Self;

namespace ServiceDiscovery.Two
{
    class Program
    {
        static void Main(string[] args)
        {
            Cluster.Bootstrap(new NancyProvider(), new ConsulProvider(), "two", "v1");

            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("Stopped. Good bye!");
                    break;
                }

                Console.WriteLine("Sending a message to service one");

                Task.Run(async () =>
                {
                    try
                    {
                        //automatically load balanced over service instances
                        var instance = await Cluster.FindServiceInstanceAsync("one");

                        //Use Rest# or similar to call into the remote service
                        using (var client = new HttpClient())
                        {
                            var result = await client.GetAsync($"http://{instance.Address}:{instance.Port}");
                            var content = await result.Content.ReadAsStringAsync();

                            Console.WriteLine($"Recieved response from service one: {content}");
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Failed to Recieve response from service one: {e.Message}");
                    }
                });
            }
        }
    }

    public class TestModule : NancyModule
    {
        public TestModule() : base("")
        {
            Get["/"] = parameters => Response.AsText("Yo dude two");
        }
    }
}
