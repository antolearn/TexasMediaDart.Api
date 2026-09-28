namespace TexasMediaDart.Application.Organizations.Models;

public sealed class CurrentOrganizationDto
{
    public Guid OrganizationId { get; init; }

    public bool IsActive { get; init; }
}