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
            var relativeFilePath = request.Url.LocalPath.TrimStart('/').Replace("/", @"\");
            var fileExtension = Path.GetExtension(relativeFilePath);

            var localFilePath = Path.Combine(directory, relativeFilePath);
            if (File.Exists(localFilePath))
            {
                return request.Url.Query == "?delete"
                    || request.Url.Query == "?remove";
            }
            else
            {
                return false;
            }
        }
        public async Task Response(HttpListenerContext context)
        {
            var relativeFilePath = context.Request.Url.LocalPath.TrimStart('/').Replace("/", @"\");
            var localFilePath = Path.Combine(directory, relativeFilePath);

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
