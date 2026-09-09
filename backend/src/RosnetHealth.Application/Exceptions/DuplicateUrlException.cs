namespace RosnetHealth.Application.Exceptions;

public class DuplicateUrlException(string url) : Exception($"A monitored URL already exists for '{url}'.")
{
    public string Url { get; } = url;
}
