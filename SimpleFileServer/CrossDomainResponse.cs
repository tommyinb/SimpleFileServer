using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public class CrossDomainResponse : IServerResponse
    {
        public bool IsValid(HttpListenerRequest request)
        {
            return Regex.IsMatch(request.Url.LocalPath, @"/CrossDomain/(?<path>.+)", RegexOptions.IgnoreCase);
        }

        public async Task Response(HttpListenerContext context)
        {
            var urlMatch = Regex.Match(context.Request.Url.LocalPath, "/CrossDomain/(?<path>.+)", RegexOptions.IgnoreCase);

            var url = urlMatch.Groups["path"].Value;
            url = Regex.Replace(url, @"^(\w+):/(?!/)", @"$1://");

            Tuple<string, byte[]> response;
            try
            {
                response = await GetResponse(context.Request, url);
            }
            catch (Exception e)
            {
                context.Response.WriteResult(HttpStatusCode.InternalServerError, e.Message);
                return;
            }

            context.Response.ContentType = response.Item1;
            context.Response.ContentLength64 = response.Item2.Length;
            await context.Response.OutputStream.WriteAsync(response.Item2, 0, response.Item2.Length);
        }
        private async Task<Tuple<string, byte[]>> GetResponse(HttpListenerRequest request, string url)
        {
            var webRequest = HttpWebRequest.CreateHttp(url);

            webRequest.Accept = request.Headers["Accept"];
            webRequest.UserAgent = request.UserAgent;

            webRequest.Proxy = null;

            var requestBytes = await request.InputStream.ReadToEndAsync();
            if (requestBytes.Length > 0)
            {
                using (var webRequestStream = await webRequest.GetRequestStreamAsync())
                {
                    await webRequestStream.WriteAsync(requestBytes, 0, requestBytes.Length);
                }
            }

            var webResponse = await webRequest.GetResponseAsync();

            using (var webResponseStream = webResponse.GetResponseStream())
            {
                var responseBytes = await webResponseStream.ReadToEndAsync();
                
                return Tuple.Create(webResponse.ContentType, responseBytes);
            }
        }
    }
}
