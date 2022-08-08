# evnex-mqtt
This builds on the unofficial API for EVNEX charge points by Anko Hanse

It has MQTT Support added.

Features: 
- Has an ini file for configuration of user/pass and MQTT parameters
- user and password is the same as your Evnex app
- Uses the unofficial API to pull data in JSON from the Evnex cloud service (read only)
- Connects to an MQTT Server and posts JSON strings to fixed subtopics

Todo
- currently no MQTT auth or TLS
- one time run... plan is to have it run as a service
- containerise it with docker (dotnet container and S6 addons to daemonize it)
- do some basic JSON parsing to get simple/most commonly used data into MQTT such as charger status
- more config options (MQTT subtopic names, )


Disclaimer: the author of this software and the unofficial Evnex API are not associated with EVNEX.com


### Example use
- Rename the evnex.ini
- add your username/pass & MQTT server 
- build in dot net environment
- run

```
[MQTT]
MqttServer=192.168.1.1
MqttMainTopic=evnex

[EVNEX]
EvnexUsername=evnexuser
EvnexPassword=evnexpass
```
