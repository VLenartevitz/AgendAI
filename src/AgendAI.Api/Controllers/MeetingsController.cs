using AgendAI.Api.Contracts;
using AgendAI.Api.Data;
using AgendAI.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/meetings")]
public sealed class MeetingsController(AgendAIDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MeetingResponse>>> GetMeetings(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId, cancellationToken);

        if (!userExists)
        {
            return NotFound("Usuario nao encontrado.");
        }

        var meetings = await dbContext.Meetings
            .AsNoTracking()
            .Where(meeting => meeting.UserId == userId)
            .OrderBy(meeting => meeting.ScheduledDate)
            .ThenBy(meeting => meeting.StartTime)
            .Select(meeting => MapMeeting(meeting))
            .ToListAsync(cancellationToken);

        return Ok(meetings);
    }

    [HttpPost]
    public async Task<ActionResult<MeetingResponse>> CreateMeeting(
        Guid userId,
        [FromBody] CreateMeetingRequest request,
        CancellationToken cancellationToken)
    {
        var userExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(user => user.Id == userId, cancellationToken);

        if (!userExists)
        {
            return NotFound("Usuario nao encontrado.");
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ClientName))
        {
            return ValidationProblem("Titulo e cliente sao obrigatorios.");
        }

        var meeting = new Meeting
        {
            UserId = userId,
            Title = request.Title.Trim(),
            ClientName = request.ClientName.Trim(),
            ScheduledDate = request.ScheduledDate,
            StartTime = request.StartTime,
            DurationMinutes = Math.Max(15, request.DurationMinutes),
            ClientPhone = request.ClientPhone?.Trim(),
            Notes = request.Notes?.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Confirmada" : request.Status.Trim(),
            SourceChannel = string.IsNullOrWhiteSpace(request.SourceChannel) ? "Dashboard" : request.SourceChannel.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.Meetings.Add(meeting);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetMeetings), new { userId }, MapMeeting(meeting));
    }

    [HttpDelete("{meetingId:guid}")]
    public async Task<IActionResult> DeleteMeeting(Guid userId, Guid meetingId, CancellationToken cancellationToken)
    {
        var meeting = await dbContext.Meetings
            .SingleOrDefaultAsync(currentMeeting => currentMeeting.Id == meetingId && currentMeeting.UserId == userId, cancellationToken);

        if (meeting is null)
        {
            return NotFound("Reuniao nao encontrada.");
        }

        dbContext.Meetings.Remove(meeting);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static MeetingResponse MapMeeting(Meeting meeting)
        => new(
            meeting.Id,
            meeting.UserId,
            meeting.Title,
            meeting.ClientName,
            meeting.ScheduledDate,
            meeting.StartTime,
            meeting.DurationMinutes,
            meeting.Status,
            meeting.SourceChannel,
            meeting.ClientPhone,
            meeting.Notes,
            meeting.CreatedAtUtc,
            meeting.UpdatedAtUtc);
}
