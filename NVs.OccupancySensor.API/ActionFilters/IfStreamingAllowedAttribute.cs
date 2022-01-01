using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace NVs.OccupancySensor.API.ActionFilters
{
    public class IfStreamingAllowedAttribute : TypeFilterAttribute
    {
        public IfStreamingAllowedAttribute() : base(typeof(StreamingAllowedActionFilter))
        {
        }

        private class StreamingAllowedActionFilter : IActionFilter
        {
            private static readonly string ConfigKey = "StreamingAllowed";
            private readonly IConfiguration config;

            private bool StreamingAllowed => bool.TryParse(config[ConfigKey], out var allowed) && allowed;

            public StreamingAllowedActionFilter(IConfiguration config)
            {
                this.config = config;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (!StreamingAllowed)
                {
                    context.Result = new NoContentResult();
                }
            }
        }
    }
}