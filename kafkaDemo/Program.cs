// See https://aka.ms/new-console-template for more information

using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


public partial class Program
{
    static void Main(string[] args)
    {
        //CreateHostBuilder(args).Build().Run();

         Start();
        //End();

    }

    private static void Start()
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = "localhost:9092",
            //EnableIdempotence = true,
            TransactionalId = "pharmacy",
        };

        var producer = new ProducerBuilder<string, string>(config).Build();
        {
            
            producer.InitTransactions(TimeSpan.FromSeconds(10));
            producer.BeginTransaction();

            producer.Produce("demo", new Message<string, string>()
            {
                Key = "1",
                Value = "Test 1234"
            });

            producer.AbortTransaction();

        }
    }

    private static void End()
    {
        var config = new ProducerConfig()
        {
            BootstrapServers = "localhost:9092",
            EnableIdempotence = true,
            TransactionalId = "pharmacy"
        };

        var producer = new ProducerBuilder<string, string>(config).Build();
        {
            producer.Poll(TimeSpan.FromSeconds(10));
            producer.InitTransactions(TimeSpan.FromSeconds(10));
            //producer.BeginTransaction();

            //producer.Produce("demo", new Message<string, string>()
            //{
            //    Key = "1",
            //    Value = "Test 123456"
            //});

            producer.CommitTransaction();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, collection) =>
            {
                //collection.AddHostedService<KafkaProducerHostedService>();
            });
}

public class KafkaProducerHostedService : IHostedService
{
    private readonly ILogger<KafkaProducerHostedService> _logger;
    private readonly IProducer<Null, string> _producer;

    public KafkaProducerHostedService(ILogger<KafkaProducerHostedService> logger)
    {
        _logger = logger;
        var config = new ProducerConfig()
        {
            BootstrapServers = "localhost:9092",
            TransactionalId = "pharmacy"

        };
        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _producer.InitTransactions(TimeSpan.FromSeconds(30));

        _producer.BeginTransaction();

        for (var i = 0; i < 2; i++)
        {
            var value = $"hello world {i}";

            _logger.LogInformation((value));

            await _producer.ProduceAsync("demo", new Message<Null, string>()
            {
                Value = value
            }, cancellationToken);
        }


        //_producer.Flush(TimeSpan.FromSeconds(10));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _producer.Dispose();

        return Task.CompletedTask;
    }
}