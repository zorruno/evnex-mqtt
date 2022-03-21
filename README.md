# EVNEX
This is the unofficial API for EVNEX charge points.

Disclaimer: the author of this software is not associated with EVNEX.com.


### Example use
```
// Initialize using same username and password as used 
// for the iOS/Android EVNEX app
var evnex = new EvnexV2( "evnexUsername", "evnexPassword" );

// Get user
dynamic user = await evnex.GetUser();

Console.WriteLine($"GetUser():");
Console.WriteLine(user);
				
string  userId = user.id;
string  orgId  = ((IEnumerable<dynamic>)user.organisations).Where(o => o.isDefault).Select(o => o.id).FirstOrDefault();

// Get indicated organization details
dynamic org  = await evnex.GetOrg(orgId);

Console.WriteLine($"GetOrg({orgId}):");
Console.WriteLine(org);

// Get all chargepoints of indicated organization
dynamic chargepoints = await evnex.GetOrgChargePoints(orgId);

Console.WriteLine($"GetOrgChargePoints({orgId}):");
Console.WriteLine(chargepoints);

string chargepointId = chargepoints.items[0].id;
string connectorId   = chargepoints.items[0].connectors[0].connectorId;
string locationId    = chargepoints.items[0].location.id;

// Get details of indicated chargepoint
dynamic chargepoint  = await evnex.GetChargePoint(chargepointId);

Console.WriteLine($"GetChargePoint({chargepointId}):");
Console.WriteLine(chargepoint);

// Get transactions of indicated chargepoint
dynamic transactions = await evnex.GetChargePointTransactions(chargepointId);

Console.WriteLine($"GetChargePointTransactions({chargepointId}):");
Console.WriteLine(transactions);

// Get organization insights for last x days
dynamic insights = await evnex.GetOrgInsights(orgId, 7);

Console.WriteLine($"GetOrgInsights({orgId},7):");
Console.WriteLine(transactions);

// Get location details
dynamic location = await evnex.GetLocation(locationId);

Console.WriteLine($"GetLocation({locationId}):");
Console.WriteLine(location);
```


### License
This project is published under the MIT license:

Copyright (c) 2019-2022 Anko Hanse

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
