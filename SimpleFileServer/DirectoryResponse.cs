using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public class DirectoryResponse : IServerResponse
    {
        public DirectoryResponse(string directory)
        {
            this.directory = directory;
        }
        private string directory;

        public bool IsValid(HttpListenerRequest request)
        {
            var relativePath = request.Url.LocalPath.TrimStart('/').Replace("/", @"\");
            var localPath = Path.Combine(directory, relativePath);

            return Directory.Exists(localPath);
        }
        public async Task Response(HttpListenerContext context)
        {
            var relativePath = context.Request.Url.LocalPath.TrimStart('/').Replace("/", @"\");
            var localPath = Path.Combine(directory, relativePath);

            var type = context.Request.QueryString["type"] ?? "file";
            switch (type)
            {
                case "file":
                case "files":
                    var onlyFilesText = GetFilesJson(localPath);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteTextAsync(onlyFilesText);
                    break;

                case "folder":
                case "folders":
                case "directory":
                case "directories":
                    var onlyDirectoriesText = GetDirectoriesJson(localPath);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteTextAsync(onlyDirectoriesText);
                    break;

                case "all":
                case "both":
                default:
                    var filesText = GetFilesJson(localPath);
                    var directoriesText = GetDirectoriesJson(localPath);
                    var jsonText = "{ files: " + filesText + ", directories: " + directoriesText + " }";
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteTextAsync(jsonText);
                    break;
            }
        }

        private string GetFilesJson(string localPath)
        {
            var files = Directory.GetFiles(localPath).Select(Path.GetFileName);
            return "[" + string.Join(",", files.Select(t => "\"" + t + "\"")) + "]";
        }
        private string GetDirectoriesJson(string localPath)
        {
            var files = Directory.GetDirectories(localPath).Select(Path.GetFileName);
            return "[" + string.Join(",", files.Select(t => "\"" + t + "\"")) + "]";
        }
    }
}
