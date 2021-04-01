# NV's Occupancy Sensor
## What's that?
Containerized ASP.Net Core application and MQTT client that uses computer vision to identify someone's presence in the room.
Uses background subtraction algorithms to identify if camera sees something which is not a part of furnishings.
MQTT client supports configuration convention used by Home Assistant [MQTT integration](https://www.home-assistant.io/docs/mqtt/)
## How to use it?
Setup the app to connect it with your camera, MQTT server and have fun!
You can run it as a regular ASP.Net Core application on Windows host (x86_64) or build a docker image to run it on Linux (x86_64 or arm32). Application can use local cameras attached to host, IP cameras (at least the ones which send MJPEG streams) or even a video file.

## Getting Started
### Prerequisites
#### Hardware
Place your camera in the area you'd like to monitor.
Find an appropriate host machine that will run the app. Depending on configuration, application may consume significant amount of CPU and RAM, so it would be hard to provide a _minimal_ and _recommended_ configurations.
It works on Raspberry Pi 4 with 4 Gb RAM, so you can try something similar or more powerful.
This application does not provide anything that can control external devices or send notifications to end users out of the box. A home automation server with MQTT support would be required to setup various integrations which depends on someone's precense in the room. In most of the cases having a separate host for home automation server would be recommended option.

#### Software
Prepare your favorite API Explorer. [swagger-ui](https://swagger.io/docs/open-source-tools/swagger-ui/usage/installation/) or [Postman](https://www.postman.com/) will work.
Application OpenAPI definition that can be downloaded from path `/swagger/v1/swagger.json`. This format can be consumed by varios API testing tools.
Depending on the hosting option you prefer, you'll need either [.Net Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-3.1.407-windows-x64-installer) or [Docker](https://docs.docker.com/get-docker/) installed on the host to build and run application.

#### Patience (optional)
In case you chose to use Docker, please ensure that you can spend couple of hours building the image. Compilation of EmguCV is the part of Docker image and it may take several hours if you're doing it on weak device.

### Building the application
Nothing special here - either build it using `dotnet build` or `docker build`. The example for Docker can be found below

### Configuration
TBD

## Concept
On a high level, application is doing the following actions to find out if someone is present in the room:
1. Camera captures an image;
1. Captured image is getting processed by denoising block;
1. Application builds the foreground mask from the denoised image;
1. Foreground mask is getting adjusted using the correction mask to exclude false-positives (TV screen, edges of objects...); 
1. Finally, detector computes foreground/background pixel ratio;
1. Computed value is getting compared with the threshold: if value is greater than threshold, application decides that someone is present in the room;
1. Detection results are getting published to MQTT broker by MQTT client.
## Configuration
Application has fair amount of configurable items: 
* Most of the processing steps mentioned above have some settings to tweak, and most of these settings have defaults.
* Serilog is used to produce the logs.
* And since it's ASP.Net Core application you can configure it's settings, like "AllowedHosts" etc.

App uses .Net Core configuration, so you can change the settings by:
* updating appsettings.json with the values you need
* overriding the properties using environment variables
#### Camera
* `CV:Capture:Source` - the source for OpenCV capture. Can be either integer value that identifies your local camera, file path or link to the stream. Default is _"0"_. This value is used to create [EmguCV VideoCapture](http://www.emgu.com/wiki/files/4.4.0/document/html/961857d0-b7ba-53d8-253a-5059bb3bc1df.htm)
* `CV:Capture:FrameInterval` - the timespan that defines how often application will request new frames from the camera. Default is _100 milliseconds_
#### Denoising
* `CV:Denoising:Algorithm` - denoising algorithm to use. Default is _None_
    * `None` - no denoising performed. The image captured from the camera goes directly to detection logic
    * `FastNlMeans` - ([FastNlMeansDenoising](https://docs.opencv.org/4.5.1/d5/d69/tutorial_py_non_local_means.html) function used, documentation can be found [here](https://emgu.com/wiki/files/4.5.1/document/html/58b1b703-e4a2-94d9-4843-efe674bae0a3.htm)).
There are several algorithm options that may be adjusted:
        * `CV:Denoising:FastNlMeans:H` - rational, optional. Default is _3_`
        * `CV:Denoising:FastNlMeans:TemplateWindowSize` - odd integer, optional. Default is _7_
        * `CV:Denoising:FastNlMeans:SearchWindowSize` - odd integer, optional. Default is _21_
    * `FastNlMeansColored` - ([FastNlMeansColoredDenoisingColored](https://docs.opencv.org/4.5.1/d5/d69/tutorial_py_non_local_means.html) function used, documentation can be found [here](https://emgu.com/wiki/files/4.5.1/document/html/55cd7112-6814-99e7-76f4-ce3b8b8d0694.htm)).
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
* `Version` field in appsettings.json is not actually a setting :) . MQTT adapter sends it to Home Assistant as a part of configuration topic.
## Building notes
Thanks to docker, container creation is pretty simple. Use `Dockerfile` in the repository root to create an arm32 image. 

Use `NVs.OccupancySensor.API/Dockerfile` to create x86_64 image. This image works well with Visual Studio and allows to debug a container from the IDE without additional configuration on solution level (you still need to setup docker on your host machine and enable linux containers support). 

Please note that image creation steps contain compilation of OpenCV and EmguCV - it may take significant time to get created. In my case it took about 5 hours to build it on Raspberry Pi 4. 
#### Docker example
Build:
```sh
#/bin/bash
docker build -t occupancy_sensor .
```
Create a volume:
```sh
#/bin/bash
docker volume create occupancy_sensor_data
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
    -v occupancy_sensor_data:/app/data
    --rm \
    occupancy_sensor
```
#### Swagger
When built in debug, application exposes Swagger UI on URL `/swagger/index.html`. This UI allows to explore and interact with HTTP API exposed by this app.
#### Known issues as of January 2021
* Cross-compilation using `qemu-user-static` on x86_64 machine may fail during dotnet build - dotnet currently does not support QEMU. See [this comment](https://github.com/dotnet/dotnet-docker/issues/1512#issuecomment-562180086) for more details
* Docker image creation fails on `apt-get update` when building on "Raspbian 10 GNU/Linux buster" - `libseccomp2` package needs to be updated on host machine to fix the original issue. Please refer to details [here](https://askubuntu.com/questions/1263284/apt-update-throws-signature-error-in-ubuntu-20-04-container-on-arm) and [here](https://github.com/moby/moby/issues/40734) 
