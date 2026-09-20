using TexasMediaDart.Application.Users.Models;

namespace TexasMediaDart.Application.Users.Abstractions;

public interface IIdentityUsersClient
{
    Task<IReadOnlyList<IdentityUserDto>> LookupAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default);
}