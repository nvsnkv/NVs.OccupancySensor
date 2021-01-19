# NV's Occupancy Sensor
## What's that?
Containerized ASP.Net Core application and MQTT client that uses computer vision to identify someone's precense in the room.
Uses background subtraction algorithms to identify if camera sees something which is not a part of furnishings.
MQTT client supports configuration convention used by Home Assistant [MQTT integration](https://www.home-assistant.io/docs/mqtt/)
## How to use it?
Setup the app to connect it with your camera, MQTT server and have fun!
Application can be deployed as regular ASP.Net Core application to the Windows host. Dockerfile in the repository allows to build a linux container with this app.
This section will be updated as soon as I start using it with my home automation server. 
## Configuration
There are quite a few things to configure: connection to the camera, sensitivity of the detector and MQTT client.
This app uses Serilog with file and console sinks so it can also be configured.
And since it's ASP.Net Core application you can configure it's settings, like "AllowedHosts" etc.

App uses .Net Core configuration, so you can:
* update appsettings.json with the values you need
* override the properties using environment variables
#### Camera
* `CV:Capture:Source` - the source for OpenCV capture. Required. Can be either integer value that identifies your local camera, file path or link to the stream. Defaul is _"0"_. This value is used to create [EmguCV VideoCapture](http://www.emgu.com/wiki/files/4.4.0/document/html/961857d0-b7ba-53d8-253a-5059bb3bc1df.htm)
* `CV:Capture:FrameInterval` - the timespan that defines how often application will request new frames from the camera. Required. Default is _100 milliseconds_
#### Detection
* `CV:Detection:Threshold` - a rational value between 0 and 1 that defines sensor sensitivity. Required. Bigger values leads makes detector less sensitive. Default is _0.1_
#### MQTT
Application uses MQTT.Net to build MQTT client. Please
* `MQTT:ClientId` - the client identifier for MQTT client. Required. Does not have a default value
* `MQTT:Server` - IP address or DNS name of MQTT server. Required. Does not have a default value
* `MQTT:Port` - TCP port of MQTT server. Required. Default is _1883_
* `MQTT:User` - username used to authenticate client on the server. Required. Does not have a default value
* `MQTT:Password` - password used to authenticate client on the server. Required. Does not have a default value
#### Startup
* `StartSensor` - toggle to start sensor on startup. Optional. Default _False_
* `StartMQTT` - toggle to start MQTT client on startup. Optional. Default _False_
#### Logging
This app uses Serilog to capture logs. Please refer to the documentation for [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration).
Startup process gets logged to `startup.ndjson` file in the application working directory. Rolling interval is set to 1 day for this log. Application will keep last 10 startup.ndjson log files. These behaviour is hardcoded.
#### Version
`Version` field in appsettings.json is not actually a setting :) . MQTT adapter sends it to Home Assistant as a part of configuration topic. It is also used in Swagger as API version.



