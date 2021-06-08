using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
namespace Microsoft.AspNetCore.Builder
{
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class RequestRewriterExtensions
    {
        public static IApplicationBuilder UseRequestRewriter(this IApplicationBuilder builder, RequestRewriterOptions rro)
        {
            return builder.UseMiddleware<RequestRewriterMiddleware>(rro);
        }
    }
    public class RequestRewriterOptions
    {

        public readonly IEnumerable<string> replacewhat;
        public readonly string replacewithwhat;

        public RequestRewriterOptions(IEnumerable<string> replacewhat, string replacewithwhat)
        {
            this.replacewhat = replacewhat;
            this.replacewithwhat = replacewithwhat;
        }

    }
    public class RequestRewriterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnumerable<string> replacewhat;
        private readonly string replacewithwhat;

        public RequestRewriterMiddleware(RequestDelegate next, RequestRewriterOptions rro)
        {
            _next = next;
            this.replacewhat = rro.replacewhat;
            this.replacewithwhat = rro.replacewithwhat;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            using (var filteredResponse = new TextReplaceStream(context.Response.Body, replacewhat, replacewithwhat, context))
            {
                context.Features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(filteredResponse));
            }
            await _next(context);
            return;
        }
    }
    public class TextReplaceStream : MemoryStream
    {
        private readonly Stream responseStream;
        private readonly IEnumerable<string> replacewhat;
        private readonly string replacewithwhat;
        private readonly HttpContext context;
        public TextReplaceStream(Stream stream, IEnumerable<string> replacewhat, string replacewithwhat, HttpContext context)
        {
            responseStream = stream;
            this.replacewhat = replacewhat;
            this.replacewithwhat = replacewithwhat;
            this.context = context;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            string html = Encoding.UTF8.GetString(buffer, offset, count);

            foreach(string replstring in replacewhat)
            {
                html = html.Replace(replstring, replacewithwhat);   
            }
                     
            var buffer1 = Encoding.UTF8.GetBytes(html);
            responseStream.Write(buffer1, offset, buffer.Length);
        }
        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            string html = Encoding.UTF8.GetString(buffer, offset, count);

             foreach(string replstring in replacewhat)
            {
                html = html.Replace(replstring, replacewithwhat);   
            }        

            var buffer1 = Encoding.UTF8.GetBytes(html);
            await responseStream.WriteAsync(buffer1, offset, buffer1.Length);
        }
        public override void Flush() => responseStream.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            //context.Response.Headers.ContentLength = null;
            return responseStream.FlushAsync(cancellationToken);
        }
    }
}