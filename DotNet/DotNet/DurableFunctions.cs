using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DotNet
{
    public static class DurableFunctions
    {
        [FunctionName("DurableFunctions")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("DurableFunctions_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("DurableFunctions_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("DurableFunctions_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("DurableFunctions_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.Log(LogLevel.Information, $"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("DurableFunctions_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequestMessage req,
            [DurableClient]IDurableOrchestrationClient starter, string instanceId,
            ILogger log)
        {
            // Function input comes from the request content.

            await starter.StartNewAsync("DurableFunctions", instanceId, (object)null);

            log.Log(LogLevel.Information, $"Started orchestration with ID = '{instanceId}'.");
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}