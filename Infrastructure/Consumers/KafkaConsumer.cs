using Confluent.Kafka;

using Core;
using Core.Commands;

using MongoDB.Driver;

using Serilog;

using System.Text.Json;
using System.Threading;

namespace Infrastructure.Consumers
{
    public class KafkaConsumer : ITweetConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly ITweetEventHandler _tweetEventHandler;

        public KafkaConsumer(
            ConsumerConfig consumerConfig,
            ITweetEventHandler tweetEventHandler)
        {
            _config = consumerConfig;
            _tweetEventHandler = tweetEventHandler;
        }

        public async Task StartConsumerAsync(string[] topicNames, CancellationToken cancellationToken)
        {
            Log.Logger.Information($"Starting consumer for topics");

            using var consumer = new ConsumerBuilder<string, string>(_config)
                    .SetKeyDeserializer(Deserializers.Utf8)
                    .SetValueDeserializer(Deserializers.Utf8)
                    .Build();

            consumer.Subscribe(topicNames);

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cancellationToken);

                if (consumeResult?.Message == null)
                {
                    continue;
                }

                try
                {
                    Log.Logger.Debug(JsonSerializer.Serialize(consumeResult));

                    switch (consumeResult.Topic)
                    {
                        case "CreateTweet":
                            var createTweetCommand = JsonSerializer.Deserialize<CreateTweetCommand>(consumeResult.Message.Value);
                            await _tweetEventHandler.OnAsync<CreateTweetCommand>(createTweetCommand, cancellationToken);
                            break;
                        case "AddReply":
                            var addReplyCommand = JsonSerializer.Deserialize<AddReplyCommand>(consumeResult.Message.Value);
                            await _tweetEventHandler.OnAsync<AddReplyCommand>(addReplyCommand, cancellationToken);
                            break;
                        case "AddUser":
                            var addUserCommand = JsonSerializer.Deserialize<AddUserCommand>(consumeResult.Message.Value);
                            await _tweetEventHandler.OnAsync<AddUserCommand>(addUserCommand, cancellationToken);
                            break;
                        default:
                            throw new ArgumentException("Wrong command type");
                    }
                }

                catch (OperationCanceledException e)
                {
                    Log.Logger.Error(e.Message);
                    break;
                }

                catch (ConsumeException e)
                {
                    Log.Logger.Error(e.Message);
                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e.Message);
                    break;
                }

                consumer.Commit(consumeResult);
            }
        }
    }
}