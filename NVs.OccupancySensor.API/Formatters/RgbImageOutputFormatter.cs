using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace NVs.OccupancySensor.API.Formatters
{
    sealed class RgbImageOutputFormatter : OutputFormatter
    {
        public RgbImageOutputFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("image/jpeg"));
        }

        protected override bool CanWriteType(Type type)
        {
            return type == typeof(Image<Rgb, int>);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var image = context.Object as Image<Rgb, int>;
            if (image == null) throw new ArgumentNullException(nameof(image));

            var bytes = image.ToJpegData();
            
            context.HttpContext.Response.Headers.ContentLength = bytes.LongLength;
            await context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}