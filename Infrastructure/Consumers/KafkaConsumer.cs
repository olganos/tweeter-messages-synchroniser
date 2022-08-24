using Confluent.Kafka;
using Core;
using Core.Commands;
using System.Text.Json;

namespace Infrastructure.Consumers
{
    public class KafkaConsumer : ITweetConsumer
    {
        private readonly ConsumerConfig _config;
        private readonly ITweetEventHandler _tweetEventHandler;

        public KafkaConsumer(ConsumerConfig consumerConfig, ITweetEventHandler tweetEventHandler)
        {
            _config = consumerConfig;
            _tweetEventHandler = tweetEventHandler;
        }

        public async Task StartConsumerAsync<T>(string topicName, CancellationToken cancellationToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_config)
                    .SetKeyDeserializer(Deserializers.Utf8)
                    .SetValueDeserializer(Deserializers.Utf8)
                    .Build();

            consumer.Subscribe(topicName);

            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = consumer.Consume(cancellationToken);

                if (consumeResult?.Message == null)
                {
                    continue;
                }

                try
                {
                    var command = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);

                    if (command == null)
                    {
                        continue;
                    }

                    await _tweetEventHandler.OnAsync<T>(command, cancellationToken);
                }

                catch (OperationCanceledException)
                {
                    // todo: maybe add log
                    break;
                }

                catch (ConsumeException e)
                {
                    // todo: maybe add log
                    if (e.Error.IsFatal)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    // todo: maybe add log
                    break;
                }

                consumer.Commit(consumeResult);
            }
        }
    }
}