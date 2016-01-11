using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public sealed class BasicServer : IDisposable
    {
        public List<IServerResponse> Responses { get; private set; }

        public BasicServer(int port, string directory = ".")
        {
            Responses = new List<IServerResponse>();
            Responses.Add(new IndexResponse(directory));
            Responses.Add(new DeleteFileResponse(directory));
            Responses.Add(new GetFileResponse(directory));
            Responses.Add(new PostFileResponse(directory));
            Responses.Add(new DirectoryResponse(directory));
            Responses.Add(new CrossDomainResponse());
            Responses.Add(new BadRequestResponse());

            listener.Prefixes.Add("http://*:" + port + "/");
            listener.Start();

            var thread = new Thread(Listen);
            thread.IsBackground = true;
            thread.Start();
        }
        public void Dispose()
        {
            listener.Close();
        }

        private HttpListener listener = new HttpListener();
        private void Listen()
        {
            while (true)
            {
                try
                {
                    var context = listener.GetContext();

                    foreach (var response in Responses)
                    {
                        if (response.IsValid(context.Request))
                        {
                            var requestResponse = response.Response(context);
                            requestResponse.ContinueWith(t => context.Response.Close());

                            break;
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
    }
}
