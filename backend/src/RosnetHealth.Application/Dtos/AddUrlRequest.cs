using System.ComponentModel.DataAnnotations;

namespace RosnetHealth.Application.Dtos;

public record AddUrlRequest(
    [Required, StringLength(200)] string Name,
    [Required, Url, StringLength(2000)] string Url);
