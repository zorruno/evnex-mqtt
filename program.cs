// From https://github.com/ankohanse/EVNEX
// Unofficial Evnex API, with MQTT support
// added by zorruno Aug 2022
// (be gentle, this is my first ever dotnet project)

// https://github.com/ankohanse/EVNEX
using AnkoHanse.EVNEX;
// https://github.com/dotnet/MQTTnet
using MQTTnet;
using MQTTnet.Client;
// https://github.com/rickyah/ini-parser
using IniParser;
using IniParser.Model;
// https://www.newtonsoft.com/json
using Newtonsoft.Json;

// prints JSON to console if true
bool debug = true;

// Creates or loads an INI file in the same directory as your executable
// named EXE.ini (where EXE is the name of the executable)
var parser = new FileIniDataParser();
IniData data = parser.ReadFile("evnex.ini");

// Read values from INI for MQTT
string MqttServer = data["MQTT"]["MqttServer"];
string MqttMainTopic = data["MQTT"]["MqttMainTopic"];

// Read values from INI for Evnex
string EvnexUsername = data["EVNEX"]["EvnexUsername"];
string EvnexPassword = data["EVNEX"]["EvnexPassword"];

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

if (debug)
{
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("User details:");
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine(userString);
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("");
}

// Build MQTT Message
var message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/user")
.WithPayload(userString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);



//----------------------------------------------
// Get indicated organization details
//----------------------------------------------
string  userId = user.id;
string  orgId  = ((IEnumerable<dynamic>)user.organisations).Where(o => o.isDefault).Select(o => o.id).FirstOrDefault();

dynamic org  = await evnex.GetOrg(orgId);
string orgString = org.ToString();

if (debug)
{
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("Org details:");
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine(orgString);
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("");
}

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/org")
.WithPayload(orgString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);
				
//----------------------------------------------                
// Get all chargepoints of indicated organization
//----------------------------------------------
dynamic chargepoints = await evnex.GetOrgChargePoints(orgId);
string chargepointsString = chargepoints.ToString();

if (debug)
{
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("Chargepoints details:");
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine(chargepointsString);
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("");
}

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/chargepoints")
.WithPayload(chargepointsString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);

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

if (debug)
{
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("First Chargepoint Details [Chargepoint 0]:");
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine(chargepointString);
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("");
}

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/details")
.WithPayload(chargepointString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);

//----------------------------------------------                
// Get location details of indicated chargepoint
//----------------------------------------------
dynamic location = await evnex.GetLocation(locationId);
string locationString = location.ToString();

if (debug)
{
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("Location Details (Chargepoint 0):");
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine(locationString);
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("");
}

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/location")
.WithPayload(locationString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);

//----------------------------------------------                
// Get transactions of indicated chargepoint
//----------------------------------------------
dynamic transactions = await evnex.GetChargePointTransactions(chargepointId);
string transactionsString = transactions.ToString();

if (debug)
{
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("First Chargepoint Transactions (Chargepoint 0):");
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine(transactionsString);
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("");
}

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/chargepoint-"+ cp + "/transactions")
.WithPayload(transactionsString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);

//----------------------------------------------                
// Get organization insights for last x days
//----------------------------------------------
dynamic insights = await evnex.GetOrgInsights(orgId, 7);
string insightsString = insights.ToString();

if (debug)
{
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("Organisation insights for X days:");
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine(insightsString);
  Console.WriteLine("--------------------------------------------");
  Console.WriteLine("");
}

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/insights")
.WithPayload(insightsString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);

