using Core.Commands;

namespace Core
{
    public interface ITweetEventHandler
    {
        Task OnAsync<T>(T command, CancellationToken cancellationToken);
    }
}
