using System;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;


namespace ConsoleSubscriber
{
    public class MobileDevice
    {
        private readonly IMqttClient _client;
        private readonly IMqttClientOptions _options;

        //These are subscribed by outside world.
        public EventHandler<string> OnConnected;
        public EventHandler<string> OnDisconnected;

        public string Id => $"Mobile_{Guid.NewGuid():N}";

        //ctor
        public MobileDevice()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();

          _options =  new MqttClientOptionsBuilder()
                .WithClientId(Id)
                //.WithTcpServer("broker.hivemq.com")
                .WithTcpServer("localhost", 1884)
                .WithCredentials("bud", "%spencer%")
                .WithCleanSession()
                .Build();
        }

        public async Task ConnectAsync()
        {
            Console.WriteLine($"Connecting......{Id}");

           

            //Event Binding
            _client.Connected += Client_Connected;
            _client.ApplicationMessageReceived += Message_Recieved;
            _client.Disconnected += Client_ConnectionClosed;

            //Connect to server
            await _client.ConnectAsync(_options);
        }

        public async Task DisconnectAsync()
        {
            Console.WriteLine($"Disconnecting......{Id}");
            await _client.DisconnectAsync();
        }



        private void Message_Recieved(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
        }


        private async void Client_Connected(object sender, EventArgs e)
        {
            Console.WriteLine("### CONNECTED WITH SERVER ###");

            //Subscribe to topic
            await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic("test").Build());
            //await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("my/topic").Build());
            Console.WriteLine("### SUBSCRIBED ###");

            OnConnected?.Invoke(this, Id);//internal event to notify other client of this mobile who are interested
        }



        private async void Client_ConnectionClosed(object sender, EventArgs e)
        {
            Console.WriteLine("### DISCONNECTED FROM SERVER ###");
            OnDisconnected?.Invoke(this, Id);

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
