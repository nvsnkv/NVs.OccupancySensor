using System;
using System.Net;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace NVs.OccupancySensor.API.Formatters
{
    internal sealed class ImageOutputFormatter : OutputFormatter
    {
        private static readonly Image<Rgb, byte> EmptyImage = new Image<Rgb, byte>(100, 100);

        public ImageOutputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/jpeg"));
        }

        protected override bool CanWriteType(Type type)
        {
            return type == typeof(Image<Rgb, byte>) || type == typeof(Image<Gray, byte>);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Object is null)
            {
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
                return;
            }

            var bytes = context.Object switch
            {
                Image<Rgb, byte> rgb => rgb.ToJpegData(),
                Image<Gray, byte> gray => gray.ToJpegData(),
                _ => EmptyImage.ToJpegData()
            };

            context.HttpContext.Response.Headers.ContentLength = bytes.LongLength;
            await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}