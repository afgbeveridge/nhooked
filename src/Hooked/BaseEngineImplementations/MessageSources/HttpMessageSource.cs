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
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Interfaces.Ancillary;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using System.Threading;
using System.IO;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.MessageSources {

    public class HttpMessageSource : BaseMessageSource {

        private ConcurrentQueue<IMessage> Messages { get; set; }
        public const string AddressKey = "address";

        public HttpMessageSource(IContentParser parser, IConfigurationSource cfg, ILogger logger) : base(parser, cfg, logger) {
            Messages = new ConcurrentQueue<IMessage>();
        }

        public override IRequestResult<IMessage> Retrieve {
            get {
                IMessage msg;
                RequestResult<IMessage> result = new RequestResult<IMessage>();
                result.Success = Messages.TryDequeue(out msg);
                result.Containee = msg;
                return result;
            }
        }

        public override IRequestResult<IMessage> Peek {
            get {
                IMessage msg;
                RequestResult<IMessage> result = new RequestResult<IMessage>();
                result.Success = Messages.TryPeek(out msg);
                result.Containee = msg;
                return result;
            }
        }

        public override bool CanPeek {
            get { return true; }
        }

        public override IRequestResult Initialize() {
            Listener = new HttpListener();
            var address = Configuration.Get<string>(this, AddressKey);
            Logger.LogInfo("Starting HTTP listening at URI: " + address);
            Listener.Prefixes.Add(address);
            Listener.Start();
            CancellationToken = new CancellationTokenSource();
            CancellationToken token = CancellationToken.Token;
            ListenerTask = Task.Factory.StartNew(() => {
                while (!token.IsCancellationRequested)
                    ListenerImplementation();
                Listener.Stop();
            });
            return RequestResult.Create();
        }

        public override IRequestResult UnInitialize() {
            CancellationToken.Cancel();
            return RequestResult.Create();
        }

        // TODO:

        public IRequestResult Hydrate(IHydrationObject obj) {
            if (obj.IsNotNull() && obj.State.IsNotNull())
                Messages = obj.State.Deserialize<StateContainer>().Messages;
            return RequestResult.Create();
        }

        public IRequestResult<IHydrationObject> Dehydrate() {
            StateContainer container = new StateContainer { Messages = Messages };
            return RequestResult<IHydrationObject>.Create(new HydrationObject(GetType(), container.Serialize().ToString()) { ServiceInterface = typeof(IMessageSource) });
        }

        private CancellationTokenSource CancellationToken { get; set; }

        private HttpListener Listener { get; set; }

        private Task ListenerTask { get; set; }

        private void ListenerImplementation() {
            var res = Listener.BeginGetContext(new AsyncCallback(ListenerCallback), Listener);
            if (!res.AsyncWaitHandle.WaitOne(1000)) {
                res.AsyncWaitHandle.Close();
            }
        }

        private void ListenerCallback(IAsyncResult result) {
            HttpListener listener = (HttpListener)result.AsyncState;
            var status = 200;
            var responseMessage = String.Empty;
            HttpListenerContext context = null;
            this.GuardedExecution(() => {
                context = listener.EndGetContext(result);
                HttpListenerRequest request = context.Request;
                using (Stream streamResponse = request.InputStream) {
                    var content = new StreamReader(streamResponse).ReadToEnd();
                    var res = Parser.Interpret(content);
                    res.Success.IfTrue(() => Messages.Enqueue(res.Containee));
                    responseMessage = res.Message;
                }
            },
            ex => status = 500);
            if (context.IsNotNull()) {
                // Now interpret response
                HttpListenerResponse response = context.Response;
                // Construct a response. 
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseMessage);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                response.StatusCode = status;
                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        [Serializable]
        private class StateContainer {
            internal ConcurrentQueue<IMessage> Messages { get; set; }
        }

    }
}
