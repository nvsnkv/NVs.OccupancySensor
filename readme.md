# NV's Occupancy Sensor
## What's that?
Containerized ASP.Net Core application and MQTT client that uses computer vision to identify someone's presence in the room.
Uses background subtraction algorithms to identify if camera sees something which is not a part of furnishings.
MQTT client supports configuration convention used by Home Assistant [MQTT integration](https://www.home-assistant.io/docs/mqtt/)
## How to use it?
Setup the app to connect it with your camera, MQTT server and have fun!
Application can be deployed as regular ASP.Net Core application to the Windows host. Dockerfiles in the repository allows to build a linux container with this app for x86_64 and arm32 architectures. 
#### Docker example
Build:
```sh
#/bin/bash
docker build -t occupancy_sensor .
```
Run:
```sh
#!/bin/bash
docker run -e "CV:Capture:FrameInterval"="00:00:01" \
  -e "MQTT:ClientId"="sensor_dev" \
  -e "MQTT:Server"="127.0.0.1" \
  -e "MQTT:USER"="user" \
  -e "MQTT:Password"="i have no clue" \
  -e "StartSensor"="True" \
  -e "StartMQTT"="True" \
  --device /dev/video0 \ #video device needs to be added to the container if CV:Capture:Source is not a file or URL
  -p 40080:80 \ #container exposes port 80 by default
  --rm \
  occupancy_sensor
```
#### Swagger
When built in debug, application exposes Swagger UI on URL `/swagger/index.html`. This UI allows to explore and interact with HTTP API exposed by this app.
## Configuration
There are quite a few things to configure: connection to the camera, image denoising, detection algorithm, sensitivity of the detector and MQTT client. 
YOu can also tweak a balance between accuracy, performance and resource consumption by tweaking image transformation pipeline settings.
This app uses Serilog with file and console sinks so it can also be configured.
And since it's ASP.Net Core application you can configure it's settings, like "AllowedHosts" etc.

App uses .Net Core configuration, so you can:
* update appsettings.json with the values you need
* override the properties using environment variables
#### Camera
* `CV:Capture:Source` - the source for OpenCV capture. Can be either integer value that identifies your local camera, file path or link to the stream. Default is _"0"_. This value is used to create [EmguCV VideoCapture](http://www.emgu.com/wiki/files/4.4.0/document/html/961857d0-b7ba-53d8-253a-5059bb3bc1df.htm)
* `CV:Capture:FrameInterval` - the timespan that defines how often application will request new frames from the camera. Default is _100 milliseconds_
#### Denoising
* `CV:Denoising:Algorithm` - denoising algorithm to use. Default is _None_
  * `None` - no denoising performed. The image captured from the camera goes directly to detection logic
  * `FastNlMeansColored` - ([FastNlMeansColoredDenoisingColored](https://docs.opencv.org/4.5.1/d5/d69/tutorial_py_non_local_means.html) function used, documentation can be found [here](https://emgu.com/wiki/files/4.5.1/document/html/55cd7112-6814-99e7-76f4-ce3b8b8d0694.htm)).
There are several algorithm options that may be adjusted:
    * `CV:Denoising:FastNlMeans:H` - rational, optional. Default is _3_`
    * `CV:Denoising:FastNlMeans:HColor` - rational, optional. Default is _3_
    * `CV:Denoising:FastNlMeans:TemplateWindowSize` - odd integer, optional. Default is _7_
    * `CV:Denoising:FastNlMeans:SearchWindowSize` - odd integer, optional. Default is _21_
#### Detection
* `CV:Detection:Threshold` - a rational value between 0 and 1 that defines sensor sensitivity. Bigger values makes detector less sensitive. Default is _0.1_
* `CV:Detection:Algorithm` - background subtraction algorithm to use. Default is _CNT_.
  * `CNT` - [CouNT](https://sagi-z.github.io/BackgroundSubtractorCNT/) subtraction algorithm created by [sagi-z](https://github.com/sagi-z). 
This algorithm has a few settings to tweak ([documentation](https://sagi-z.github.io/BackgroundSubtractorCNT/doxygen/html/index.html)):
    * `CV:Detection:CNT:MinPixelStability` - integer, optional. Default is _15_
    * `CV:Detection:CNT:UseHistory` - boolean, optional. Default is _True_
    * `CV:Detection:CNT:MaxPixelStability` - integer, optional. Default is _900_
    * `CV:Detection:CNT:IsParallel` - boolean, optional. Default is _True_
#### MQTT
Application uses MQTT.Net to build MQTT client. The following settings used to connect application to MQTT broker:
* `MQTT:ClientId` - the string value that defines client identifier for MQTT client. Required. Does not have a default value
* `MQTT:Server` - IP address or DNS name of MQTT server. Required. Does not have a default value
* `MQTT:Port` - TCP port of MQTT server. Default is _1883_
* `MQTT:User` - username used to authenticate client on the server. Does not have a default value
* `MQTT:Password` - password used to authenticate client on the server. Does not have a default value
#### Startup
* `StartSensor` - boolean toggle to start sensor on startup. Default _False_
* `StartMQTT` - boolean toggle to start MQTT client on startup. Default _False_
#### Logging
This app uses Serilog to capture logs. Please refer to the documentation for [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration).
Startup process gets logged to `startup.ndjson` file in the application working directory. Rolling interval is set to 1 day for this log. Application will keep last 10 startup.ndjson log files. This behaviour is hardcoded.
#### Version
* `Version` field in appsettings.json is not actually a setting :) . MQTT adapter sends it to Home Assistant as a part of configuration topic. It is also used in Swagger as API version.
* `ApiVersion` defines the version of HTTP API. Used to generate OpenAPI Specification.
## Building notes
Thanks to docker, container creation is pretty simple. Use `Dockerfile` in the repository root to create an arm32 image. 

Use `NVs.OccupancySensor.API/Dockerfile` to create x86_64 image. This image works well with Visual Studio and allows to debug a container from the IDE without additional configuration on solution level (you still need to setup docker on your host machine and enable linux containers support). 

Please note that image creation steps contains compilation of OpenCV and EmguCV - it may take significant time to get created. In my case it took about 5 hours to build it on Raspberry Pi 4.
#### Known issues as of January 2021
* Cross-compilation using `qemu-user-static` on x86_64 machine may fail during dotnet build - dotnet currently does not support QEMU. See [this comment](https://github.com/dotnet/dotnet-docker/issues/1512#issuecomment-562180086) for more details
* Image creation fails on `apt-get update` when building on "Raspbian 10 GNU/Linux buster" - `libseccomp2` package needs to be updated on host machine to fix the original issue. Please refer to details [here](https://askubuntu.com/questions/1263284/apt-update-throws-signature-error-in-ubuntu-20-04-container-on-arm) and [here](https://github.com/moby/moby/issues/40734) 
