using RosnetHealth.Domain.Enums;

namespace RosnetHealth.Application.Dtos;

public record HealthCheckHistoryEntryDto(
    DateTime CheckedAt,
    int? StatusCode,
    long ResponseTimeMs,
    HealthStatus Status,
    string? ErrorMessage);
