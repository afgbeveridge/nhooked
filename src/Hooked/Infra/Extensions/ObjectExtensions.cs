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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ComplexOmnibus.Hooked.Infra.Extensions {
	
	public static class ObjectExtensions {

		public static TType Fluently<TType>(this TType src, Action act) {
			act.IsNotNull(act);
			return src;
		}

		public static bool IsNotNull<TType>(this TType src, Action act = null) where TType : class {
			bool isNotNull = src != null;
			if (act != null && isNotNull)
				act();
			return isNotNull;
		}

		public static bool IsNull<TType>(this TType src, Action act = null) where TType : class {
			bool isNull = src == null;
			if (act != null && isNull)
				act();
			return isNull;
		}

		public static TResult GuardedExecution<TResult>(this object src, Func<TResult> f, Func<Exception, TResult> onException = null) {
			TResult result = default(TResult);
			try {
				result = f();
			}
			catch (Exception ex) {
				onException.IsNotNull(() => result = onException(ex));
			}
			return result;
		}

		public static void GuardedExecution(this object src, Action f, Action<Exception> onException = null) {
			try {
				f();
			}
			catch (Exception ex) {
				onException.IsNotNull(() => onException(ex));
			}
		}

		public static TAncillary Deserialize<TAncillary>(this object src) where TAncillary : class {
			TAncillary result = default(TAncillary);
			src.IsNotNull(() => {
				byte[] b = Convert.FromBase64String(src.ToString());
				using (var stream = new MemoryStream(b)) {
					var formatter = new BinaryFormatter();
					stream.Seek(0, SeekOrigin.Begin);
					result = (TAncillary)formatter.Deserialize(stream);
				}
			});
			return result;
		}

		public static object Serialize<TAncillary>(this TAncillary obj) where TAncillary : class {
			object result = null;
			obj.IsNotNull(() => {
				using (var stream = new MemoryStream()) {
					var formatter = new BinaryFormatter();
					formatter.Serialize(stream, obj);
					stream.Flush();
					stream.Position = 0;
					result = Convert.ToBase64String(stream.ToArray());
				}
			});
			return result;
		}
	}
}
