using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public class PostFileResponse : IServerResponse
    {
        public PostFileResponse(string directory)
        {
            this.directory = directory;
        }
        private string directory;

        public bool IsValid(HttpListenerRequest request)
        {
            if (request.HttpMethod != "POST") return false;

            var relativeFilePath = request.Url.LocalPath.TrimStart('/').Replace("/", @"\");
            var fileExtension = Path.GetExtension(relativeFilePath);

            return string.IsNullOrEmpty(fileExtension) == false;
        }
        public async Task Response(HttpListenerContext context)
        {
            var relativeFilePath = context.Request.Url.LocalPath.TrimStart('/').Replace("/", @"\");
            var filePath = Path.Combine(directory, relativeFilePath);

            var directoryPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directoryPath);

            var oldBytes = File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;
            var newBytes = await context.Request.InputStream.ReadToEndAsync();

            File.WriteAllBytes(filePath, newBytes);

            var units = new[] { Tuple.Create(1048576, "MB"), Tuple.Create(1024, "KB"), Tuple.Create(1, "B") };
            var getByteText = new Func<byte[], string>(t =>
            {
                var unit = units.FirstOrDefault(s => t.Length >= s.Item1);
                return unit != null ? t.Length / unit.Item1 + unit.Item2 : "0B";
            });

            var oldByteText = oldBytes != null ? getByteText(oldBytes) : "N/A";
            var newByteText = getByteText(newBytes);
            await context.Response.WriteTextAsync(filePath + " (" + oldByteText + " -> " + newByteText + ")");
        }
    }
}
