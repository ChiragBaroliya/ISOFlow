using ISOFlow.Api.Extensions;
using ISOFlow.Application.DTOs;
using ISOFlow.Application.Interfaces;
using ISOFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ISOFlow.Api.Controllers;

/// <summary>
/// System Notifications API
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationsController(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Get paginated and filtered list of Notifications
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<Notification>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResponse<Notification>>>> GetPaged([FromQuery] PagedRequestDto request)
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var paged = await _notificationRepository.GetPagedNotificationsAsync(request, organizationId);
        return Ok(ApiResponse<PagedResponse<Notification>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all system Notifications
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Notification>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Notification>>>> GetAll()
    {
        var organizationId = User.GetOrganizationIdOrNull();
        var list = await _notificationRepository.GetNotificationsAsync(organizationId);
        return Ok(ApiResponse<List<Notification>>.SuccessResponse(list));
    }

    /// <summary>
    /// The caller's notifications, newest first, with the caller's own read state
    /// </summary>
    /// <param name="unreadOnly">Only return notifications the caller hasn't read</param>
    /// <param name="limit">Maximum number to return (1–100, default 20)</param>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(ApiResponse<List<Notification>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Notification>>>> GetMine([FromQuery] bool unreadOnly = false, [FromQuery] int limit = 20)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var list = await _notificationRepository.GetForUserAsync(User.GetOrganizationIdOrNull(), userId, unreadOnly, Math.Clamp(limit, 1, 100));
        return Ok(ApiResponse<List<Notification>>.SuccessResponse(list));
    }

    /// <summary>
    /// Number of notifications the caller hasn't read
    /// </summary>
    [HttpGet("mine/unread-count")]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    public async Task<ActionResult<ApiResponse<int>>> GetMyUnreadCount()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var count = await _notificationRepository.GetUnreadCountAsync(User.GetOrganizationIdOrNull(), userId);
        return Ok(ApiResponse<int>.SuccessResponse(count));
    }

    /// <summary>
    /// Mark one notification as read for the caller
    /// </summary>
    [HttpPost("{id:int}/read")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<ActionResult<ApiResponse<bool>>> MarkRead(int id)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var found = await _notificationRepository.MarkReadAsync(id, User.GetOrganizationIdOrNull(), userId);
        return found
            ? Ok(ApiResponse<bool>.SuccessResponse(true))
            : NotFound(ApiResponse<bool>.FailureResponse($"Notification '{id}' was not found."));
    }

    /// <summary>
    /// Mark all of the caller's notifications as read
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(typeof(ApiResponse<int>), 200)]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllRead()
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var marked = await _notificationRepository.MarkAllReadAsync(User.GetOrganizationIdOrNull(), userId);
        return Ok(ApiResponse<int>.SuccessResponse(marked, $"{marked} notification(s) marked as read."));
    }

    private bool TryGetUserId(out int userId) => int.TryParse(User.GetUserId(), out userId);
}
