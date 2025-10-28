using System;

namespace Domain.Entities;

public class AiSessionMessage
{
    public int Id { get; set; }

    public string SessionId { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
