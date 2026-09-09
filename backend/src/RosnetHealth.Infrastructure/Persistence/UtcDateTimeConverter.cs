using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RosnetHealth.Infrastructure.Persistence;

/// <summary>
/// SQLite has no native datetime type and doesn't round-trip DateTimeKind - every DateTime
/// read back from the database comes back as Kind=Unspecified, even though we only ever store
/// UTC values. Without this, System.Text.Json omits the 'Z' suffix when serializing, and
/// JavaScript's Date constructor then misinterprets the timestamp as local time.
/// </summary>
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v,
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
