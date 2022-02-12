using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NVs.OccupancySensor.API.ActionResults
{
    /// <summary>
    /// Motion JPEG ActionResult - Chris Green's implementation: https://blog.green.web.za/2019/11/23/mjpeg-in-asp-net-core.html
    /// </summary>
    internal sealed class MjpegStreamContent : IActionResult
    {
        private static readonly string Boundary = "MotionImageStream";
        private static readonly string ContentType = "multipart/x-mixed-replace;boundary=" + Boundary;
        private static readonly byte[] NewLine = Encoding.UTF8.GetBytes("\r\n");

        private readonly Func<CancellationToken, Task<byte[]?>> onNextImage;
        private readonly Action onEnd;

        public MjpegStreamContent(Func<CancellationToken, Task<byte[]?>> onNextImage, Action onEnd)
        {
            this.onNextImage = onNextImage;
            this.onEnd = onEnd;
        }

        public async Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.ContentType = ContentType;

            var outputStream = context.HttpContext.Response.Body;
            var cancellationToken = context.HttpContext.RequestAborted;

            try
            {
                bool streamEnded = false;
                while (!cancellationToken.IsCancellationRequested && !streamEnded)
                {
                    var imageBytes = await onNextImage(cancellationToken);
                    if (imageBytes != null)
                    {
                        var header = $"--{Boundary}\r\nContent-Type: image/jpeg\r\nContent-Length: {imageBytes.Length}\r\n\r\n";
                        var headerData = Encoding.UTF8.GetBytes(header);
                        await outputStream.WriteAsync(headerData, cancellationToken);
                        await outputStream.WriteAsync(imageBytes, cancellationToken);
                        await outputStream.WriteAsync(NewLine, cancellationToken);
                    }
                    else
                    {
                        streamEnded = true;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // connection closed, no need to report this
            }
            finally
            {
                onEnd();
            }
        }
    }
}