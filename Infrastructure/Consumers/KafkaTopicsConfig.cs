using Confluent.Kafka;

namespace Infrastructure.Consumers
{
    public class KafkaTopicsConfig
    {
        public KafkaTopicsConfig(
            string createTweetTopicName,
            string addReplyTopicName
        )
        {
            CreateTweetTopicName = createTweetTopicName;
            AddReplyTopicName = addReplyTopicName;
        }

        public string CreateTweetTopicName { get; }
        public string AddReplyTopicName { get; }
    }
}
