using MQTTnet.Client;
using MQTTnet;
using System.Text;

var mqttFactory = new MqttFactory();

using (var mqttClient = mqttFactory.CreateMqttClient())
{
    var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithCredentials("admin", "qwe123")
        .WithTcpServer("52.166.0.19", 1883)
        .Build();

    // Setup message handling before connecting so that queued messages
    // are also handled properly. When there is no event handler attached all
    // received messages get lost.

    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

    mqttClient.ApplicationMessageReceivedAsync += e =>
    {
        Console.WriteLine($"Received application message. {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");


        return Task.CompletedTask;
    };

    var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
        .WithTopicFilter(
            f =>
            {
                f.WithTopic("18-47");
            })
        .Build();
  

    await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

    Console.WriteLine("MQTT client subscribed to topic.");

    Console.WriteLine("Press enter to exit.");

    Console.ReadLine();
}