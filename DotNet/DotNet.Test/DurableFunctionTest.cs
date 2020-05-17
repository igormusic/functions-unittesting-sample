using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace DotNet.Test
{
    [TestClass]
    public class DurableFunctionTest : FunctionTestHelper.FunctionTest
    {
        [TestMethod]
        public async Task Run_Orchectrator()
        {
            var contextMock = new Mock<IDurableOrchestrationContext>();

            contextMock.Setup(context => context.CallActivityAsync<string>("DurableFunctions_Hello", "Tokyo")).Returns(Task.FromResult<string>("Hello Tokyo!"));
            contextMock.Setup(context => context.CallActivityAsync<string>("DurableFunctions_Hello", "Seattle")).Returns(Task.FromResult<string>("Hello Seattle!"));
            contextMock.Setup(context => context.CallActivityAsync<string>("DurableFunctions_Hello", "London")).Returns(Task.FromResult<string>("Hello London!"));
            var result = await DotNet.DurableFunctions.RunOrchestrator(contextMock.Object);
            Assert.AreEqual("Hello Tokyo!", result[0]);
            Assert.AreEqual("Hello Seattle!", result[1]);
            Assert.AreEqual("Hello London!", result[2]);
        }

        [TestMethod]
        public async Task Run_Hello_Activity()
        {
            var result = DotNet.DurableFunctions.SayHello("Amsterdam", log);
            Assert.AreEqual("Hello Amsterdam!", result);
        }

        [TestMethod]
        public async Task Run_Orchectrator_Client()
        {
            var clientMock = new Mock<IDurableOrchestrationClient>();
            // https://github.com/Azure/azure-functions-durable-extension/blob/0345b369ffa1745c24ffbacfaf8a43fb62dd2572/src/WebJobs.Extensions.DurableTask/DurableOrchestrationClient.cs#L46
            var requestMock = new Mock<HttpRequestMessage>();
            var id = "8e503c5e-19de-40e1-932d-298c4263115b";
            clientMock.Setup(client => client.StartNewAsync("DurableFunctions", id, (object) null)).Returns(Task.FromResult<string>(id));
            var request = requestMock.Object;
            clientMock.Setup(client => client.CreateCheckStatusResponse(request, id, false));
            var result = DotNet.DurableFunctions.HttpStart(request, clientMock.Object,id, log);
            try
            {

                clientMock.Verify(client => client.StartNewAsync("DurableFunctions", id, (object)null));
                clientMock.Verify(client => client.CreateCheckStatusResponse(request, id, false));
            } catch (MockException ex)
            {
                Assert.Fail();
            }
        }
    }
}
