using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace SimulatedRemoteClient {
    
    class Program {
        static void Main(string[] args) {

            var addr = !args.Any() ? "http://localhost:8080" : args.First();

            using (WebApp.Start<StartUp>(addr)) {
                Console.WriteLine("Demo remote client is operational.");
                Console.WriteLine("Press any key to quit.");
                Console.ReadLine();
            }

        }
    }
}
