using Confluent.Kafka;
using Core;
using Infrastructure.Consumers;
using Infrastructure.Handlers;
using Infrastructure.Repositories;
using Synchroniser.BackgroundServices;
using Serilog;
using Serilog.Sinks.Elasticsearch;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, logConfiguration) =>
    {
        logConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(Environment.GetEnvironmentVariable("ELASTICSEARCH_URI")
                ?? context.Configuration["ElasticConfiguration:Uri"]))
            {
                IndexFormat = $"tweeter-synchroniser-logs",
                AutoRegisterTemplate = true,
                NumberOfShards = 2,
                NumberOfReplicas = 1
            }).Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName).ReadFrom.Configuration(context.Configuration);
    });

    Log.Information("Starting up");

    // Add services to the container.

    builder.Services.AddSingleton<IMessageRepository>(sp => new MessageRepository(
        Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:ConnectionString"),
        Environment.GetEnvironmentVariable("DB_NAME")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbName"),
        Environment.GetEnvironmentVariable("DB_TWEET_COLLECTION")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbTweetCollectionName"),
        Environment.GetEnvironmentVariable("DB_REPLY_COLLECTION")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbReplyCollectionName"),
        Environment.GetEnvironmentVariable("DB_USER_COLLECTION")
            ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbUserCollectionName")
    ));

    builder.Services.AddSingleton<IHostedService, BackgroundKafkaConsumerService>();

    builder.Services.AddSingleton(new KafkaTopicsConfig(
        Environment.GetEnvironmentVariable("KAFKA_CREATE_TWEET_TOPIC_NAME")
            ?? builder.Configuration.GetValue<string>("KafkaSettings:CreateTweetTopicName"),
        Environment.GetEnvironmentVariable("KAFKA_ADD_REPLY_TOPIC_NAME")
            ?? builder.Configuration.GetValue<string>("KafkaSettings:AddReplyTopicName"),
        Environment.GetEnvironmentVariable("KAFKA_ADD_USER_TOPIC_NAME")
            ?? builder.Configuration.GetValue<string>("KafkaSettings:AddUserTopicName")
    ));

    bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    var kafkaConsumerConfig = isDevelopment
        ? new ConsumerConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_SERVER")
                ?? builder.Configuration.GetValue<string>("KafkaSettings:BootstrapServers"),
        }
        : new ConsumerConfig
        {
            BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_SERVER")
                ?? builder.Configuration.GetValue<string>("KafkaSettings:BootstrapServers"),
            SaslUsername = builder.Configuration.GetValue<string>("KafkaSettings:Key"),
            SaslPassword = builder.Configuration.GetValue<string>("KafkaSettings:Secret"),
            SaslMechanism = SaslMechanism.Plain,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

    kafkaConsumerConfig.EnableAutoCommit = false;
    kafkaConsumerConfig.GroupId = "tweeter";

    builder.Services.AddSingleton(kafkaConsumerConfig);

    builder.Services.AddScoped<ITweetEventHandler, TweetEventHandler>();
    builder.Services.AddScoped<ITweetConsumer, KafkaConsumer>();

    var app = builder.Build();

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException") // https://github.com/dotnet/runtime/issues/60600
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
