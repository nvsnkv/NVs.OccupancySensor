using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NVs.OccupancySensor.API.Formatters;
using NVs.OccupancySensor.API.MQTT;
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

            services.AddSingleton<IMqttAdapter>(s => new HomeAssistantMqttAdapter(
                s.GetService<IOccupancySensor>() ?? throw new InvalidOperationException("OccupancySensor was not resolved!"), 
                s.GetService<ILogger<HomeAssistantMqttAdapter>>() ?? throw new InvalidOperationException("Logger for HomeAssistantMqttAdapter was not resolved!"),
                HomeAssistantMqttAdapter.CreateClient,
                new AdapterSettings(s.GetService<IConfiguration>() ?? throw new InvalidOperationException("Configuration was not resolved!"))));
            
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

            if (bool.TryParse(Configuration["StartSensor"], out var startSensor) && startSensor)
            {
                var sensor = app.ApplicationServices.GetService<IOccupancySensor>() ?? throw new InvalidOperationException("Unable to resolve OccupancySensor!");
                sensor.Start();
            }
            
            if (bool.TryParse(Configuration["StartMQTT"], out var startAdapter) && startAdapter)
            {
                var adapter = app.ApplicationServices.GetService<IMqttAdapter>() ?? throw new InvalidOperationException("Unable to resolve HomeAssistantMqttAdapter!");
                var _ =adapter.Start();
            }
        }
    }
}
