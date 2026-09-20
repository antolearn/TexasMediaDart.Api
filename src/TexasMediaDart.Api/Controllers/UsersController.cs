using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TexasMediaDart.Application.Users.Models;
using TexasMediaDart.Application.Users.Queries.SearchUsers;

namespace TexasMediaDart.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly SearchUsersQueryHandler _searchUsersHandler;

    public UsersController(
        SearchUsersQueryHandler searchUsersHandler)
    {
        _searchUsersHandler = searchUsersHandler;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(UserSearchResultDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? identityUserId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isApproved = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchUsersQuery(
            identityUserId,
            isActive,
            isApproved,
            pageNumber,
            pageSize);

        var result =
            await _searchUsersHandler.HandleAsync(
                query,
                cancellationToken);

        return Ok(result);
    }
}