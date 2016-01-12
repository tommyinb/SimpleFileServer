using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileServer
{
    public class BadRequestResponse : IServerResponse
    {
        public bool IsValid(HttpListenerRequest request)
        {
            return true;
        }

        public async Task Response(HttpListenerContext context)
        {
            await context.Response.WriteResultAsync(HttpStatusCode.BadRequest, "bad request");
        }
    }
}
