using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NVs.OccupancySensor.API.Formatters;
using NVs.OccupancySensor.CV;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.CV.Utils;

namespace NVs.OccupancySensor.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddPresenceDetection()
                .AddControllers(o => o.OutputFormatters.Add(new RgbImageOutputFormatter()));

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NV's Occupancy Sensor API V0");
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseStaticFiles();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            if (bool.TryParse(Configuration["CV:StartSensor"], out var startSensor) && startSensor)
            {
                var sensor = app.ApplicationServices.GetService<IOccupancySensor>() ?? throw new InvalidOperationException("Unable to resolve OccupancySensor!");
                sensor.Start();
            }
        }
    }
}
