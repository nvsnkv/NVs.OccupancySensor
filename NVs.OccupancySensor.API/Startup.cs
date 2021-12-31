using System;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NVs.OccupancySensor.API.Formatters;
using NVs.OccupancySensor.API.Models;
using NVs.OccupancySensor.API.MQTT;
using NVs.OccupancySensor.API.MQTT.Watchdog;
using NVs.OccupancySensor.CV.BackgroundSubtraction;
using NVs.OccupancySensor.CV.Capture;
using NVs.OccupancySensor.CV.Correction;
using NVs.OccupancySensor.CV.Denoising;
using NVs.OccupancySensor.CV.Detection;
using NVs.OccupancySensor.CV.Observation;
using NVs.OccupancySensor.CV.Sense;
using NVs.OccupancySensor.CV.Utils;
using Serilog;

namespace NVs.OccupancySensor.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

            services
                .AddPresenceDetection()
                .AddControllers(o => o.OutputFormatters.Add(new ImageOutputFormatter()));

            services.AddSingleton(s => new Streams(
                s.GetService<ICamera>() ?? throw new InvalidOperationException("Camera was not resolved!"), 
                s.GetService<IDenoiser>() ?? throw new InvalidOperationException("Denoiser was not resolved!"),
                s.GetService<IBackgroundSubtractor>() ?? throw new InvalidOperationException("BackgroundSubtractor was not resolved!"), 
                s.GetService<ICorrector>() ?? throw new InvalidOperationException("Corrector was not resolved!"), 
                s.GetService<IPeopleDetector>() ?? throw new InvalidOperationException("PeopleDetector was not resolved!")));

            services.AddScoped(s => new Observers(
                s.GetService<IImageObserver<Gray>>() ?? throw new InvalidOperationException("Gray observer was not resolved!")));

            services.AddSingleton<IMqttAdapter>(s => new HomeAssistantMqttAdapter(
                s.GetService<IOccupancySensor>() ?? throw new InvalidOperationException("OccupancySensor was not resolved!"),
                s.GetService<ILogger<HomeAssistantMqttAdapter>>() ?? throw new InvalidOperationException("Logger for HomeAssistantMqttAdapter was not resolved!"),
                HomeAssistantMqttAdapter.CreateClient(
                    new WatchdogSettings(s.GetService<IConfiguration>() ?? throw new InvalidOperationException("Configuration was not resolved!")),
                    s.GetService<ILogger<Watchdog>>() ?? throw new InvalidOperationException("Watchdog loggrer was not resolved!")),
                new AdapterSettings(s.GetService<IConfiguration>() ?? throw new InvalidOperationException("Configuration was not resolved!"))));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "NV's Occupancy Sensor HTTP API",
                    Description = "API for OpenCV-based occupancy detector",
                    Contact = new OpenApiContact
                    {
                        Name = "nvsnkv",
                        Email = string.Empty,
                        Url = new Uri("https://github.com/nvsnkv"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT",
                        Url = new Uri("https://github.com/nvsnkv/NVs.OccupancySensor/blob/master/LICENSE.txt"),
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAll");
            app.UseSwagger();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NV's Occupancy Sensor API v1");
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
                var _ = adapter.Start();
            }
        }
    }
}
