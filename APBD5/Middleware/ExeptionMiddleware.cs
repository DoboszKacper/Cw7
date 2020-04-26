using APBD5.Exceptions;
using APBD5.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD5.Middleware
{
    public class ExeptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExeptionMiddleware (RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync (HttpContext context)
        {
            try
            {
                await _next(context);
            }catch(Exception e)
            {
                await HandleExeptionAsync(context, e);
            }
            
        }

        private Task HandleExeptionAsync(HttpContext context, Exception e)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            if(e is StudentCannotDefendException)
            {
                return context.Response.WriteAsync(new ErrorDetails
                {
                    StatusCode = (int)StatusCodes.Status400BadRequest,
                    Message = e.Message
                }.ToString());

            }

            return context.Response.WriteAsync(new ErrorDetails
            {
                StatusCode = (int)StatusCodes.Status500InternalServerError,
                Message = "Wystapił błąd"
            }.ToString());
        }
    }
}
