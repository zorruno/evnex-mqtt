# evnex-mqtt
This builds on the unofficial API for EVNEX charge points by Anko Hanse

It has MQTT Support to tell my home automation system whether my car is charging or not.  
Note this is my first ever C# and dotnet project so be gentle...

Version
- 1.0 - Initial use of Anko's API to pull data and push JSON documents to MQTT
- 1.1 - Some work on JSON parsing to push items like 'Chargepoint Status' directly to MQTT topics

Features: 
- Has basic ini file for configuration of user/pass and MQTT parameters
- user and password is the same as your Evnex app
- Uses the unofficial API from Anko Hanse to pull data in JSON from the Evnex cloud service (read only)
- Connects to an MQTT Server and posts values to fixed subtopics

Todo
- currently no MQTT auth or TLS
- one time run... plan is to have it run as a service
- containerise it with docker (dotnet container and S6 addons to daemonize it)
- ~~do some basic JSON parsing to get simple/most commonly used data into MQTT such as charger status~~
- more config options (MQTT subtopic names, frequency of data grab?)
- possibly push some data into influxDB directly

Not currently proposed/Potential bugs
- only allows for one chargepoint currently (but built so it could be modified if you have multiple)
- doesn't allow for any return control (such as 'charge now', 'lock' etc).  Anko's API doesn't either.
- will likely break if Evnex change data structure or JSON names etc
- no idea how often you should pull data from their cloud... I suggest you don't abuse it

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
