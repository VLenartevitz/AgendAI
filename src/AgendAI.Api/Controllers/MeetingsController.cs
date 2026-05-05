using AgendAI.Api.Contracts;
using AgendAI.Api.Data;
using AgendAI.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgendAI.Api.Controllers;

[ApiController]
[Route("api/enterprises/{enterpriseSlug}/meetings")]
public sealed class MeetingsController(AgendAIDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MeetingResponse>>> GetMeetings(
        string enterpriseSlug,
        CancellationToken cancellationToken)
    {
        var enterprise = await dbContext.Enterprises
            .AsNoTracking()
            .SingleOrDefaultAsync(currentEnterprise => currentEnterprise.Slug == NormalizeSlug(enterpriseSlug), cancellationToken);

        if (enterprise is null)
        {
            return NotFound("Enterprise nao encontrada.");
        }

        var meetings = await dbContext.Meetings
            .AsNoTracking()
            .Where(meeting => meeting.EnterpriseId == enterprise.Id)
            .OrderBy(meeting => meeting.ScheduledDate)
            .ThenBy(meeting => meeting.StartTime)
            .Select(meeting => MapMeeting(meeting))
            .ToListAsync(cancellationToken);

        return Ok(meetings);
    }

    [HttpPost]
    public async Task<ActionResult<MeetingResponse>> CreateMeeting(
        string enterpriseSlug,
        [FromBody] CreateMeetingRequest request,
        CancellationToken cancellationToken)
    {
        var enterprise = await dbContext.Enterprises
            .SingleOrDefaultAsync(currentEnterprise => currentEnterprise.Slug == NormalizeSlug(enterpriseSlug), cancellationToken);

        if (enterprise is null)
        {
            return NotFound("Enterprise nao encontrada.");
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ClientName))
        {
            return ValidationProblem("Titulo e cliente sao obrigatorios.");
        }

        if (request.CreatedByUserId is not null)
        {
            var userExists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(user => user.Id == request.CreatedByUserId, cancellationToken);

            if (!userExists)
            {
                return ValidationProblem("Usuario criador nao encontrado.");
            }
        }

        var meeting = new Meeting
        {
            EnterpriseId = enterprise.Id,
            CreatedByUserId = request.CreatedByUserId,
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

        return CreatedAtAction(nameof(GetMeetings), new { enterpriseSlug = enterprise.Slug }, MapMeeting(meeting));
    }

    [HttpDelete("{meetingId:guid}")]
    public async Task<IActionResult> DeleteMeeting(string enterpriseSlug, Guid meetingId, CancellationToken cancellationToken)
    {
        var enterprise = await dbContext.Enterprises
            .AsNoTracking()
            .SingleOrDefaultAsync(currentEnterprise => currentEnterprise.Slug == NormalizeSlug(enterpriseSlug), cancellationToken);

        if (enterprise is null)
        {
            return NotFound("Enterprise nao encontrada.");
        }

        var meeting = await dbContext.Meetings
            .SingleOrDefaultAsync(currentMeeting => currentMeeting.Id == meetingId && currentMeeting.EnterpriseId == enterprise.Id, cancellationToken);

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
            meeting.Notes,
            meeting.CreatedAtUtc,
            meeting.UpdatedAtUtc);

    private static string NormalizeSlug(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var chars = normalized
            .Select(currentChar => char.IsLetterOrDigit(currentChar) ? currentChar : '-')
            .ToArray();

        return string.Join('-', new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
    }
}
