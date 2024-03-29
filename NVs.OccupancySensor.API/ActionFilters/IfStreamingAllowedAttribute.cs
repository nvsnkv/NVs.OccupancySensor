﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NVs.OccupancySensor.API.Models;

namespace NVs.OccupancySensor.API.ActionFilters
{
    internal class IfStreamingAllowedAttribute : Attribute, IFilterFactory
    {
        private readonly Streaming allowed;

        public IfStreamingAllowedAttribute(Streaming allowed)
        {
            this.allowed = allowed;
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new SteamingAllowedActionFilter(allowed, serviceProvider.GetService<IConfiguration>() ?? throw new InvalidOperationException("Failed to retrieve configuration"));
        }

        public bool IsReusable => false;

        private class SteamingAllowedActionFilter : IAsyncActionFilter
        {
            private readonly Streaming allowed;
            private readonly IConfiguration configuration;

            public SteamingAllowedActionFilter(Streaming allowed, IConfiguration configuration)
            {
                this.allowed = allowed;
                this.configuration = configuration;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (Enum.TryParse(configuration["Streaming"], out Streaming configured) && configured >= allowed)
                {
                    await next();
                }
                else
                {
                    context.Result = new NoContentResult();
                }
            }
        }
    }
}