using System;
using System.Linq;
using System.Text;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {

            // Configure MQTT server.
            var optionsBuilder = new MqttServerOptionsBuilder()
                .WithConnectionValidator(c => //validating MQTT Clients.
                {
                    if (c.ClientId.Length < 10)
                    {
                        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedIdentifierRejected;
                    }
                    if (c.Username != "mySecretUser")
                    {
                        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                    }

                    if (c.Password != "mySecretPassword")
                    {
                        c.ReturnCode = MqttConnectReturnCode.ConnectionRefusedBadUsernameOrPassword;
                    }

                    Console.WriteLine($"{c.ClientId} connection validator for c.Endpoint: {c.Endpoint}");
                    c.ReturnCode = MqttConnectReturnCode.ConnectionAccepted;
                })
                .WithSubscriptionInterceptor(context =>
                {
                    if (context.TopicFilter.Topic.StartsWith("admin/foo/bar") && context.ClientId != "theAdmin")
                    {
                        context.AcceptSubscription = false;
                    }

                    if (context.TopicFilter.Topic.StartsWith("the/secret/stuff") && context.ClientId != "Imperator")
                    {
                        context.AcceptSubscription = false;
                        context.CloseConnection = true;
                    }
                    Console.WriteLine($"{context.ClientId} subscribed for Topic: {context.TopicFilter.Topic}");
                })
                .WithApplicationMessageInterceptor(context =>
                {
                    //Intercepting application messages

                    //if (MqttTopicFilterComparer.IsMatch(context.ApplicationMessage.Topic, "/myTopic/WithTimestamp/#"))
                    {
                        Console.WriteLine("WithApplicationMessageInterceptor block merging data");
                        // Replace the payload with the timestamp. But also extending a JSON based payload with the timestamp is a suitable use case.
                       
                        var newData = Encoding.UTF8.GetBytes(DateTime.Now.ToString("O"));
                        var oldData = context.ApplicationMessage.Payload;
                        var mergedData = newData.Concat(oldData).ToArray();
                        context.ApplicationMessage.Payload = mergedData;
                    }

                })
                .WithConnectionBacklog(100)
                .WithDefaultEndpointPort(1884);

            // Start MQTT server
            var mqttServer = new MqttFactory().CreateMqttServer();
            mqttServer.StartAsync(optionsBuilder.Build()).Wait();


            Console.WriteLine("Server is running.");

            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
            mqttServer.StopAsync().Wait();
        }
    }
}
