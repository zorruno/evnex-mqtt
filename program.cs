// -------------------------------------------------------------------------------
// evnet-mqtt
// -------------------------------------------------------------------------------
// This builds on the unofficial API for EVNEX charge points by Anko Hanse from https://github.com/ankohanse/EVNEX
// It has MQTT Support to tell my home automation system whether my car is charging or not.  
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
// Example
//  evnex-mqtt -s (Publish status to MQTT, no console output)
//  evnex-mqtt -s -d Status (Publish status to MQTT and show the Status items on the console)
//
// Version
// 1.0 - zorruno - Aug 2022 - Initial use of Anko's API to pull data and push JSON documents to MQTT
// 1.1 - zorruno - Aug 2022 - Some work on JSON parsing to push items like 'Chargepoint Status' directly to MQTT topics
// 1.2 - zorruno - Aug 2022 - Deleted code that grabs everything except the main charger status points (a copy kept as program-full.cs.old)
// 1.3 - zorruno - Aug 2022 - Added command line options (not all yet doing something)


// https://github.com/ankohanse/EVNEX
using AnkoHanse.EVNEX;
// Local
using EvnexDebugOptions;
// https://github.com/dotnet/MQTTnet
using MQTTnet;
using MQTTnet.Client;
// https://github.com/rickyah/ini-parser
using IniParser;
using IniParser.Model;
// https://code-maze.com/introduction-system-text-json-examples/
using System.Text.Json;
// https://docs.microsoft.com/en-us/dotnet/standard/commandline/
// prerelease at this stage: dotnet add package System.CommandLine --prerelease 
using System.CommandLine;


bool debugStatus = false;
bool mqttPublishStatus = false;

// Future use:
bool debugActivity = false;  
bool mqttPublishActivity = false;

//----------------------------------------------
// Set up System.CommandLine Options
//----------------------------------------------
Option<FileInfo> iniFileOption = new(
    aliases: new[] { "--ini-file", "-i" },
    getDefaultValue: () => new FileInfo("evnex.ini"),
    description: "The location of the ini file.");
Option<Debug> debugOption = new(
    aliases: new[] { "--debug", "-d" },
    description: "Output 'Status', 'Activity' or 'All' info to console (Activity not currently Implemented)");
Option<Boolean> statusOption = new(
    aliases: new[] { "--chargepoint-status", "-s" },
    description: "Gets chargepoint status information and publishes to MQTT.");
Option<Boolean> activityOption = new(
    aliases: new[] { "--chargepoint-activity", "-a" },
    description: "Gets chargepoint activity information and publishes to MQTT. (not currently implemented)");

RootCommand rootCommand = new(description: "Application to get data from your Evnex chargepoint via the Evnex cloud and publish to MQTT. You must give option of --chargepoint-status and/or --chargepoint-activity to get output to MQTT.")
{
    iniFileOption,
    debugOption,
    statusOption,
    activityOption,
};

rootCommand.SetHandler(
    async (FileInfo iniFile, Debug debugSelection, Boolean statusSelected, Boolean activitySelected) =>
    {
        if (debugSelection >= Debug.Status)    debugStatus = true;
        if (debugSelection >= Debug.Activity)  debugActivity = true;
        if (debugSelection >= Debug.All)     { debugStatus = true; debugActivity = true; }
 
        if (statusSelected) mqttPublishStatus = true;

    },
    iniFileOption,
    debugOption,
    statusOption,
    activityOption);

await rootCommand.InvokeAsync(args);
//----------------------------------------------


// Creates or loads an INI file in the same directory as your executable
// named EXE.ini (where EXE is the name of the executable)
var parser = new FileIniDataParser();
IniData iniData = parser.ReadFile("evnex-mqtt.ini");

// Read values from INI for MQTT
string MqttServer = iniData["MQTT"]["MqttServer"];
string MqttMainTopic = iniData["MQTT"]["MqttMainTopic"];

// Read values from INI for Evnex
string EvnexUsername = iniData["EVNEX"]["EvnexUsername"];
string EvnexPassword = iniData["EVNEX"]["EvnexPassword"];

// Set up new MQTT Client
var factory = new MqttFactory();
var mqttClient = factory.CreateMqttClient();

var mqttClientOptions = new MqttClientOptionsBuilder()
    .WithTcpServer(server: MqttServer)
    .Build();

System.Threading.CancellationToken cancellationToken;
await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

// Initialize using same username and password as used 
// for the iOS/Android EVNEX app
var evnex = new EvnexV2( EvnexUsername, EvnexPassword );


//----------------------------------------------
// Get user
//----------------------------------------------
dynamic user = await evnex.GetUser();
string userString = user.ToString();

//----------------------------------------------
// Get indicated organization details
//----------------------------------------------
string  userId = user.id;
string  orgId  = ((IEnumerable<dynamic>)user.organisations).Where(o => o.isDefault).Select(o => o.id).FirstOrDefault();

dynamic org  = await evnex.GetOrg(orgId);
			
//----------------------------------------------                
// Get all chargepoints of indicated organization
//----------------------------------------------
dynamic chargepoints = await evnex.GetOrgChargePoints(orgId);
string chargepointsString = chargepoints.ToString();

//----------------------------------------------                
// Get details of indicated chargepoint
//----------------------------------------------

// Which ID Chargepoint are we going to deal with?
// We will assume only one chargepoint at this stage
// Which is the first one, or 0 in the array
int cp = 0;

string chargepointId = chargepoints.items[cp].id;
string connectorId   = chargepoints.items[cp].connectors[cp].connectorId;
string locationId    = chargepoints.items[cp].location.id;

dynamic chargepoint  = await evnex.GetChargePoint(chargepointId);
string chargepointString = chargepoint.ToString();

// Pull the specific data items we want from the JSON document
var jsonObject = JsonDocument.Parse(chargepointString);

var connectorValues = jsonObject.RootElement.GetProperty("connectors");
    foreach (var connectorValue in connectorValues.EnumerateArray())
    {
        // -------------------------------
        // Get and publish chargepoint status
        // -------------------------------
        var connectorStatus = connectorValue.GetProperty("status"); 
        string connectorStatusString = connectorStatus.ToString();       
        if (debugStatus) Console.WriteLine(connectorStatusString) ;

        if (mqttPublishStatus) 
        {
        // Build MQTT Message
        var message = new MqttApplicationMessageBuilder()
        .WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/status")
        .WithPayload(connectorStatusString)
        .WithQualityOfServiceLevel(0)
        .Build();

        // Publish MQTT message
        await mqttClient.PublishAsync(message, CancellationToken.None);
        }

        // -------------------------------
        // Get and publish chargepoint ocppCode
        // -------------------------------
        var connectorOcppcode = connectorValue.GetProperty("ocppCode"); 
        string connectorOcppcodeString = connectorOcppcode.ToString();       
        if (debugStatus) Console.WriteLine(connectorOcppcodeString) ;

        if (mqttPublishStatus) 
        {
        // Build MQTT Message
        var message = new MqttApplicationMessageBuilder()
        .WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/ocppCode")
        .WithPayload(connectorOcppcodeString)
        .WithQualityOfServiceLevel(0)
        .Build();

        // Publish MQTT message
        await mqttClient.PublishAsync(message, CancellationToken.None);
        }


        // -------------------------------
        // Get and publish chargepoint ocppStatus
        // -------------------------------
        var connectorOcppstatus = connectorValue.GetProperty("ocppStatus"); 
        string connectorOcppstatusString = connectorOcppstatus.ToString();       
        if (debugStatus) Console.WriteLine(connectorOcppstatusString) ;

        if (mqttPublishStatus) 
        {
        // Build MQTT Message
        var message = new MqttApplicationMessageBuilder()
        .WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/ocppStatus")
        .WithPayload(connectorOcppstatusString)
        .WithQualityOfServiceLevel(0)
        .Build();

        // Publish MQTT message
        await mqttClient.PublishAsync(message, CancellationToken.None);
        }

    }
