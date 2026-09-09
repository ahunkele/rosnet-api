using RosnetHealth.Domain.Enums;

namespace RosnetHealth.Application.Dtos;

public record UrlStatusDto(
    int Id,
    string Name,
    string Url,
    bool IsActive,
    HealthStatus? Status,
    int? LastStatusCode,
    long? LastResponseTimeMs,
    DateTime? LastCheckedAt,
    string? LastErrorMessage);
