﻿#region License
/*
Copyright (c) 2015 Tony Beveridge

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to 
whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE 
AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using ComplexOmnibus.Hooked.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using System.Messaging;

namespace NHookedSimulator {

    /// <summary>
    /// Cheap and cheerful testing harness
    /// </summary>
    class Program {

        static void Main(string[] args) {
            Assert.True(args.Length == 1, () => "Must supply an endpoint");
            var files = Directory.EnumerateFiles(".", "*.txt").Select(s => new SimulationUnit(File.ReadAllLines(s), args.First()));
            Console.WriteLine("Simulate load: " + files.Count() + " file(s) found....");
            CancellationTokenSource src = new CancellationTokenSource();
            var token = src.Token;
            var tasks =
                files.Select(u => {
                    return Task.Run(async () => {
                        var unit = (SimulationUnit)u;
                        while (!token.IsCancellationRequested) {
                            var delay = unit.SimulatedDelay;
                            Console.WriteLine(unit.Name + " - waiting " + delay + " ms");
                            await Task.Delay(delay);
                            try {
                                Console.WriteLine(unit.Name + " - sending ");
                                // Cycnical this, but I'm not holding up a test harness as an exemplar of good design - this is utility at work
                                if (unit.IsHttp)
                                    await HttpSend(unit);
                                else
                                    await MsMqSend(unit);
                            }
                            catch (Exception ex) {
                                Console.WriteLine("Unit failed...." + ex.ToString());
                            }
                        }
                    }, token);
                })
                .ToArray();
            Console.WriteLine("Press enter to terminate");
            Console.ReadLine();
            src.Cancel();
            Task.WaitAll(tasks);
        }

        private static async Task<HttpResponseMessage> HttpSend(SimulationUnit unit) {
            var client = new HttpClient();
            return await client.PostAsync(unit.TargetEndpoint, new StringContent(unit.GeneratedBody, Encoding.ASCII, unit.MimeType));
        }

        private static async Task MsMqSend(SimulationUnit unit) {
            using (var queue = new MessageQueue(unit.TargetEndpoint)) {
                Message msg = new Message { Body = unit.GeneratedBody, Formatter =  new BinaryMessageFormatter() };
                queue.Send(msg);
            }
        }

    }

    internal class SimulationUnit {

        private static readonly Tuple<int, int> Low = Tuple.Create(3000, 10000);
        private static readonly Tuple<int, int> High = Tuple.Create(1, 50);

        public SimulationUnit(IEnumerable<string> from, string targetEndpoint) {
            Name = from.First();
            Rate = from.Skip(1).First() == "Low" ? Low : High;
            MimeType = from.Skip(2).First();
            RawBody = string.Join(Environment.NewLine, from.Skip(3));
            RandomSource = new Random();
            TargetEndpoint = targetEndpoint;
        }

        public string Name { get; private set; }

        public string RawBody { get; private set; }

        public string GeneratedBody { 
            get {
                return RawBody.Replace("#SEQUENCE#", (++Sequence).ToString()).Replace("#ID#", (++Id).ToString());
            } 
        }

        public string MimeType { get; private set; }

        public int SimulatedDelay { 
            get { 
                return RandomSource.Next(Rate.Item1, Rate.Item2); 
            } 
        }

        public string TargetEndpoint { get; private set; }

        public bool IsHttp { 
            get {
                return TargetEndpoint.StartsWith(Uri.UriSchemeHttp);
            } 
        }

        private Tuple<int, int> Rate { get; set; }

        private Random RandomSource { get; set; }

        private int Id { get; set; }

        private int Sequence { get; set; }

    }

}
