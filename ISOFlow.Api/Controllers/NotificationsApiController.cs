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
        var paged = await _notificationRepository.GetPagedNotificationsAsync(request);
        return Ok(ApiResponse<PagedResponse<Notification>>.SuccessResponse(paged));
    }

    /// <summary>
    /// Get all system Notifications
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(ApiResponse<List<Notification>>), 200)]
    public async Task<ActionResult<ApiResponse<List<Notification>>>> GetAll()
    {
        var list = await _notificationRepository.GetNotificationsAsync();
        return Ok(ApiResponse<List<Notification>>.SuccessResponse(list));
    }
}
