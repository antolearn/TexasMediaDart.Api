using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TexasMediaDart.Api.Models.Users;
using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.Users.Commands.CreateUser;
using TexasMediaDart.Application.Users.Models;
using TexasMediaDart.Application.Users.Queries.SearchUsers;

namespace TexasMediaDart.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly SearchUsersQueryHandler _searchUsersHandler;
    private readonly CreateUserCommandHandler _createUserHandler;

    public UsersController(
        SearchUsersQueryHandler searchUsersHandler,
        CreateUserCommandHandler createUserHandler)
    {
        _searchUsersHandler = searchUsersHandler;
        _createUserHandler = createUserHandler;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(UserSearchResultDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? identityUserId = null,
        [FromQuery] string? email = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isApproved = null,
        [FromQuery] string sortBy = "createdUtc",
        [FromQuery] string sortDirection = "desc",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new SearchUsersQuery(
                identityUserId,
                email,
                isActive,
                isApproved,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize);

            var result =
                await _searchUsersHandler.HandleAsync(
                    query,
                    cancellationToken);

            return Ok(result);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(UserDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(
                    "Email is required.");
            }

            var command =
                new CreateUserCommand(
                    request.Email.Trim());

            var result =
                await _createUserHandler.HandleAsync(
                    command,
                    cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                result);
        }
        catch (BadRequestException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ConflictException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }
}