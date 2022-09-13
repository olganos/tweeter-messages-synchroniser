using Confluent.Kafka;
using Core;
using Infrastructure.Consumers;
using Infrastructure.Handlers;
using Infrastructure.Repositories;
using Synchroniser.BackgroundServices;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

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

    builder.Services.AddSingleton(new ConsumerConfig
    {
        BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_SERVER")
            ?? builder.Configuration.GetValue<string>("KafkaSettings:BootstrapServers"),
        EnableAutoCommit = false,
        GroupId = "tweeter"
    });

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
