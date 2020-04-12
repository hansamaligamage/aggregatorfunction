using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DurableTask.Core.Stats;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace aggregator
{
    public static class online_users
    {

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

    }
}