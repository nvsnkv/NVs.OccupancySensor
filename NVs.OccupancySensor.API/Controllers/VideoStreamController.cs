using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NVs.OccupancySensor.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoStreamController : ControllerBase
    {
        [HttpGet]
        public async Task GetStream()
        {
            try
            {
                var response = Response.Body;
                await using var writer = new StreamWriter(response);
                for (var i = 0; i < 100000; i++)
                {
                    await Task.Delay(100);
                    await writer.WriteLineAsync(DateTime.Now.ToUniversalTime().ToLongTimeString());
                    await writer.FlushAsync();
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }
    }
}