using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            return request.HttpMethod == "POST";
        }

        public async Task Response(HttpListenerContext context)
        {
            var streamContent = new StreamContent(context.Request.InputStream);
            if (streamContent.IsMimeMultipartContent())
            {
                var saveTexts = new List<string>();

                var multipart = await streamContent.ReadAsMultipartAsync();
                var fileParts = multipart.Contents.Where(t => t.Headers.ContentDisposition.FileName != null);
                foreach (var filePart in fileParts)
                {
                    var filePath = Path.Combine(directory, filePart.Headers.ContentDisposition.FileName);
                    var fileBytes = await filePart.ReadAsByteArrayAsync();

                    var saveText = SaveFile(filePath, fileBytes);
                    saveTexts.Add(saveText);
                }

                var responseText = string.Join("\r\n", saveTexts);
                await context.Response.WriteTextAsync(responseText);
            }
            else
            {
                var localFilePath = context.Request.MapFilePath(directory);
                var fileBytes = await streamContent.ReadAsByteArrayAsync();

                var saveText = SaveFile(localFilePath, fileBytes);
                await context.Response.WriteTextAsync(saveText);
            }
        }
        private string SaveFile(string filePath, byte[] fileBytes)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directoryPath);

            var oldBytes = File.Exists(filePath) ? File.ReadAllBytes(filePath) : null;

            File.WriteAllBytes(filePath, fileBytes);

            var getSizeText = new Func<int, string>(t =>
                t >= 1073741824 ? (int)Math.Round((double)t / 1073741824) + "GB"
                : t >= 1048576 ? (int)Math.Round((double)t / 1048576) + "MB"
                : t >= 1024 ? (int)Math.Round((double)t / 1024) + "KB" : t + "B");

            var oldByteText = oldBytes != null ? getSizeText(oldBytes.Length) : "N/A";
            var fileByteText = getSizeText(fileBytes.Length);
            return filePath + " (" + oldByteText + " -> " + fileByteText + ")";
        }
    }
}
