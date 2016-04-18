using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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
            if (request.HttpMethod == "POST")
            {
                var localFilePath = request.MapFilePath(directory);
                return Path.GetExtension(localFilePath) != string.Empty;
            }
            else
            {
                return false;
            }
        }

        public async Task Response(HttpListenerContext context)
        {
            var localFilePath = context.Request.MapFilePath(directory);

            if (context.Request.ContentType.Contains("multipart/form-data"))
            {
                var fileBytes = await GetBytesFromMultipart(context.Request);

                if (fileBytes == null)
                {
                    await context.Response.WriteResultAsync(HttpStatusCode.BadRequest, "no file provided");
                    return;
                }

                var saveText = SaveFile(localFilePath, fileBytes);
                await context.Response.WriteTextAsync(saveText);
            }
            else if (context.Request.Headers["Content-Transfer-Encoding"] == "base64")
            {
                var inputBytes = await context.Request.InputStream.ReadToEndAsync();
                var inputText = Encoding.ASCII.GetString(inputBytes);

                var fileBytes = Convert.FromBase64String(inputText);
                var saveText = SaveFile(localFilePath, fileBytes);
                await context.Response.WriteTextAsync(saveText);
            }
            else
            {
                var fileBytes = await context.Request.InputStream.ReadToEndAsync();

                var saveText = SaveFile(localFilePath, fileBytes);
                await context.Response.WriteTextAsync(saveText);
            }
        }
        private async Task<byte[]> GetBytesFromMultipart(HttpListenerRequest request)
        {
            var multiparts = await MultipartFormData.ParseAsync(request);

            var multipart = multiparts.FirstOrDefault(t =>
            {
                if (t.Headers.ContainsKey("Content-Disposition") == false) return false;

                var contentDispositionText = t.Headers["Content-Disposition"];
                var contentDispositionParts = contentDispositionText.Split(new[] { "; ", ";" }, StringSplitOptions.RemoveEmptyEntries);

                var fileNameMatches = contentDispositionParts.Select(s =>
                    Regex.Match(s, @"^filename=[""']?(?<fileName>[^""']+)[""']?$"));
                return fileNameMatches.Any(s => s.Success);
            });

            if (multipart == null) return null;

            return multipart.Data;
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
