using Confluent.Kafka;

namespace Infrastructure.Consumers
{
    public class KafkaTopicsConfig
    {
        public KafkaTopicsConfig(
            string createTweetTopicName,
            string addReplyTopicName,
            string addUserTopicName
        )
        {
            CreateTweetTopicName = createTweetTopicName;
            AddReplyTopicName = addReplyTopicName;
            AddUserTopicName = addUserTopicName;
        }

        public string CreateTweetTopicName { get; }
        public string AddReplyTopicName { get; }
        public string AddUserTopicName { get; }
    }
}
