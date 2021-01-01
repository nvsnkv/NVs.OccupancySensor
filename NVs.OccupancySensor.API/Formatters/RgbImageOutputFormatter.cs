using System;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace NVs.OccupancySensor.API.Formatters
{
    sealed class RgbImageOutputFormatter : OutputFormatter
    {
        private static readonly Image<Rgb, float> EmptyImage = new Image<Rgb, float>(100, 100);
        
        public RgbImageOutputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/jpeg"));
        }

        protected override bool CanWriteType(Type type)
        {
            return type == typeof(Image<Rgb, float>);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var image = context.Object as Image<Rgb, float> ?? EmptyImage;
            
            var bytes = image.ToJpegData();
            
            context.HttpContext.Response.Headers.ContentLength = bytes.LongLength;
            await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}