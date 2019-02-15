using System;
using System.Text;
using System.Threading;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;

namespace ConsoleSubscriber
{
    class Program
    {
        private static MqttFactory _factory;
        private static IMqttClient client;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Subsriber....");

            var device1 = new MobileDevice();
            device1.OnConnected += (s, e) =>
            {
                Console.WriteLine($"ConnectedHandler called in Main for Device: {e}");
            };
            device1.OnDisconnected += (s, e) =>
            {
                Console.WriteLine($"DisconnectedHandler called in Main for Device: {e}");
            };

            Console.WriteLine($"Press any key to connec the device: {device1.Id}");
            Console.ReadLine();
            device1.ConnectAsync().Wait();

            Console.WriteLine("Subscriber Connected. Press any key to disconnect");
            Console.ReadLine();

            device1.DisconnectAsync().Wait();


            //MainMethodCode();

            Console.ReadLine();
        }





        #region previousMain
        private static void MainMethodCode()
        {
            // Create a new MQTT client.
            _factory = new MqttFactory();
            client = _factory.CreateMqttClient();


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


            //Event Binding
            client.Connected += Client_Connected;
            client.ApplicationMessageReceived += Message_Recieved;


            client.ConnectAsync(options).Wait();


        }


        private static void Message_Recieved(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
        }


        private static async void Client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("### CONNECTED WITH SERVER ###");

            //Subscribe to topic
            await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("test").Build());
            //await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("my/topic").Build());
            Console.WriteLine("### SUBSCRIBED ###");
        }
        #endregion





    }
}
