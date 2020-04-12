# Aggregator functions in durable function framework
This sample code has showed how to track the changes in an entity and aggregate the results. The code is developed on .NET Core 3.1 and function v3 in Visual Studio 2019

## Installed Packages
Microsoft.NET.Sdk.Functions version 3 (3.0.5) and Microsoft.Azure.WebJobs.Extensions.DurableTask 2 (2.2.0)

## Code snippets
### Http trigger function
This is a http trigger function and the entry point for the application
```
        [FunctionName("online_users_HttpStart")]
        public static async Task<HttpResponseMessage> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
        [DurableClient] IDurableEntityClient entityClient, ILogger log)
        {
           
            EntityId entityId = new EntityId("Counter", "user-count");
            await entityClient.SignalEntityAsync(entityId, "add", 5);
            var state = await entityClient.ReadEntityStateAsync<JValue>(entityId);
            return req.CreateResponse(HttpStatusCode.OK, state.EntityState);
        }
```

### Entity function
This function makes changes to the entity and track all the changes per given time period
```
        [FunctionName("Counter")]
        public static void Counter([EntityTrigger] IDurableEntityContext ctx)
        {
            int currentValue = ctx.GetState<int>();

            switch (ctx.OperationName.ToLowerInvariant())
            {
                case "add":
                    int amount = ctx.GetInput<int>();
                    currentValue += amount;
                    break;
                case "substract":
                    amount = ctx.GetInput<int>();
                    currentValue -= amount;
                    break;
                case "reset":
                    currentValue = 0;
                    break;
                case "get":
                    ctx.Return(currentValue);
                    break;
            }

            ctx.SetState(currentValue);
        }
```
