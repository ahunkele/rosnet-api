using System.Net;
using RosnetHealth.Domain.Entities;
using RosnetHealth.Domain.Enums;
using RosnetHealth.Infrastructure.Http;

namespace RosnetHealth.Infrastructure.Tests.Http;

public class HttpUrlHealthCheckerTests
{
    private static MonitoredUrlEntity TestUrl() =>
        new() { Id = 1, Name = "Site", Url = "https://site.example" };

    [Fact]
    public async Task CheckAsync_SuccessResponse_ReturnsUpWithStatusCode()
    {
        using var httpClient = new HttpClient(FakeHttpMessageHandler.ReturningStatus(HttpStatusCode.OK));
        var checker = new HttpUrlHealthChecker(httpClient);

        var result = await checker.CheckAsync(TestUrl(), CancellationToken.None);

        Assert.Equal(HealthStatus.Up, result.Status);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task CheckAsync_ServerErrorResponse_ReturnsDownWithStatusCode()
    {
        using var httpClient = new HttpClient(FakeHttpMessageHandler.ReturningStatus(HttpStatusCode.InternalServerError));
        var checker = new HttpUrlHealthChecker(httpClient);

        var result = await checker.CheckAsync(TestUrl(), CancellationToken.None);

        Assert.Equal(HealthStatus.Down, result.Status);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task CheckAsync_ConnectionFailure_ReturnsDownWithNullStatusCodeAndErrorMessage()
    {
        using var httpClient = new HttpClient(
            FakeHttpMessageHandler.Throwing(new HttpRequestException("Connection refused")));
        var checker = new HttpUrlHealthChecker(httpClient);

        var result = await checker.CheckAsync(TestUrl(), CancellationToken.None);

        Assert.Equal(HealthStatus.Down, result.Status);
        Assert.Null(result.StatusCode);
        Assert.Equal("Connection refused", result.ErrorMessage);
    }

    [Fact]
    public async Task CheckAsync_Timeout_ReturnsDownWithNullStatusCodeAndErrorMessage()
    {
        using var httpClient = new HttpClient(
            FakeHttpMessageHandler.Throwing(new TaskCanceledException("The request timed out", new TimeoutException())));
        var checker = new HttpUrlHealthChecker(httpClient);

        var result = await checker.CheckAsync(TestUrl(), CancellationToken.None);

        Assert.Equal(HealthStatus.Down, result.Status);
        Assert.Null(result.StatusCode);
        Assert.NotNull(result.ErrorMessage);
    }
}
