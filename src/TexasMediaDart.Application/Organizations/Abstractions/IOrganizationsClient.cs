using TexasMediaDart.Application.Organizations.Models;

namespace TexasMediaDart.Application.Organizations.Abstractions;

public interface IOrganizationsClient
{
    Task<CurrentOrganizationDto> GetCurrentAsync(
        CancellationToken cancellationToken = default);
}