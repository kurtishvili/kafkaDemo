// See https://aka.ms/new-console-template for more information

using NATS.Client;
static async Task StartNatsConsumer()
{
    var options = ConnectionFactory.GetDefaultOptions();
    //options.Url = "nats://52.166.0.19:4222";
    options.Url = "nats://localhost:4222";
    options.NoEcho = true;
    options.NoRandomize = false;

    var natsConnection = new ConnectionFactory().CreateConnection(options);

    using var subscription = natsConnection.SubscribeSync("pharmacy_admin_command", "USER_CHAT_1");
    {
        while (true)
        {
            var msg = subscription.NextMessage();

            var message = System.Text.Encoding.UTF8.GetString(msg.Data);

            Console.WriteLine(message);
        };
    }
}

Task.Run(() => { StartNatsConsumer(); });

Console.ReadKey();