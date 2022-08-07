// From https://github.com/ankohanse/EVNEX
// Unofficial Evnex API, with MQTT support
// added by zorruno Aug 2022
// (be gentle, this is my first ever dotnet project)
//

// https://github.com/ankohanse/EVNEX
using AnkoHanse.EVNEX;
// https://github.com/dotnet/MQTTnet
using MQTTnet;
using MQTTnet.Client;
// https://github.com/rickyah/ini-parser
using IniParser;
using IniParser.Model;

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
string chargepointId = chargepoints.items[0].id;
string connectorId   = chargepoints.items[0].connectors[0].connectorId;
string locationId    = chargepoints.items[0].location.id;

dynamic chargepoint0  = await evnex.GetChargePoint(chargepointId);
string chargepoint0String = chargepoint0.ToString();

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/chargepoint0")
.WithPayload(chargepoint0String)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);

//----------------------------------------------                
// Get transactions of indicated chargepoint
//----------------------------------------------
dynamic transactions = await evnex.GetChargePointTransactions(chargepointId);
string transactionsString = transactions.ToString();

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/transactions")
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

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/insights")
.WithPayload(insightsString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);


//----------------------------------------------                
// Get organization insights for last x days
//----------------------------------------------
dynamic location = await evnex.GetLocation(locationId);
string locationString = location.ToString();

// Build MQTT Message
message = new MqttApplicationMessageBuilder()
.WithTopic(MqttMainTopic + "/location")
.WithPayload(locationString)
.WithQualityOfServiceLevel(0)
.Build();

// Publish our MQTT message
await mqttClient.PublishAsync(message, CancellationToken.None);
