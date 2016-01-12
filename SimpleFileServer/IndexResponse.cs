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
        }
        
        private string directory;
        private string GetIndexFileName()
        {
            var indexNames = new[] { "index.html", "index.htm", "index.txt" };

            return indexNames.FirstOrDefault(t =>
            {
                var indexPath = Path.Combine(directory, t);
                return File.Exists(indexPath);
            });
        }

        public bool IsValid(HttpListenerRequest request)
        {
            return request.HttpMethod == "GET"
                && request.Url.LocalPath == "/";
        }
        public async Task Response(HttpListenerContext context)
        {
            var fileName = GetIndexFileName();
            if (fileName != null)
            {
                var filePath = Path.Combine(directory, fileName);
                await GetFileResponse.Response(context.Response, filePath);
            }
            else
            {
                await context.Response.WriteTextAsync("Simple File Server");
            }
        }
    }
}
