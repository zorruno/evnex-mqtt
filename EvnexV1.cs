/*
;    Project:       AnkoHanse/EVNEX
;
;    (C) 2019       Anko Hanse
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
    ///   Nuget package 'Amazon.Extensions.CognitoAuthentication' 1.0.3 or later
    ///   Nuget package 'NLog' 4.0.0 or later
    /// 
    /// Note: this version has become obsolete as of 2020. Use EvnexV2 class instead.
    /// </summary>
	public class EvnexV1
	{
		// Consts
		private const string			COGNITO_USER_POOL_ID	= "ap-southeast-2_zWnqo6ASv";
		private const string			COGNITO_CLIENT_ID		= "rol3lsv2vg41783550i18r7vi";

		private const string            EVNEX_BASE_URL			= "https://client-api.evnex.io";

        // Helpers
        private static	NLog.Logger    logger					= NLog.LogManager.GetCurrentClassLogger();
		private static	HttpClient     httpClient				= new HttpClient();

		// Members
		private readonly string 		m_evnexUsername			= null;
		private readonly string			m_evnexPassword			= null;

		private string					m_idToken				= null;
		private string					m_accessToken			= null;
		private string					m_refreshToken			= null;

	
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="username"></param>
		/// <param name="password"></param>
		public EvnexV1(string username, string password)
		{
			m_evnexUsername = username;
			m_evnexPassword = password;
		}


		public async Task<dynamic> GetDevices()
		{
			return await _GET( $"/devices" );
		}


		public async Task<dynamic> GetDevice(string deviceId)
		{
			return await _GET( $"/devices/{deviceId}" );
		}


		public async Task<dynamic> GetDeviceTransactions(string deviceId)
		{
			return await _GET( $"/devices/{deviceId}/transactions" );
		}


		public async Task<bool> StopTransaction(string deviceId, int connectorId)
		{
			string      url     = $"/devices/{deviceId}/stop-transaction";
			string      data    = $"{{ \"connectorId\": {connectorId} }}";

			return await _POST(url, data);
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
				logger.Info("EvnexV1 Login succeeded");
				return true;
			}
			catch (Exception ex)
			{
				logger.Error(ex, "EvnexV1 Login failed. " + ex.Message);
			}
			return false;
		}


		private async Task<dynamic> _GET(string url)
		{ 
			try
			{ 
				if (!await _Login())
					return null;

				string sUrl = (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) ? url : EVNEX_BASE_URL + url;

				using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, sUrl))
				{
					req.Headers.Add("Authorization", m_accessToken);

					using (HttpResponseMessage rsp = await httpClient.SendAsync(req))
					{
						if (rsp.IsSuccessStatusCode)
						{
							string  result    = await rsp.Content.ReadAsStringAsync();
							dynamic resultObj = JsonConvert.DeserializeObject(result);

							if (resultObj.error == null)
							{
								logger.Info($"EvnexV1 GET {url} succeeded");
								logger.Trace( result );
								return resultObj.data;
							}
							else
							{
								logger.Info($"EvnexV1 GET {url} response {0}", resultObj.error);
							}
						}
						else
						{
							logger.Info($"EvnexV1 GET {url} response {0} {1}", rsp.StatusCode, rsp.ReasonPhrase);
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"EvnexV1 GET {url} failed. " + ex.Message);
			}
			return false;
		}


		private async Task<bool> _POST(string url, string json)
		{ 
			try
			{
				if (!await _Login())
					return false;
				
				string sUrl = (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)) ? url : EVNEX_BASE_URL + url;

				using (HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, sUrl))
				{
					req.Headers.Add("Authorization", m_accessToken);
					req.Content = new StringContent(json, Encoding.UTF8, "application/json");

					using (HttpResponseMessage response = await httpClient.SendAsync(req))
					{
						if (response.IsSuccessStatusCode)
						{
							string result     = await response.Content.ReadAsStringAsync();
							dynamic resultObj = JsonConvert.DeserializeObject(result);

							if (resultObj.error == null)
							{
								logger.Info($"EvnexV1 POST {url} succeeded with status: '{0}'", resultObj.response?.status);
								logger.Trace( result );
								return true;
							}
							else
							{
								logger.Info($"EvnexV1 POST {url} response {0}", resultObj.error);
							}
							return true;
						}
						else
						{
							logger.Info($"EvnexV1 POST {url} response {0} {1}", response.StatusCode, response.ReasonPhrase);
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.Error(ex, $"EvnexV1 POST {url} failed. " + ex.Message);
			}
			return false;
		}


	}

}