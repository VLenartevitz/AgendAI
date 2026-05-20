using AgendAI.Api.Contracts;
using AgendAI.Api.Data;
using AgendAI.Api.Infrastructure;
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

        var session = await SessionAuth.GetSessionAsync(dbContext, Request, cancellationToken);
        if (session is null || session.PrincipalType != "Enterprise" || session.EnterpriseId != enterprise.Id)
        {
            return Forbid();
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

    [HttpGet("public")]
    public async Task<ActionResult<IReadOnlyList<MeetingResponse>>> GetPublicMeetings(
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
            .Select(meeting => new MeetingResponse(
                meeting.Id,
                meeting.EnterpriseId,
                meeting.CreatedByUserId,
                "Ocupado",
                "Reservado",
                meeting.ScheduledDate,
                meeting.StartTime,
                meeting.DurationMinutes,
                meeting.Status,
                meeting.SourceChannel,
                null,
                null,
                null,
                null,
                meeting.CreatedAtUtc,
                meeting.UpdatedAtUtc))
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

        Guid? createdByUserId = null;
        string status;
        string sourceChannel;

        var session = await SessionAuth.GetSessionAsync(dbContext, Request, cancellationToken);
        if (session is not null && session.PrincipalType == "Enterprise" && session.EnterpriseId == enterprise.Id)
        {
            status = "Confirmada";
            sourceChannel = string.IsNullOrWhiteSpace(request.SourceChannel) ? "Dashboard" : request.SourceChannel.Trim();
        }
        else if (session is not null && session.PrincipalType == "User")
        {
            createdByUserId = session.UserId;
            status = "Pendente";
            sourceChannel = string.IsNullOrWhiteSpace(request.SourceChannel) ? "PublicPage" : request.SourceChannel.Trim();
        }
        else
        {
            status = "Pendente";
            sourceChannel = string.IsNullOrWhiteSpace(request.SourceChannel) ? "PublicPage" : request.SourceChannel.Trim();
        }

        if (createdByUserId is not null)
        {
            var userExists = await dbContext.Users
                .AsNoTracking()
                .AnyAsync(user => user.Id == createdByUserId, cancellationToken);

            if (!userExists)
            {
                return ValidationProblem("Usuario criador nao encontrado.");
            }
        }

        var duration = Math.Max(15, request.DurationMinutes);
        var requestedStart = request.StartTime.ToTimeSpan();
        var requestedEnd = requestedStart.Add(TimeSpan.FromMinutes(duration));

        var conflicts = await dbContext.Meetings
            .AsNoTracking()
            .Where(m => m.EnterpriseId == enterprise.Id && m.ScheduledDate == request.ScheduledDate)
            .AnyAsync(m =>
                requestedStart < m.StartTime.ToTimeSpan().Add(TimeSpan.FromMinutes(m.DurationMinutes)) &&
                requestedEnd > m.StartTime.ToTimeSpan(),
                cancellationToken);

        if (conflicts)
        {
            return Conflict("Ja existe uma reuniao nesse horario.");
        }

        var meeting = new Meeting
        {
            EnterpriseId = enterprise.Id,
            CreatedByUserId = createdByUserId,
            Title = request.Title.Trim(),
            ClientName = request.ClientName.Trim(),
            ScheduledDate = request.ScheduledDate,
            StartTime = request.StartTime,
            DurationMinutes = duration,
            ClientPhone = request.ClientPhone?.Trim(),
            ClientEmail = request.ClientEmail?.Trim(),
            ClientCpf = request.ClientCpf?.Trim(),
            Notes = request.Notes?.Trim(),
            Status = status,
            SourceChannel = sourceChannel,
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

        var session = await SessionAuth.GetSessionAsync(dbContext, Request, cancellationToken);
        if (session is null || session.PrincipalType != "Enterprise" || session.EnterpriseId != enterprise.Id)
        {
            return Forbid();
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

    [HttpPatch("{meetingId:guid}/status")]
    public async Task<ActionResult<MeetingResponse>> UpdateStatus(
        string enterpriseSlug,
        Guid meetingId,
        [FromBody] UpdateMeetingStatusRequest request,
        CancellationToken cancellationToken)
    {
        var enterprise = await dbContext.Enterprises
            .AsNoTracking()
            .SingleOrDefaultAsync(currentEnterprise => currentEnterprise.Slug == NormalizeSlug(enterpriseSlug), cancellationToken);

        if (enterprise is null)
        {
            return NotFound("Enterprise nao encontrada.");
        }

        var session = await SessionAuth.GetSessionAsync(dbContext, Request, cancellationToken);
        if (session is null || session.PrincipalType != "Enterprise" || session.EnterpriseId != enterprise.Id)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return ValidationProblem("Status e obrigatorio.");
        }

        var meeting = await dbContext.Meetings
            .SingleOrDefaultAsync(currentMeeting => currentMeeting.Id == meetingId && currentMeeting.EnterpriseId == enterprise.Id, cancellationToken);

        if (meeting is null)
        {
            return NotFound("Reuniao nao encontrada.");
        }

        meeting.Status = request.Status.Trim();
        meeting.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapMeeting(meeting));
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
            meeting.ClientEmail,
            meeting.ClientCpf,
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
