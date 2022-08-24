using Core;
using Core.Commands;
using Infrastructure.Consumers;

namespace Synchroniser.BackgroundServices;

public class BackgroundCreateTweetService : BackgroundService
{
    private readonly KafkaTopicsConfig _topicsConfig;
    private readonly IServiceProvider _services;

    public BackgroundCreateTweetService(IServiceProvider services, KafkaTopicsConfig topicsConfig)
    {
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

        await scopedProcessingService.StartConsumerAsync<CreateTweetCommand>(
            _topicsConfig.CreateTweetTopicName,
            stoppingToken
        );
    }
}