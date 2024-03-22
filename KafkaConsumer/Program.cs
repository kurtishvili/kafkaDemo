

using Confluent.Kafka;

var configC = new ConsumerConfig()
{
    BootstrapServers = "localhost:9092",
    GroupId = "tab",
    IsolationLevel = IsolationLevel.ReadCommitted
};

var consumer = new ConsumerBuilder<string, string>(configC).Build();

consumer.Subscribe("rs_pharmacy");

while (true)
{
    var result = consumer.Consume();

    Console.WriteLine(result.Message.Value);
}