#region License
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
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Infra;

namespace Hooked {

	class Program {

        private static Dictionary<string, Func<IBasicTest>> Handlers = new Dictionary<string, Func<IBasicTest>> {
            { "im", () => new BasicInMemoryTest() },
            { "ef", () => new BasicEFTest() }
        };

		static void Main(string[] args) {
            var key = args.Length == 0 ? Handlers.Keys.First() : args.First();
            Assert.True(Handlers.ContainsKey(key), () => "No such test: " + key);
            Console.WriteLine("Running selected tester: " + key);
            IBasicTest tester = Handlers[key]();
            tester.Init(args.Length < 2 || args[1] == "http" ? Initializer.Http : Initializer.MSMQ);
            tester.Start();
            Console.WriteLine("Enter to terminate");
            Console.ReadLine();
            tester.Stop();
		}
	}
}
