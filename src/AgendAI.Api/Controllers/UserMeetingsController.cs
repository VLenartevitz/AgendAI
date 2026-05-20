using AgendAI.Api.Contracts;
using AgendAI.Api.Data;
using AgendAI.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Controllers;

[ApiController]
[Route("api/users/me/meetings")]
public sealed class UserMeetingsController(AgendAIDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MeetingResponse>>> GetMyMeetings(CancellationToken cancellationToken)
    {
        var session = await SessionAuth.GetSessionAsync(dbContext, HttpContext, cancellationToken);
        if (session is null || session.PrincipalType != "User" || session.UserId is null)
        {
            return Forbid();
        }

        var meetings = await dbContext.Meetings
            .AsNoTracking()
            .Where(meeting => meeting.CreatedByUserId == session.UserId)
            .OrderBy(meeting => meeting.ScheduledDate)
            .ThenBy(meeting => meeting.StartTime)
            .Select(meeting => new MeetingResponse(
                meeting.Id,
                meeting.EnterpriseId,
                meeting.CreatedByUserId,
                meeting.Title,
                meeting.ClientName,
                meeting.ScheduledDate,
                meeting.StartTime,
                meeting.DurationMinutes,
                meeting.Status,
                meeting.SourceChannel,
                meeting.ClientPhone,
                meeting.ClientEmail,
                meeting.ClientCpf,
                meeting.Notes,
                meeting.CreatedAtUtc,
                meeting.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(meetings);
    }
}

