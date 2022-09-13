using Core;
using Core.Commands;
using Core.Entities;

using Infrastructure.Exceptions;

namespace Infrastructure.Handlers
{
    public class TweetEventHandler : ITweetEventHandler
    {
        private readonly IMessageRepository _messageRepository;

        public TweetEventHandler(IMessageRepository messageRepository)
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
                case AddUserCommand:
                    await OnAddUserAsync(command as AddUserCommand, cancellationToken);
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
                Created = command.Created,
            };
            await _messageRepository.CreateAsync(newTweet, cancellationToken);
        }

        private async Task OnAddReplyAsync(AddReplyCommand command, CancellationToken cancellationToken)
        {
            var tweetExists = await _messageRepository.TweetExistsAsync(command.TweetId, cancellationToken);

            if (!tweetExists)
            {
                return;
            }

            var newReply = new Reply
            {
                Text = command.Text,
                TweetId = command.TweetId,
                UserName = command.UserName,
                Created = command.Created,
            };

            await _messageRepository.AddReplyAsync(newReply, cancellationToken);
        }

        private async Task OnAddUserAsync(AddUserCommand command, CancellationToken cancellationToken)
        {
            var userExists = await _messageRepository.UserExistsAsync(command.UserName, cancellationToken);

            // To avoid dupplications
            if (userExists)
            {
                return;
            }

            var newUser = new User
            {
                UserName = command.UserName,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            await _messageRepository.AddUserAsync(newUser, cancellationToken);
        }
    }
}
