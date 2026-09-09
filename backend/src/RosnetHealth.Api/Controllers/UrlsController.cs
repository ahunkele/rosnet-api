using Microsoft.AspNetCore.Mvc;
using RosnetHealth.Application.Dtos;
using RosnetHealth.Application.Interfaces;

namespace RosnetHealth.Api.Controllers;

[ApiController]
[Route("api/urls")]
public class UrlsController(IUrlMonitorService urlMonitorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UrlStatusDto>>> GetUrls()
    {
        var urls = await urlMonitorService.GetUrlsAsync();
        return Ok(urls);
    }

    [HttpPost]
    public async Task<ActionResult<UrlStatusDto>> AddUrl(AddUrlRequest request)
    {
        var dto = await urlMonitorService.AddUrlAsync(request);
        return CreatedAtAction(nameof(GetUrls), routeValues: null, dto);
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<IReadOnlyList<HealthCheckHistoryEntryDto>>> GetHistory(int id, [FromQuery] int take = 50)
    {
        var history = await urlMonitorService.GetHistoryAsync(id, take);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpPatch("{id:int}/active")]
    public async Task<ActionResult<UrlStatusDto>> SetActive(int id, SetActiveRequest request)
    {
        var dto = await urlMonitorService.SetActiveAsync(id, request.IsActive);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUrl(int id)
    {
        var deleted = await urlMonitorService.DeleteUrlAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
