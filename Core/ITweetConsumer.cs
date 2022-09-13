namespace Core
{
    public interface ITweetConsumer
    {
        Task StartConsumerAsync(string[] topicNames, CancellationToken cancellationToken);
    }
}