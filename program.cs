// From https://github.com/ankohanse/EVNEX
// Unofficial Evnex API, with MQTT support
// added by zorruno Aug 2022
// (be gentle, this is my first ever dotnet project)
//
// Version
// 1.0 - zorruno - Aug 2022 - Initial use of Anko's API to pull data and push JSON documents to MQTT
// 1.1 - zorruno - Aug 2022 - Some work on JSON parsing to push items like 'Chargepoint Status' directly to MQTT topics
// 1.2 - zorruno - Aug 2022 - Deleted code that grabs everything except the main charger status points (a copy kept as program-full.cs.old)

// https://github.com/ankohanse/EVNEX
using AnkoHanse.EVNEX;
// https://github.com/dotnet/MQTTnet
using MQTTnet;
using MQTTnet.Client;
// https://github.com/rickyah/ini-parser
using IniParser;
using IniParser.Model;
// https://code-maze.com/introduction-system-text-json-examples/
using System.Text.Json;

// prints various stuff to console if true
bool debug = true;

// Creates or loads an INI file in the same directory as your executable
// named EXE.ini (where EXE is the name of the executable)
var parser = new FileIniDataParser();
IniData iniData = parser.ReadFile("evnex.ini");

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
        // Publish chargepoint status
        // -------------------------------
        var connectorStatus = connectorValue.GetProperty("status"); 
        string connectorStatusString = connectorStatus.ToString();       
        if (debug) Console.WriteLine(connectorStatusString) ;

        // Build MQTT Message
        var message = new MqttApplicationMessageBuilder()
        .WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/status")
        .WithPayload(connectorStatusString)
        .WithQualityOfServiceLevel(0)
        .Build();

        // Publish MQTT message
        await mqttClient.PublishAsync(message, CancellationToken.None);
        
        // -------------------------------
        // Publish chargepoint ocppCode
        // -------------------------------
        var connectorOcppcode = connectorValue.GetProperty("ocppCode"); 
        string connectorOcppcodeString = connectorOcppcode.ToString();       
        if (debug) Console.WriteLine(connectorOcppcodeString) ;

        // Build MQTT Message
        message = new MqttApplicationMessageBuilder()
        .WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/ocppCode")
        .WithPayload(connectorOcppcodeString)
        .WithQualityOfServiceLevel(0)
        .Build();

        // Publish MQTT message
        await mqttClient.PublishAsync(message, CancellationToken.None);

        // -------------------------------
        // Publish chargepoint ocppStatus
        // -------------------------------
        var connectorOcppstatus = connectorValue.GetProperty("ocppStatus"); 
        string connectorOcppstatusString = connectorOcppstatus.ToString();       
        if (debug) Console.WriteLine(connectorOcppstatusString) ;

        // Build MQTT Message
        message = new MqttApplicationMessageBuilder()
        .WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/ocppStatus")
        .WithPayload(connectorOcppstatusString)
        .WithQualityOfServiceLevel(0)
        .Build();

        // Publish MQTT message
        await mqttClient.PublishAsync(message, CancellationToken.None);

    }
