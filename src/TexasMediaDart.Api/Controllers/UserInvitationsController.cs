using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TexasMediaDart.Api.Models.UserInvitations;
using TexasMediaDart.Application.Common.Exceptions;
using TexasMediaDart.Application.UserInvitations.Commands.AcceptUserInvitation;

namespace TexasMediaDart.Api.Controllers;

[ApiController]
[Route("api/user-invitations")]
public sealed class UserInvitationsController : ControllerBase
{
    private readonly AcceptUserInvitationCommandHandler
        _acceptUserInvitationHandler;

    public UserInvitationsController(
        AcceptUserInvitationCommandHandler acceptUserInvitationHandler)
    {
        _acceptUserInvitationHandler =
            acceptUserInvitationHandler;
    }

    [HttpPost("accept")]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(AcceptUserInvitationResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    public async Task<IActionResult> Accept(
        [FromBody] AcceptUserInvitationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var command =
                new AcceptUserInvitationCommand(
                    request.Token,
                    request.Password,
                    request.ConfirmPassword);

            var result =
                await _acceptUserInvitationHandler.HandleAsync(
                    command,
                    cancellationToken);

            return Ok(result);
        }
        catch (BadRequestException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad Request",
                detail: ex.Message);
        }
        catch (NotFoundException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: ex.Message);
        }
        catch (ConflictException ex)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: ex.Message);
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }
}