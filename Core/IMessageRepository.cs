using Core.Entities;

namespace Core
{
    public interface IMessageRepository
    {
        Task<bool> TweetExistsAsync(string id, CancellationToken cancellationToken);
        Task CreateAsync(Tweet tweet, CancellationToken cancellationToken);
        Task AddReplyAsync(Reply reply, CancellationToken cancellationToken);
    }
}
