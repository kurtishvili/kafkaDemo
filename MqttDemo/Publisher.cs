// See https://aka.ms/new-console-template for more information
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

Console.WriteLine("Hello, World!");

var mqttFactory = new MqttFactory();
IMqttClient client = mqttFactory.CreateMqttClient();
var options = new MqttClientOptionsBuilder()
    .WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer("localhost", 1883)
    .WithCleanSession()
    .Build();

client.UseConnectedHandler(e =>
{
    Console.WriteLine("Connected to the broker successful");
});

client.UseDisconnectedHandler(e =>
{
    Console.WriteLine("Disconnected to the broker successful");
});
await client.ConnectAsync(options);

Console.WriteLine("Please press a key to publish a message");

Console.ReadLine();

await PublishMessageAsync(client);

await client.DisconnectAsync();
async Task PublishMessageAsync(IMqttClient client)
{
    string messagePayload = "sadsad";
    var message = new MqttApplicationMessageBuilder()
        .WithTopic("Gio")
        .WithPayload(messagePayload)
        .WithAtLeastOnceQoS()
        .Build();

    if (client.IsConnected)
    {
        await client.PublishAsync(message);
    }
}