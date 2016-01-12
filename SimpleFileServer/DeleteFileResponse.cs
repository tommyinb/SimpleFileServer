using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public class DeleteFileResponse : IServerResponse
    {
        public DeleteFileResponse(string directory)
        {
            this.directory = directory;
        }
        private string directory;

        public bool IsValid(HttpListenerRequest request)
        {
            var validHeader = request.HttpMethod == "DELETE"
                || request.Url.Query == "?delete"
                || request.Url.Query == "?remove";

            if (validHeader == false) return false;

            var localFilePath = request.MapFilePath(directory);

            if (File.Exists(localFilePath) == false) return false;

            return true;
        }
        public async Task Response(HttpListenerContext context)
        {
            var localFilePath = context.Request.MapFilePath(directory);

            try
            {
                File.Delete(localFilePath);
            }
            catch (Exception e)
            {
                context.Response.WriteResult(HttpStatusCode.InternalServerError, e.Message);
                return;
            }

            await context.Response.WriteTextAsync(localFilePath + " deleted");
        }
    }
}
