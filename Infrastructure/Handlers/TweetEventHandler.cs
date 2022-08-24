using Core;
using Core.Commands;
using Core.Entities;
using Infrastructure.Exceptions;

namespace Infrastructure.Handlers
{
    //public class TweetEventHandler : ITweetEventHandler
    //{
    //    private readonly IMessageRepository _messageRepository;

    //    public TweetEventHandler(
    //        IMessageRepository messageRepository)
    //    {
    //        _messageRepository = messageRepository;
    //    }

    //    public Task OnAsync(CreateTweetCommand command, CancellationToken cancellationToken)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Task OnAsync(AddReplyCommand command, CancellationToken cancellationToken)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    //public async Task SendCommandAsync(CreateTweetCommand command, CancellationToken cancellationToken)
    //    //{
    //    //    //await _consumer.ConsumeAsync(_handlerConfig.CreateTweetTopicName, command, cancellationToken);
    //    //}

    //    //public async Task SendCommandAsync(AddReplyCommand command, CancellationToken cancellationToken)
    //    //{
    //    //    var tweetDb = await _messageRepository.GetOneAsync(command.TweetId, cancellationToken);

    //    //    if (tweetDb == null)
    //    //    {
    //    //        throw new TweetNotFoundExeption("Tweet not found");
    //    //    }

    //    //    //await _consumer.ConsumeAsync(_handlerConfig.AddReplyTopicName, command, cancellationToken);
    //    //}

    //    // public async Task SendCommandAsync(UpdateTweetCommand command, CancellationToken cancellationToken)
    //    // {
    //    //     var tweetDb = await _messageRepository.GetOneAsync(command.UserName, command.TweetId, cancellationToken);

    //    //     if (tweetDb == null)
    //    //     {
    //    //         throw new TweetNotFoundExeption("Tweet not found");
    //    //     }

    //    //     tweetDb.Text = command.Text;

    //    //     await _messageRepository.EditAsync(tweetDb, cancellationToken);
    //    // }

    //    // public async Task SendCommandAsync(DeleteTweetCommand command, CancellationToken cancellationToken)
    //    // {
    //    //     var tweetDb = await _messageRepository.GetOneAsync(command.TweetId, cancellationToken);

    //    //     if (tweetDb == null)
    //    //     {
    //    //         throw new TweetNotFoundExeption("Tweet not found");
    //    //     }

    //    //     await _messageRepository.DeleteAsync(command.UserName, command.TweetId, cancellationToken);
    //    // }
    // }

    public class TweetEventHandler : ITweetEventHandler
    {
        private readonly IMessageRepository _messageRepository;

        public TweetEventHandler(
            IMessageRepository messageRepository
            )
        {
            _messageRepository = messageRepository;
        }

        public async Task OnAsync<T>(T command, CancellationToken cancellationToken)
        {

            switch (command)
            {
                case CreateTweetCommand:
                    await OnCreateTweetAsync(command as CreateTweetCommand, cancellationToken);
                    break;
                case AddReplyCommand:
                    await OnAddReplyAsync(command as AddReplyCommand, cancellationToken);
                    break;
                default:
                    throw new ArgumentException("Wrong command type");
            }
        }

        private async Task OnCreateTweetAsync(CreateTweetCommand command, CancellationToken cancellationToken)
        {
            var newTweet = new Tweet
            {
                Text = command.Text,
                UserName = command.UserName,
            };
            await _messageRepository.CreateAsync(newTweet, cancellationToken);
        }

        private Task OnAddReplyAsync(AddReplyCommand command, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
