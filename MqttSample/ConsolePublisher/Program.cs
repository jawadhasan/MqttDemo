using System;
using System.Text;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;

namespace ConsolePublisher
{
    class Program
    {
        private static IMqttClient client;

        static void Main(string[] args)
        {
            Console.WriteLine("Publisher loading....");

            // Create a new MQTT client.
            var factory = new MqttFactory();
            client = factory.CreateMqttClient();

            // Create TCP based options using the builder.
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                //.WithTcpServer("localhost")
                .WithTcpServer("broker.hivemq.com")
                //.WithTcpServer("broker.hivemq.com", 1883) // Port is optional
                .WithCredentials("bud", "%spencer%")
                //.WithTls() // Use secure TCP connection
                .WithCleanSession()
                .Build();


            client.Connected += async (s, e) =>
                {
                    await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("test").Build());
                };
            client.ApplicationMessageReceived += (sender, e) =>
            {
                Console.WriteLine($"ApplicationMessageReceived at {DateTime.UtcNow}");
                Console.WriteLine(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
            };

           client.ConnectAsync(options).Wait();


            Console.WriteLine("Publisher Connected. Press any key to simulate message sending!");
            Console.ReadLine();
            SimulatePublish();
            
            Console.WriteLine("Simulation ended! press any key to exit.");
            Console.ReadLine();
        }

        static void SimulatePublish()
        {

            var counter = 0;
            while (counter < 10)
            {
                counter++;
                var testMessage = new MqttApplicationMessageBuilder()
                    .WithTopic("test")
                    .WithPayload($"Payload: {counter}")
                    .Build();

                Console.WriteLine($"publishing at {DateTime.UtcNow}");
                client.PublishAsync(testMessage);
                Thread.Sleep(2000);
            }
        }

    }
}
