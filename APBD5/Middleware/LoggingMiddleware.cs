using APBD5.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APBD5.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IStudentsDbService service)
        {

            context.Request.EnableBuffering();
            if (context.Request != null)
            {
                string path = context.Request.Path;
                string method = context.Request.Method;
                string queryString = context.Request.QueryString.ToString();
                string bodyStr = "";

                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                // zapisanie do pliku 
                using (FileStream fs = new FileStream("requestLogs", FileMode.CreateNew))
                {
                    using (BinaryWriter w = new BinaryWriter(fs))
                    {
                        w.Write("Method: "+ method);
                        w.Write("Path: "+ path);
                        w.Write("Body: " +bodyStr);
                        w.Write("Query: " + queryString);
                    }
                }

            }

            if(_next!=null)
            await _next(context);

        }
    }
}
