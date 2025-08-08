using MQTTnet;
using System.Text;

namespace Mosquitto
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var mqttClientFactory = new MqttClientFactory();
            var mqttClient = mqttClientFactory.CreateMqttClient();

            // Konfigurerar anslutningsalternativ
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("test.mosquitto.org", 1883) // 1883 är standardporten för oskyddad MQTT
                .WithCleanSession()
                .Build();

            // Hanterar meddelanden som mottas
            mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                if(message == "turn lamp on")
                {
                    Console.WriteLine("Nu lyser lampan");
                }
                if (message == "turn lamp off")
                {
                    Console.WriteLine("Nu är lampan släckt");
                }


                return Task.CompletedTask;
            };

            // Hanterar vad som ska ske vid anslutning
            mqttClient.ConnectedAsync += async e =>
            {
                Console.WriteLine("Ansluten till MQTT-broker.");

                // Prenumerera på ett ämne
                var topicFilter = new MqttTopicFilterBuilder()
                    .WithTopic("stefan/iot/lampan12345")
                    .Build();

                await mqttClient.SubscribeAsync(topicFilter);

            };

            // Hanterar vad som ska ske vid frånkoppling
            mqttClient.DisconnectedAsync += e =>
            {
                Console.WriteLine("Frånkopplad från MQTT-mäklaren.");
                return Task.CompletedTask;
            };

            // Anslut till mäklaren
            Console.WriteLine("Ansluter...");
            await mqttClient.ConnectAsync(options, CancellationToken.None);

            // Vänta på att meddelanden ska mottas och programmet inte ska avslutas direkt
            Console.WriteLine("Tryck på valfri tangent för att avsluta.");
            Console.ReadKey();

            // Frånkoppla
            await mqttClient.DisconnectAsync();
        }
    }
}
