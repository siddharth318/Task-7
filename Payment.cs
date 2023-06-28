using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Task_7
{
    public static class Payment
    {
       
        [FunctionName("PaymentFunction")]
        public static async Task<bool> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {   

            log.LogInformation("Received a Payment request");

            //Streaming the body

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var client = new ServiceBusClient("Endpoint=sb://qk-namespace.servicebus.windows.net/;SharedAccessKeyName=sample;SharedAccessKey=woGPU7d5CLPPScDdUajgKubZUt5NDD59n+ASbG9qrIA=;EntityPath=paymnt-queue");


                var sender = client.CreateSender("paymnt-queue");
                var message = new ServiceBusMessage(requestBody);

                if (requestBody.Contains("scheduled"))
                    message.ScheduledEnqueueTime = DateTimeOffset.UtcNow.AddSeconds(15);

                if (requestBody.Contains("ttl"))
                    message.TimeToLive = TimeSpan.FromSeconds(20);

                await sender.SendMessageAsync(message);

                log.LogInformation("returning True");

                return true;
            }
            catch (Exception e)
            {

                return false;
            }


        }

    
    }
}
