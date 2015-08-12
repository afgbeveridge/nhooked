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

namespace Hooked {

	class Program {

		static void Main(string[] args) {
            //List<string> Context = new List<string> { "A" };
            //Task.Factory.StartNew(state => { 
            //    List<string> s = state as List<string>;
            //    Console.WriteLine(string.Join(",", s));
            //    System.Threading.Thread.Sleep(4000);
            //    Console.WriteLine(string.Join(",", s));
            //}, Context);
            //Console.WriteLine("Press ENTER to update list");
            //Console.ReadLine();
            //Context.Add("B");
            //Console.WriteLine("Press ENTER to finish");
            //new HttpListenerTest().Start();
            Console.WriteLine("Enter to terminate");
            BasicTest.Init();
            BasicTest.Start();
            Console.ReadLine();
            BasicTest.Stop();
		}
	}
}
