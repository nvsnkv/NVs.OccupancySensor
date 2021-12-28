# NV's [Computer Vision] Room Occupancy Sensor
![Build(master)](https://github.com/nvsnkv/NVs.OccupancySensor/actions/workflows/dotnet.yml/badge.svg?branch=master)
## What's That?
Open-source ASP.Net Core application and MQTT client that uses computer vision to identify someone's presence in the room.
App uses background subtraction algorithms to identify if camera sees something which is not a part of furnishings.
MQTT client supports configuration convention used by Home Assistant [MQTT integration](https://www.home-assistant.io/docs/mqtt/)
## How to Use It?
Setup the app to connect it with your camera, MQTT server and have fun!
You can run it as a regular ASP.Net Core application on Windows host (x86_64) or Linux host (x86_64 or arm32). Application can use local cameras attached to host, IP cameras (at least the ones which send MJPEG streams) or even a video file.

## Getting Started
### Prerequisites
#### Hardware
Place your camera in the area you'd like to monitor.

Find an appropriate host machine that will run the app. Depending on configuration, application may consume significant amount of CPU and RAM, so it would be hard to provide a _minimal_ and _recommended_ configurations.
It works on Raspberry Pi 4 with 4 Gb RAM, so you can try something similar or more powerful.

This application does not provide anything that can control external devices or send notifications to end users out of the box. A home automation server with MQTT support would be required to setup various integrations which depends on someone's precense in the room. In most of the cases having a separate host for home automation server would be recommended option.

#### Software
Prepare your favorite API Explorer. [Swagger-UI](https://swagger.io/docs/open-source-tools/swagger-ui/usage/installation/) or [Postman](https://www.postman.com/) will work.
Application produces OpenAPI definition that can be downloaded from path `/swagger/v1/swagger.json`. This format can be consumed by varios API testing tools.

You'll need [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed on the host to build and run application.

### Building the Application
Nothing special there - just run `dotnet build` and MSBuild will do the rest for you!

### Quick Setup
1. Build application;
1. Update a few required settings:
    1. Ensure `CV:Capture:Source` is set to the proper source;
    2. Ensure MQTT settings are correct;
    3. Set `StreamingAllowed` to _True_ to enable video translations on debug page;
2. Deploy the application (either run [dotnet publish](https://docs.microsoft.com/ru-ru/dotnet/core/tools/dotnet-publish) or just move `NVs.OccupancySensor.API` folder to the preferred location);
3. Start the app by running `dotnet occ-sensor.dll` in the target folder. By default app will start listening on port 5000;
4. Open `/debug.html` URL in your favorite browser. If application was successfully deployed you should see the debug page;
5. Start the sensor by making a POST HTTP request to `/api/v1/Sensor/Start`. You can download API definition from `/swagger/v1/swagger.json` and use API explorer like Swagger-UI or Postman to simplify requests submission;
6. Refresh the debug page;
7. Adjust the position of your camera using the translations on the debug page;
8. Start MQTT adapter by making a POST HTTP request to `/api/v1/MQTTAdapter/Start`;
9. Ensure that sensor started to publish MQTT topics to the server;
10. If everything is fine, set `StreamingAllowed` setting to _False_ to improve your privacy;
11. Additionaly, update `StartSensor` and `StartMQTT` settings to _True_ to enable automated start after reboots;
12. Restart application;
13. Read the docs below.
# Docs
## Configuration
On a high level, application is doing the following actions to find out if someone is present in the room:
1. Camera captures an image;
1. Captured image is getting processed by denoising block;
1. Application builds the foreground mask from the denoised image;
1. Foreground mask is getting adjusted using the correction mask to exclude false-positives (TV screen, edges of objects...); 
1. Finally, detector computes foreground/background pixel ratio;
1. Computed value is getting compared with the threshold: if value is greater than threshold, application decides that someone is present in the room;
1. Detection results are getting published to MQTT broker by MQTT client.

Application has fair amount of configurable items: 
* Most of the processing steps mentioned above have some settings to tweak, and most of these settings have defaults;
* Serilog is used to produce the logs;
* And since it's ASP.Net Core application you can configure it's settings, like "Urls", "AllowedHosts" etc.

App uses .Net Core configuration, so you can change the settings by:
* updating appsettings.json with the values you need;
* overriding the properties using environment variables.
#### Camera
* `CV:Capture:Source` - the source for OpenCV capture. Can be either integer value that identifies your local camera, file path or link to the stream. Default is _"0"_. This value is used to create [EmguCV VideoCapture](http://www.emgu.com/wiki/files/4.4.0/document/html/961857d0-b7ba-53d8-253a-5059bb3bc1df.htm)
* `CV:Capture:FrameInterval` - the timespan that defines how often application will request new frames from the camera. Default is _100 milliseconds_.
#### Denoising
* `CV:Denoising:Algorithm` - denoising algorithm to use. Default is _None_
    * `None` - no denoising performed. The image captured from the camera goes directly to detection logic
    * `FastNlMeans` - ([FastNlMeansDenoising](https://docs.opencv.org/4.5.1/d5/d69/tutorial_py_non_local_means.html) function used, documentation can be found [here](https://emgu.com/wiki/files/4.5.1/document/html/58b1b703-e4a2-94d9-4843-efe674bae0a3.htm))
There are several algorithm options that may be adjusted:
        * `CV:Denoising:FastNlMeans:H` - rational, optional. Default is _3_`
        * `CV:Denoising:FastNlMeans:TemplateWindowSize` - odd integer, optional. Default is _7_
        * `CV:Denoising:FastNlMeans:SearchWindowSize` - odd integer, optional. Default is _21_
    * `FastNlMeansColored` - ([FastNlMeansColoredDenoisingColored](https://docs.opencv.org/4.5.1/d5/d69/tutorial_py_non_local_means.html) function used, documentation can be found [here](https://emgu.com/wiki/files/4.5.1/document/html/55cd7112-6814-99e7-76f4-ce3b8b8d0694.htm))
There are several algorithm options that may be adjusted:
        * `CV:Denoising:FastNlMeans:H` - rational, optional. Default is _3_`
        * `CV:Denoising:FastNlMeans:HColor` - rational, optional. Default is _3_
        * `CV:Denoising:FastNlMeans:TemplateWindowSize` - odd integer, optional. Default is _7_
        * `CV:Denoising:FastNlMeans:SearchWindowSize` - odd integer, optional. Default is _21_
    * `MedianBlur` - [MedianBlur](https://emgu.com/wiki/files/4.5.1/document/html/32b54325-0d91-bedb-60b4-910e4c65a8db.htm) function used. The only adjustable parameter is:
        * `CV:Denoising:MedianBlur:K` - odd integer greater then 1, optional. Default is _3_
#### Background Subtraction
* `CV:Subtraction:Algorithm` - background subtraction algorithm to use. Default is _CNT_.
    * `CNT` - [CouNT](https://sagi-z.github.io/BackgroundSubtractorCNT/) subtraction algorithm created by [sagi-z](https://github.com/sagi-z). 
This algorithm has a few settings to tweak ([documentation](https://sagi-z.github.io/BackgroundSubtractorCNT/doxygen/html/index.html)):
        * `CV:Subtraction:CNT:MinPixelStability` - integer, optional. Default is _15_
        * `CV:Subtraction:CNT:UseHistory` - boolean, optional. Default is _True_
        * `CV:Subtraction:CNT:MaxPixelStability` - integer, optional. Default is _900_
        * `CV:Subtraction:CNT:IsParallel` - boolean, optional. Default is _True_
#### Correction
* `CV:Correction:Algorithm` - a post-processing correction options, optional. Default is _None_. Valid values are:
    * `None` - no correction will be performed
    * `StaticMask` - an additional static mask will be applied to the computed foreground mask. This mode uses additional parameter:
        * `CV:Correction:StaticMask:PathToFile` - a path to the static mask, optional. Default is _"data/correction_mask.bmp"_. Should be a bi-colored (black and white) bitmap.
#### Detection
* `CV:Detection:Threshold` - a rational value between 0 and 1 that defines sensor sensitivity. Bigger values makes detector less sensitive. Default is _0.1_
#### MQTT
Application uses MQTT.Net to build MQTT client. The following settings used to connect application to MQTT broker:
* `MQTT:ClientId` - the string value that defines client identifier for MQTT client. Required. Does not have a default value
* `MQTT:Server` - string that containts IP address or DNS name of MQTT server. Required. Does not have a default value
* `MQTT:Port` - integer, TCP port of MQTT server. Optional. Default is _1883_
* `MQTT:User` - string, username used to authenticate client on the server. Required. Does not have a default value
* `MQTT:Password` - string, password used to authenticate client on the server. Required. Does not have a default value
* `MQTT:Reconnect:AttemptsCount` - integer, count of attemps to automatically reconnect to the server if connection was lost. Default is _0_
* `MQTT:Reconnect:IntervalBetweenAttempts` - time span, base delay detween two attemps. Application uses progressive delays, multiplying this value to the current attempt number. Default is _00:00:00_
#### Startup
* `StartSensor` - boolean toggle to start sensor on startup. Default _False_
* `StartMQTT` - boolean toggle to start MQTT client on startup. Default _False_
* `StreamingAllowed` - boolean toggle to enable or disble streaming of incoming video and processed results. Default _False_
#### Logging
This app uses Serilog to capture logs, with `File` and `Console` sinks available. Please refer to the documentation for [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration).
Startup process gets logged to `startup.ndjson` file in the application working directory. Rolling interval is set to 1 day for this log. Application will keep last 10 startup.ndjson log files. This behaviour is hardcoded.
#### Swagger
When built in debug, application exposes Swagger UI on URL `/swagger/index.html`. This UI allows to explore and interact with HTTP API exposed by this app.
