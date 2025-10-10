using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using SocialNet.Core.Enums;
using SocialNet.Core.Models.HealthCheckModels;
using SocialNet.Core.Util.ActionTimerSchedulers;
using System.Net.WebSockets;

namespace SocialNet.Infrastructure.Hubs;

public class HealthHub : Hub
{
    private readonly IHubContext<HealthHub> _healthHubContext;
    private readonly HealthCheckService _healthCheckService;
    private readonly IActionTimerScheduler _actionTimerScheduler;
    private readonly ILogger<HealthHub> _logger;
    private static bool _isFirstInit;

    public HealthHub(
        HealthCheckService healthCheckService,
        IHubContext<HealthHub> healthHubContext,
        IActionTimerScheduler actionTimerScheduler,
        ILogger<HealthHub> logger)
    {
        _healthCheckService = healthCheckService;
        _healthHubContext = healthHubContext;
        _actionTimerScheduler = actionTimerScheduler;
        _logger = logger;

        if (!_isFirstInit)
        {
            _actionTimerScheduler.ScheduleAction(SendHealthStatusToAllAsync, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            _isFirstInit = true;
        }
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var healthCheckModel = new HealthCheckModel
            {
                Status = report.Status.ToString(),
                Results = report.Entries.Select(entry => new HealthCheckResultModel
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Error = entry.Value.Exception?.Message
                })
            };

            if (report.Status == HealthStatus.Unhealthy)
            {
                _logger.LogError($"Health status is unhealthy. Details: {healthCheckModel}");
            }

            await Clients.Caller.SendAsync(WebSocketMessages.ReceiveHealthStatus.ToString(), healthCheckModel);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while sending health status: {ex.Message}");
            throw new WebSocketException($"Error while sending health status: {ex.Message}");
        }

        await base.OnConnectedAsync();
    }

    private async Task SendHealthStatusToAllAsync()
    {
        try
        {
            var report = await _healthCheckService.CheckHealthAsync();
            var healthCheckModel = new HealthCheckModel
            {
                Status = report.Status.ToString(),
                Results = report.Entries.Select(entry => new HealthCheckResultModel
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Description = entry.Value.Description,
                    Error = entry.Value.Exception?.Message
                })
            };

            if (report.Status == HealthStatus.Unhealthy)
            {
                _logger.LogError($"Health status is unhealthy. Details: {healthCheckModel}");
            }

            await _healthHubContext.Clients.All.SendAsync(WebSocketMessages.ReceiveHealthStatus.ToString(), healthCheckModel);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while sending health status: {ex.Message}");
            throw new WebSocketException($"Error while sending health status: {ex.Message}");
        }
    }
}
