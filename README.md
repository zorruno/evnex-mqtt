# evnex-mqtt

This builds on the unofficial API for EVNEX charge points by Anko Hanse and adds command line and MQTT support.
Disclaimer: the author of this software is not associated with EVNEX.com

```
// -------------------------------------------------------------------------------
// evnex-mqtt
// -------------------------------------------------------------------------------
// This builds on the unofficial API for EVNEX charge points by Anko Hanse from https://github.com/ankohanse/EVNEX
// It currently has simple MQTT Support to publish a status noting whether my car is charging, connected, or in error.
// Note that Anko's code won't be modified, but used purely as the API.
// by zorruno, Aug 2022, 
// This project is published under the MIT license, (c) 2022 zorruno
//
// Note this is my first ever C# and dotnet project so be gentle...
//
// Description:
//  Application to get data from your Evnex chargepoint via the Evnex cloud and publish to MQTT.
//  You must give option of --chargepoint-status and/or --chargepoint-activity to get output to MQTT.
//
// Usage:
//  evnex-mqtt [options]
//
// Options:
//  -i, --ini-file <ini-file>               The location of the ini file. [default: evnex.ini]
//  -d, --debug <Activity|All|None|Status>  Output 'Status', 'Activity' or 'All' info to console (Activity not currently Implemented)
//  -s, --chargepoint-status                Gets chargepoint status information and publishes to MQTT.
//  -a, --chargepoint-activity              Gets chargepoint activity information and publishes to MQTT. (not currently implemented)
//  --version                               Show version information
//  -?, -h, --help                          Show help and usage information
//
// Examples
//  evnex-mqtt -s (Publish status to MQTT, no console output)
//  evnex-mqtt -s -d Status (Publish status to MQTT and show the Status items on the console)
//
// Version
// 1.0 - zorruno - Aug 2022 - Initial use of Anko's API to pull data and push JSON documents to MQTT
// 1.1 - zorruno - Aug 2022 - Some work on JSON parsing to push items like 'Chargepoint Status' directly to MQTT topics
// 1.2 - zorruno - Aug 2022 - Deleted code that grabs everything except the main charger status points (a copy kept as program-full.cs.old)
// 1.3 - zorruno - Aug 2022 - Added command line options (not all yet doing something)
```

Features: 
- Has basic ini file for configuration of user/pass and MQTT parameters
- user and password is the same as your Evnex app
- command line parameters for simple or more complex output (so you can run both at various intervals)
- console output with debug selections
- Uses the unofficial API from Anko Hanse to pull data in JSON from the Evnex cloud service (read only)
- Connects to an MQTT Server and posts values to fixed subtopics
- an example output that was generated by the program-full (with debug on) is included here

Todo
- currently no MQTT auth or TLS
- one time run... plan is to have it run as a service
- containerise it with docker (dotnet container and S6 addons to daemonize it)
- ~~do some basic JSON parsing to get simple/most commonly used data into MQTT such as charger status~~
- more config options (MQTT subtopic names?, frequency of data grab?)
- possibly push some data into influxDB directly

Not currently proposed/Potential bugs
- only allows for one chargepoint currently (but built so it could be modified if you have multiple)
- doesn't allow for any return control (such as 'charge now', 'lock' etc).  Anko's API doesn't either.
- will likely break if Evnex change data structure or JSON names etc
- no idea how often you should pull data from their cloud... I suggest you don't abuse it




### Building
- Rename the evnex-mqtt.ini.sample to evnex-mqtt.ini
- add your username/pass & MQTT server 
- build in dot net environment
- run

### Example use
Publish status items to MQTT, no console output
```evnex-mqtt -s``` 
Publish status to MQTT and show the Status items on the console
```evnex-mqtt -s -d Status``` 

### evnex-mqtt.ini
```
[MQTT]
MqttServer=192.168.1.1
MqttMainTopic=evnex

[EVNEX]
EvnexUsername=evnexuser
EvnexPassword=evnexpass
```
