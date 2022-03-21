/*
;    Project:       AnkoHanse/EVNEX
;
;    (C) 2019-2022  Anko Hanse
;
; Permission is hereby granted, free of charge, to any person obtaining a copy
; of this software and associated documentation files (the "Software"), to deal
; in the Software without restriction, including without limitation the rights
; to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
; copies of the Software, and to permit persons to whom the Software is
; furnished to do so, subject to the following conditions:
;
; The above copyright notice and this permission notice shall be included in
; all copies or substantial portions of the Software.
;
; THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
; IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
; FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
; AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
; LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
; OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
; THE SOFTWARE.
*/

using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Newtonsoft.Json;
using NLog;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AnkoHanse.EVNEX
{
    /// <summary>
    /// API to extract information from EVNEX charge-points
    /// 
    /// Target Framework:  
    ///   .NET Core 3.1 or later
    /// 
    /// Dependencies:
    ///   add reference to Nuget package 'Amazon.Extensions.CognitoAuthentication' 1.0.3 or later
    ///   add reference to Nuget package 'NLog' 4.0.0 or later
    /// </summary>
	public class EvnexV2
	{
		// Consts
		private const string				COGNITO_USER_POOL_ID	= "ap-southeast-2_zWnqo6ASv";
		private const string				COGNITO_CLIENT_ID		= "rol3lsv2vg41783550i18r7vi";

		private const string				EVNEX_BASE_URL			= "https://client-api.evnex.io";

        // Helpers
        private static	NLog.Logger			logger					= NLog.LogManager.GetCurrentClassLogger();
		private static  HttpClientHandler	httpHandler             = new HttpClientHandler() { AutomaticDecompression = System.Net.DecompressionMethods.GZip };
		private static	HttpClient			httpClient				= new HttpClient(httpHandler);

		// Members
		private readonly string 			m_evnexUsername			= null;
		private readonly string				m_evnexPassword			= null;

		private string						m_idToken				= null;
		private string						m_accessToken			= null;
		private string						m_refreshToken			= null;

	
		/// <summary>
		/// EvnexV2 Constructor
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		public EvnexV2(string username, string password)
		{
			m_evnexUsername = username;
			m_evnexPassword = password;
		}


		/// <summary>
		/// dynamic GetUser()
		/// 
		/// Result:
		/// {
		///		"email":"email@domain.com",
		///		"id":"de8138ab-9286-4f2a-ab02-b058b760b547",
		///		"name":"email@domain.com",
		///		"createdDate":"2019-09-26T05:26:53.225Z",
		///		"organisations":[{
		///			"id":"9BCA0BF0-D412-42FC-8D56-A55D9DFC1BD1",
		///			"name":"email@domain.com",
		///			"slug":"9BCA0BF0-D412-42FC-8D56-A55D9DFC1BD1",
		///			"tier":0,
		///			"isDefault":true,	
		///			"role":0
		///		}],
		///		"updatedDate":"2020-02-18T21:56:38.501Z"
		///	}
		/// </summary>
		/// <returns></returns>
		public async Task<dynamic> GetUser()
		{
			return await _GET( $"/v2/apps/user" );
		}


		/// <summary>
		/// dynamic GetOrg(string orgId)
		/// 
		/// Result:
		/// {
		///		"id":"9BCA0BF0-D412-42FC-8D56-A55D9DFC1BD1",
		///		"name":"email@domain.com",
		///		"slug":"9BCA0BF0-D412-42FC-8D56-A55D9DFC1BD1",
		///		"tier":0,
		///		"createdDate":"2020-02-18T21:56:38.085Z",
		///		"updatedDate":"2020-02-18T21:56:38.085Z"
		///	}
		/// </summary>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public async Task<dynamic> GetOrg(string orgId)
		{
			return await _GET( $"/v2/apps/organisations/{orgId}" );
		}


		/// <summary>
		/// dynamic GetOrgChargePoints(string orgId)
		/// 
		/// Result:
		/// {
		///		"items":[{
		///			"connectors":[{
		///				"amperage":32,
		///				"connectorFormat":"SOCKET",
		///				"connectorId":"1",
		///				"connectorType":"IEC_62196_T2",
		///				"evseId":"ENX-ED00387B4-1",
		///				"ocppCode":"NoError",
		///				"ocppStatus":"AVAILABLE",
		///				"powerType":"AC_1_PHASE",
		///				"status":"AVAILABLE",
		///				"updatedDate":"2020-05-09T04:17:20.670Z",
		///				"voltage":240
		///			}],
		///			"createdDate":"2019-10-27T20:08:02.000Z",
		///			"details":{
		///				"firmware":"1.3.0",
		///				"iccid":"8964050087213569217",
		///				"model":"R7-T2S-3G",
		///				"vendor":"Evnex"
		///			},
		///			"id":"F46FB7E3-9650-429F-8267-0FD2C843AB7B",
		///			"lastHeard":"2020-05-09T22:22:20.042Z",
		///			"metadata":{"referenceId":"V005731 - AB19356019"},
		///			"name":"MyChargePoint",
		///			"networkStatus":"ONLINE",
		///			"serial":"AB19356019",
		///			"updatedDate":"2020-05-09T04:17:20.670Z",
		///			"location":{
		///				"address":{
		///					"address1":"123 Sesame Street",
		///					"city":"Muppetville",
		///					"country":"Moon"
		///				},
		///				"chargePointCount":1,
		///				"coordinates":{"latitude":"36.8681084","longitude":"74.559621"},
		///				"createdDate":"2020-02-18T21:56:39.485Z",
		///				"id":"0F312EE8-4683-4CC9-9430-742434D0EC54",
		///				"name":"MyChargePoint",
		///				"updatedDate":"2020-02-18T21:56:39.485Z"
		///			}
		///		}]
		/// }
		/// </summary>
		/// <param name="orgId"></param>
		/// <returns></returns>
		public async Task<dynamic> GetOrgChargePoints(string orgId)
		{
			return await _GET( $"/v2/apps/organisations/{orgId}/charge-points" );
		}


		/// <summary>
		/// dynamic GetOrgInsights(string orgId, int nDays)
		/// 
		/// Result:
		/// {
		/// 	"items":[{
		/// 		"carbonOffset":2212.2,
		/// 		"costs":[{
		/// 			"cost":1.69602,
		/// 			"currency":"USD"
		/// 		}],
		/// 		"duration":7487000,
		/// 		"powerUsage":7374,
		/// 		"sessions":1,
		/// 		"startDate":"2020-05-03T00:00:00.000Z"
		/// 	},
		/// 	{"carbonOffset":0,"costs":[{"cost":0,"currency":"USD"}],"duration":0,"powerUsage":0,"sessions":0,"startDate":"2020-05-04T00:00:00.000Z"},
		/// 	{"carbonOffset":4523.099999999999,"costs":[{"cost":3.46771,"currency":"USD"}],"duration":19756000,"powerUsage":15077,"sessions":1,"startDate":"2020-05-05T00:00:00.000Z"},
		/// 	{"carbonOffset":0,"costs":[{"cost":0,"currency":"USD"}],"duration":0,"powerUsage":0,"sessions":0,"startDate":"2020-05-06T00:00:00.000Z"},
		/// 	{"carbonOffset":0,"costs":[{"cost":0,"currency":"USD"}],"duration":0,"powerUsage":0,"sessions":0,"startDate":"2020-05-07T00:00:00.000Z"},
		/// 	{"carbonOffset":0,"costs":[{"cost":0,"currency":"USD"}],"duration":0,"powerUsage":0,"sessions":0,"startDate":"2020-05-08T00:00:00.000Z"},
		/// 	{"carbonOffset":0,"costs":[{"cost":0,"currency":"USD"}],"duration":0,"powerUsage":0,"sessions":0,"startDate":"2020-05-09T00:00:00.000Z"}]
		/// }
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="nDays">Typically 7 or 30</param>
		/// <returns></returns>
		public async Task<dynamic> GetOrgInsights(string orgId, int nDays)
		{
			return await _GET( $"/v2/apps/organisations/{orgId}/summary/insights?days={nDays}" );
		}


		/// <summary>
		/// dynamic GetChargePoint(string cpId)
		/// 
		/// Result:
		/// {
		/// 	"connectors":[{
		/// 		"amperage":32,
		/// 		"connectorFormat":"SOCKET",
		/// 		"connectorId":"1",
		/// 		"connectorType":"IEC_62196_T2",
		/// 		"evseId":"ENX-ED00387B4-1",
		/// 		"meter":{
		///				"powerType":"AC_1_PHASE",
		///				"power":3544.8,
		///				"register":877850,
		///				"frequency":50,
		///				"updatedDate":"2020-05-11T08:20:01.522Z",
		///				"current":15.63,"voltage":227.4
		///			},
		/// 		"ocppCode":"NoError",
		/// 		"ocppStatus":"CHARGING",
		/// 		"powerType":"AC_1_PHASE",
		/// 		"status":"CHARGING",
		/// 		"updatedDate":"2020-05-09T04:17:20.670Z",
		/// 		"voltage":240
		/// 	}],
		/// 	"createdDate":"2019-10-27T20:08:02.000Z",
		/// 	"details":{
		/// 		"firmware":"1.3.0",
		/// 		"iccid":"8964050087213569217",
		/// 		"model":"R7-T2S-3G",
		/// 		"vendor":"Evnex"
		/// 	},
		/// 	"id":"F46FB7E3-9650-429F-8267-0FD2C843AB7B",
		/// 	"lastHeard":"2020-05-09T21:52:19.707Z",
		/// 	"metadata":{"referenceId":"V005731 - AB19356019"},
		/// 	"name":"MyChargePoint",
		/// 	"networkStatus":"ONLINE",
		/// 	"serial":"AB19356019",
		/// 	"updatedDate":"2020-05-09T04:17:20.670Z"
		///		"location":{
		///			"address":{
		///				"address1":"123 Sesame Street",
		///				"city":"Muppetville",
		///				"country":"Moon"
		///			},
		///			"chargePointCount":1,
		///			"coordinates":{"latitude":"36.8681084","longitude":"74.559621"},
		///			"createdDate":"2020-02-18T21:56:39.485Z",
		///			"id":"0F312EE8-4683-4CC9-9430-742434D0EC54",
		///			"name":"MyChargePoint",
		///			"updatedDate":"2020-02-18T21:56:39.485Z"
		///		},
		///		"configuration":{
		///			"maxCurrent":32,
		///			"plugAndCharge":true
		///		},
		///		"loadSchedule":{
		///			"chargingProfilePeriods":[
		///				{"limit":32,"start":0},
		///				{"limit":0,"start":82800}
		///			],
		///			"duration":86400,
		///			"enabled":true,
		///			"timezone":"Pacific/Auckland",
		///			"units":"A",
		///			"updatedDate":"2020-05-11T08:23:27.171Z"
		///		}
		/// }
		/// </summary>
		/// <param name="cpId"></param>
		/// <returns></returns>
		public async Task<dynamic> GetChargePoint(string cpId)
		{
			return await _GET( $"/v2/apps/charge-points/{cpId}" );
		}


		/// <summary>
		/// dynamic GetChargePointTransactions(string cpId)
		/// 
		/// Result:
		/// {
		/// 	"items":[{
		/// 		"carbonOffset":4.5230999999999995,
		/// 		"connectorId":"1",
		/// 		"electricityCost":{"cost":3.46771,"currency":"USD"},
		/// 		"endDate":"2020-05-05T07:14:17.000Z",
		/// 		"evseId":"ENX-ED00387B4-1",
		/// 		"id":"129",
		/// 		"powerUsage":15077,
		/// 		"reason":"EvDisconnected",
		/// 		"startDate":"2020-05-05T01:45:01.000Z"
		/// 	},
		/// 	{"carbonOffset":2.2121999999999997,"connectorId":"1","electricityCost":{"cost":1.69602,"currency":"USD"},"endDate":"2020-05-03T05:30:46.000Z","evseId":"ENX-ED00387B4-1","id":"128","powerUsage":7374,"reason":"EvDisconnected","startDate":"2020-05-03T03:25:59.000Z"},
		/// 	{"carbonOffset":2.7771,"connectorId":"1","electricityCost":{"cost":2.12911,"currency":"USD"},"endDate":"2020-05-02T22:42:40.000Z","evseId":"ENX-ED00387B4-1","id":"127","powerUsage":9257,"reason":"EvDisconnected","startDate":"2020-05-02T20:05:52.000Z"},
		/// 	{"carbonOffset":2.3870999999999998,"connectorId":"1","electricityCost":{"cost":1.83011,"currency":"USD"},"endDate":"2020-04-28T07:12:37.000Z","evseId":"ENX-ED00387B4-1","id":"126","powerUsage":7957,"reason":"EvDisconnected","startDate":"2020-04-28T04:57:52.000Z"},
		/// 	{"carbonOffset":1.6340999999999999,"connectorId":"1","electricityCost":{"cost":1.25281,"currency":"USD"},"endDate":"2020-04-12T02:20:48.000Z","evseId":"ENX-ED00387B4-1","id":"125","powerUsage":5447,"reason":"EvDisconnected","startDate":"2020-04-12T00:48:19.000Z"},
		/// 	{"carbonOffset":1.1598,"connectorId":"1","electricityCost":{"cost":0.88918,"currency":"USD"},"endDate":"2020-03-23T05:38:51.000Z","evseId":"ENX-ED00387B4-1","id":"124","powerUsage":3866,"startDate":"2020-03-23T04:33:13.000Z"},
		/// 	{"carbonOffset":0.9204,"connectorId":"1","electricityCost":{"cost":0.70564,"currency":"USD"},"endDate":"2020-03-22T06:12:22.000Z","evseId":"ENX-ED00387B4-1","id":"123","powerUsage":3068,"startDate":"2020-03-22T05:20:17.000Z"},
		/// 	...
		/// 	{"carbonOffset":2.8743,"connectorId":"1","electricityCost":{"cost":2.20363,"currency":"USD"},"endDate":"2019-11-21T11:32:14.000Z","evseId":"ENX-ED00387B4-1","id":"30","powerUsage":9581,"startDate":"2019-11-21T08:49:33.000Z"}]
		/// }
		/// </summary>
		/// <param name="cpId"></param>
		/// <returns></returns>
		public async Task<dynamic> GetChargePointTransactions(string cpId)
		{
			return await _GET( $"/v2/apps/charge-points/{cpId}/transactions" );
		}


		/// <summary>
		/// bool StopChargePointTransaction(string cpId, int connectorId)
		/// 
		/// Result:  true | false
		/// </summary>
		/// <param name="cpId"></param>
		/// <param name="connectorId"></param>
		/// <returns></returns>
		public async Task<dynamic> StopChargePointTransaction(string orgId, string cpId, int connectorId)
		{
			string      url     = $"/v2/apps/organisations/{orgId}/charge-points/{cpId}/commands/remote-stop-transaction";
			string      data    = $"{{ \"connectorId\": {connectorId} }}";

			return await _POST(url, data);
		}


		/// <summary>
		/// dynamic GetLocation(string locId)
		/// 
		/// Result:
		/// {
		/// 	"address":{
		/// 		"address1":"123 Sesame Street",
		/// 		"city":"Muppetville",
		/// 		"country":"Moon"
		/// 	},
		/// 	"chargePointCount":1,
		/// 	"coordinates":{"latitude":"36.8681084","longitude":"74.559621"},
		/// 	"createdDate":"2020-02-18T21:56:39.485Z",
		/// 	"id":"0F312EE8-4683-4CC9-9430-742434D0EC54",
		/// 	"name":"MyChargePoint",
		/// 	"updatedDate":"2020-02-18T21:56:39.485Z",
		/// 	"chargePoints":[{
		/// 		"connectors":[{
		/// 			"amperage":32,
		/// 			"connectorFormat":"SOCKET",
		/// 			"connectorId":"1",
		/// 			"connectorType":"IEC_62196_T2",
		/// 			"evseId":"ENX-ED00387B4-1",
		/// 			"ocppCode":"NoError",
		/// 			"ocppStatus":"AVAILABLE",
		/// 			"powerType":"AC_1_PHASE",
		/// 			"status":"AVAILABLE",
		/// 			"updatedDate":"2020-05-09T04:17:20.670Z",
		/// 			"voltage":240
		/// 		}],
		/// 		"createdDate":"2019-10-27T20:08:02.000Z",
		/// 		"details":{
		/// 			"firmware":"1.3.0",
		/// 			"iccid":"8964050087213569217",
		/// 			"model":"R7-T2S-3G",
		/// 			"vendor":"Evnex"
		/// 		},
		/// 		"id":"F46FB7E3-9650-429F-8267-0FD2C843AB7B",
		/// 		"lastHeard":"2020-05-09T21:52:19.707Z",
		/// 		"metadata":{"referenceId":"V005731 - AB19356019"},
		/// 		"name":"MyChargePoint",
		/// 		"networkStatus":"ONLINE",
		/// 		"serial":"AB19356019",
		/// 		"updatedDate":"2020-05-09T04:17:20.670Z"
		/// 	}],
		/// 	"electricityCosts":{
		/// 		"costs":[{"cost":0.23,"start":0}],
		/// 		"currency":"USD",
		/// 		"duration":86400,
		/// 		"updatedDate":"2020-02-18T21:56:39.000Z"
		/// 	},
		/// 	"timeZone":"Pacific/Auckland"
		/// }
		/// </summary>
		/// <param name="locId"></param>
		/// <returns></returns>
		public async Task<dynamic> GetLocation(string locId)
		{
			return await _GET( $"https://client-api.evnex.io/v2/apps/locations/{locId}" );
		}


		/// <summary>
		/// dynamic SetChargeSchedule(string cpId, bool enabled, int startSecs, int startLimit, int stopSecs, int stopLimit)
		/// 
		/// Result:
		/// {
		///		"chargingProfilePeriods":[
		///			{"limit":32,"start":0},		// 12:00 am in seconds
		///			{"limit":0,"start":82800}	// 11:00 pm in seconds
		///		],
		///		"duration":86400,
		///		"enabled":true,
		///		"timezone":"Pacific/Auckland",
		///		"units":"A",
		///		"updatedDate":"2020-05-11T08:23:27.171Z"
		///	}
		/// </summary>
		/// <param name="cpId"></param>
		/// <param name="enabled"></param>
		/// <param name="startSecs"></param>
		/// <param name="startLimit"></param>
		/// <param name="stopSecs"></param>
		/// <param name="stopLimit"></param>
		/// <returns></returns>
		public async Task<dynamic> SetChargeSchedule(string cpId, bool enabled, int startSecs, int startLimit, int stopSecs, int stopLimit)
		{
			string      url     = $"/v2/apps/charge-points/{cpId}/charge-schedule";

			string      data    = $"{{ \"chargingProfilePeriods\": [" +
											$"{{\"start\":{startSecs},\"limit\":{startLimit}}}," +
											$"{{\"start\":{stopSecs},\"limit\":{stopLimit}}}" +
									   $"]," +
									   $"\"duration\":86400," +
									   $"\"enabled\":{enabled}," + 
									   $"\"units\":\"A\"" +
								  $"}}";

			return await _PUT(url, data);

		}


		///////////////////////////////////////////////////////////////////////////////////////////////////////////////


		private async Task<bool> _Login()
		{
			try
			{
				// Already authenticated?
				if (m_idToken     != null &&
					m_accessToken != null &&
					m_refreshToken != null)
				{
					return true;
				}

				// Autheticate to Amazon to get access token
				AWSCredentials credentials = new Amazon.Runtime.AnonymousAWSCredentials();
				string         systemName  = COGNITO_USER_POOL_ID.Split('_')[0];
				RegionEndpoint region      = RegionEndpoint.GetBySystemName(systemName);
				using (IAmazonCognitoIdentityProvider provider = new AmazonCognitoIdentityProviderClient(credentials, region))
				{
					CognitoUserPool userPool = new CognitoUserPool(COGNITO_USER_POOL_ID, COGNITO_CLIENT_ID, provider);
					CognitoUser     user     = new CognitoUser(m_evnexUsername, COGNITO_CLIENT_ID, userPool, provider);

					InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
					{
						Password = m_evnexPassword
					};

					AuthFlowResponse response = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

					m_idToken      = response.AuthenticationResult.IdToken;
					m_accessToken  = response.AuthenticationResult.AccessToken;
					m_refreshToken = response.AuthenticationResult.RefreshToken;
				}
				logger.Info("EvnexV2 Login succeeded");
				return true;
			}
			catch (Exception ex)
			{
				logger.Error(ex, "EvnexV2 Login failed. " + ex.Message);
			}
			return false;
		}


		private async Task<dynamic> _GET(string url)
		{ 
			try
			{ 
				if (!await _Login())
					return null;

				string sUrl = (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
					           url.StartsWith("https", StringComparison.OrdinalIgnoreCase) ) ? url : EVNEX_BASE_URL + url;

				using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, sUrl))
				{
					req.Headers.Add("authorization", m_accessToken);
					req.Headers.Add("accept", "application/json, text/plain, */*");
					req.Headers.Add("User-Agent", "okhttp/3.12.1");
					req.Headers.Add("Connection", "Keep-Alive");

					using (HttpResponseMessage rsp = await httpClient.SendAsync(req))
					{
						if (rsp.IsSuccessStatusCode)
						{
							string  result    = await rsp.Content.ReadAsStringAsync();
							dynamic resultObj = JsonConvert.DeserializeObject(result);

							if (resultObj.error == null)
							{
								logger.Info($"EvnexV2 GET {url} succeeded");
								logger.Trace( result );
								return resultObj.data;
							}
							else
							{
								logger.Info($"EvnexV2 GET {url} response {resultObj.error}");
							}
						}
						else
						{
							logger.Info($"EvnexV2 GET {url} response {(int)rsp.StatusCode} {rsp.ReasonPhrase}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"EvnexV2 GET {url} failed. " + ex.Message);
			}
			return null;
		}


		private async Task<dynamic> _POST(string url, string json)
		{ 
			try
			{
				if (!await _Login())
					return null;
				
				string sUrl = (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
					           url.StartsWith("https", StringComparison.OrdinalIgnoreCase) ) ? url : EVNEX_BASE_URL + url;

				using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, sUrl))
				{
					req.Headers.Add("Authorization", m_accessToken);
					req.Headers.Add("accept", "application/json, text/plain, */*");
					req.Headers.Add("User-Agent", "okhttp/3.12.1");
					req.Headers.Add("Connection", "Keep-Alive");

					req.Content = new StringContent(json, Encoding.UTF8, "application/json");

					using (HttpResponseMessage response = await httpClient.SendAsync(req))
					{
						if (response.IsSuccessStatusCode)
						{
							string result     = await response.Content.ReadAsStringAsync();
							dynamic resultObj = JsonConvert.DeserializeObject(result);

							if (resultObj.error == null)
							{
								logger.Info($"EvnexV2 POST {url} succeeded");
								logger.Trace( result );
								return resultObj.data;
							}
							else
							{
								logger.Info($"EvnexV2 POST {url} response {resultObj.error}");
								return null;
							}
						}
						else
						{
							logger.Info($"EvnexV2 POST {url} response {(int)response.StatusCode} {response.ReasonPhrase}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"EvnexV2 POST {url} failed. " + ex.Message);
			}
			return null;
		}


		private async Task<dynamic> _PUT(string url, string json)
		{ 
			try
			{
				if (!await _Login())
					return null;
				
				string sUrl = (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
					           url.StartsWith("https", StringComparison.OrdinalIgnoreCase) ) ? url : EVNEX_BASE_URL + url;

				using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Put, sUrl))
				{
					req.Headers.Add("Authorization", m_accessToken);
					req.Headers.Add("accept", "application/json, text/plain, */*");
					req.Headers.Add("User-Agent", "okhttp/3.12.1");
					req.Headers.Add("Connection", "Keep-Alive");

					req.Content = new StringContent(json, Encoding.UTF8, "application/json");

					using (HttpResponseMessage response = await httpClient.SendAsync(req))
					{
						if (response.IsSuccessStatusCode)
						{
							string result     = await response.Content.ReadAsStringAsync();
							dynamic resultObj = JsonConvert.DeserializeObject(result);

							if (resultObj.error == null)
							{
								logger.Info($"EvnexV2 PUT {url} succeeded");
								logger.Trace( result );
								return resultObj.data;
							}
							else
							{
								logger.Info($"EvnexV2 PUT {url} response {resultObj.error}");
								return null;
							}
						}
						else
						{
							logger.Info($"EvnexV2 PUT {url} response {(int)response.StatusCode} {response.ReasonPhrase}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"EvnexV2 PUT {url} failed. " + ex.Message);
			}
			return null;
		}


	}

}