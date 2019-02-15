using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;

namespace ConsolePublisher
{
    class Program
    {
        private static IMqttClient _client;
        private static IMqttClientOptions _options;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Publisher....");

            // Create a new MQTT client.
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            //// Create TCP based options using the builder.
            //var options = new MqttClientOptionsBuilder()
            //    .WithClientId(Guid.NewGuid().ToString())
            //    //.WithTcpServer("localhost")
            //    //.WithTcpServer("broker.hivemq.com")
            //    //.WithTcpServer("broker.hivemq.com", 1883) // Port is optional
            //    .WithTcpServer("localhost", 1884)
            //    .WithCredentials("bud", "%spencer%")
            //    //.WithTls() // Use secure TCP connection
            //    .WithCleanSession()
            //    .Build();

            _options = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer("localhost", 1884)
                .WithCredentials("bud", "%spencer%")
                .WithCleanSession()
                .Build();


            #region EventBinding
            _client.Connected += Client_Connected;
            _client.ApplicationMessageReceived += Message_Recieved;
            _client.Disconnected += Client_ConnectionClosed;
            #endregion


            _client.ConnectAsync(_options).Wait();

            Console.ReadLine();
            SimulatePublish();
            
            Console.WriteLine("Simulation ended! press any key to exit.");
            _client.DisconnectAsync().Wait();
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
                    //.WithExactlyOnceQoS()
                    .WithRetainFlag()
                    .Build();

              
                if (_client.IsConnected)
                {
                    Console.WriteLine($"publishing at {DateTime.UtcNow}");
                    _client.PublishAsync(testMessage);
                    //counter++;
                }
               
                Thread.Sleep(2000);
            }
        }
        private static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes =Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }


        //Event-Handling
        private static void Message_Recieved(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine($"ApplicationMessageReceived at {DateTime.UtcNow}");
            //Console.WriteLine(Encoding.UTF8.GetString(e.ApplicationMessage.Payload)); //works
            Console.WriteLine(Base64Decode(e.ApplicationMessage.Payload.ToString())); //also works
        }
        private static void Client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("### CONNECTED WITH SERVER ###");
            Console.WriteLine("Press any key to simulate message sending!");
            //await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("test").Build());
        }
        private static async void Client_ConnectionClosed(object sender, EventArgs e)
        {
            Console.WriteLine($"Client_ConnectionClosed:  {e}" );
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await _client.ConnectAsync(_options);
            }
            catch
            {
                Console.WriteLine("### RECONNECTING FAILED ###");
            }
        }

    }
}
