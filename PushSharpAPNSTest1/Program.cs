using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushSharp.Apple;
using PushSharp.Core;
using Newtonsoft.Json.Linq;
using System.IO;

namespace PushSharpAPNSTest1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configuration (NOTE: .pfx can also be used here)
            var p = Environment.CurrentDirectory;
            p = Path.Combine(p, "APNSTest2Certificate.p12");

            var config = new ApnsConfiguration(
                ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                p,
                string.Empty);

            // Create a new broker
            var apnsBroker = new ApnsServiceBroker(config);

            apnsBroker.OnNotificationSucceeded += (notification) =>
            {
                Console.WriteLine("Apple Notification Sent!");
            };

            // Wire up events
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    // See what kind of exception it was to further diagnose
                    if (ex is ApnsNotificationException)
                    {
                        ApnsNotificationException notificationException = ex as ApnsNotificationException;
                        // Deal with the failed notification
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;
                    }
                    else
                    {
                        // Inner exception might hold more useful information like an ApnsConnectionException			
                        Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                    }

                    // Mark it as handled
                    return true;
                });
            };

            // Start the broker
            apnsBroker.Start();

            int counter = 0;
            string input = string.Empty;
            Console.WriteLine("Type your message and hit <ENTER>");

            while (input != "exit")
            {
                input = Console.ReadLine();

                if (input != "exit")
                {
                    counter += 1;
                    apnsBroker.QueueNotification(new ApnsNotification
                    {
                        DeviceToken = "07fe4f023fe8a573648669f7ab7815189c7d8a2b23f71b3e52c4034bfb3ae12b",
                        Payload = JObject.Parse("{\"aps\":{\"alert\":\"Testing from Pushsharp. " + System.Environment.NewLine + input + "(" + Environment.MachineName + ": "+ counter.ToString() + ")\",\"badge\":1,\"sound\":\"default\"}}")
                    });
                }
            }

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker

            apnsBroker.Stop();

        }
    }
}
