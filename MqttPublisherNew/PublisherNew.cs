
using MQTTnet.Client;
using MQTTnet;

var mqttFactory = new MqttFactory();

using (var mqttClient = mqttFactory.CreateMqttClient())
{
    var mqttClientOptions = new MqttClientOptionsBuilder()
        .WithCredentials("clue", "1")
        .WithTcpServer("localhost", 1883)
        .Build();

    await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

    var applicationMessage = new MqttApplicationMessageBuilder()
        .WithTopic("test")
        .WithPayload("19.5")
        .Build();

    await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

    await mqttClient.DisconnectAsync();

    Console.WriteLine("MQTT application message is published.");
    Console.ReadLine();
}