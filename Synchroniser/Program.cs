using Confluent.Kafka;
using Core;
using Infrastructure.Consumers;
using Infrastructure.Handlers;
using Infrastructure.Repositories;
using Synchroniser.BackgroundServices;

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
        ?? builder.Configuration.GetValue<string>("MongoDbSettings:DbReplyCollectionName")
));

builder.Services.AddHostedService<BackgroundCreateTweetService>();
builder.Services.AddHostedService<BackgroundAddReplyService>();

builder.Services.AddSingleton(new KafkaTopicsConfig(
    Environment.GetEnvironmentVariable("KAFKA_CREATE_TWEET_TOPIC_NAME")
        ?? builder.Configuration.GetValue<string>("KafkaSettings:CreateTweetTopicName"),
    Environment.GetEnvironmentVariable("KAFKA_ADD_REPLY_TOPIC_NAME")
        ?? builder.Configuration.GetValue<string>("KafkaSettings:AddRealyTopicName")
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
