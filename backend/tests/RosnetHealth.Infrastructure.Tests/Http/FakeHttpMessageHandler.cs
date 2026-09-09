namespace RosnetHealth.Infrastructure.Tests.Http;

internal class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
    public static FakeHttpMessageHandler ReturningStatus(System.Net.HttpStatusCode statusCode) =>
        new(_ => new HttpResponseMessage(statusCode));

    public static FakeHttpMessageHandler Throwing(Exception exception) =>
        new(_ => throw exception);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(respond(request));
    }
}
