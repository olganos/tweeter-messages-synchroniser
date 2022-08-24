using Core;
using Core.Commands;
using Core.Entities;
using Infrastructure.Exceptions;

namespace Infrastructure.Handlers
{
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

        private async Task OnAddReplyAsync(AddReplyCommand command, CancellationToken cancellationToken)
        {
            var tweetDb = await _messageRepository.GetOneAsync(command.TweetId, cancellationToken);

            if (tweetDb == null)
            {
                return;
            }

            tweetDb.Text = command.Text;
            if (tweetDb.Replies == null)
            {
                tweetDb.Replies = new List<Reply>();
            }

            tweetDb.Replies.Add(
                new Reply
                {
                    Text = command.Text,
                    UserName = command.UserName,
                });

            await _messageRepository.EditAsync(tweetDb, cancellationToken);
        }
    }
}
