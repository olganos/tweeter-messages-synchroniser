namespace Core
{
    public interface ITweetConsumer
    {
        Task StartConsumerAsync<T>(string topicName, CancellationToken cancellationToken);
    }
}