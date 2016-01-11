using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public class IndexResponse : IServerResponse
    {
        public IndexResponse(string directory)
        {
            this.directory = directory;

            indexFiles.Add("index.html");
            indexFiles.Add("index.htm");
        }
        private string directory;
        private List<string> indexFiles = new List<string>();

        public bool IsValid(HttpListenerRequest request)
        {
            return request.Url.LocalPath == "/"
                && request.QueryString.Count <= 0
                && indexFiles.Select(t => Path.Combine(directory, t)).Any(File.Exists);
        }
        public async Task Response(HttpListenerContext context)
        {
            var indexFile = indexFiles.Select(t => Path.Combine(directory, t)).FirstOrDefault(File.Exists);

            if (indexFile != null)
            {
                var fileResponse = new GetFileResponse(directory);

                await fileResponse.Response(context.Response, indexFile);
            }
            else
            {
                await context.Response.WriteTextAsync("Simple File Server");
            }
        }
    }
}
