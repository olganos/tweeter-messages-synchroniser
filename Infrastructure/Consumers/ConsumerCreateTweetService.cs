using Core;
using Core.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Consumers;

public class ConsumerCreateTweetService : BackgroundService
{
    private readonly KafkaTopicsConfig _topicsConfig;

    public ConsumerCreateTweetService(IServiceProvider services, KafkaTopicsConfig topicsConfig)
    {
        Services = services;
        _topicsConfig = topicsConfig;
    }

    public IServiceProvider Services { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DoWork(stoppingToken);
    }

    private async Task DoWork(CancellationToken stoppingToken)
    {
        using var scope = Services.CreateScope();

        var scopedProcessingService = scope.ServiceProvider.GetRequiredService<ITweetConsumer>();

        await scopedProcessingService.StartConsumerAsync<CreateTweetCommand>(
            _topicsConfig.CreateTweetTopicName,
            stoppingToken
        );
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        await base.StopAsync(stoppingToken);
    }
}