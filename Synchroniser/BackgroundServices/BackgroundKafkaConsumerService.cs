using Core;
using Core.Commands;

using Infrastructure.Consumers;

using Serilog;

namespace Synchroniser.BackgroundServices;

public class BackgroundKafkaConsumerService : BackgroundService
{
    private readonly KafkaTopicsConfig _topicsConfig;
    private readonly IServiceProvider _services;

    public BackgroundKafkaConsumerService(IServiceProvider services, KafkaTopicsConfig topicsConfig)
    {
        Log.Logger.Information("Create kafka consumer service");
        _services = services;
        _topicsConfig = topicsConfig;
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        using var scope = _services.CreateScope();

        var scopedProcessingService = scope.ServiceProvider.GetRequiredService<ITweetConsumer>();

        await scopedProcessingService.StartConsumerAsync(
            new[] { _topicsConfig.CreateTweetTopicName, _topicsConfig.AddReplyTopicName, _topicsConfig.AddUserTopicName },
            stoppingToken
        );
    }
}