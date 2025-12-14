using API.Models.DsfTables;

namespace API.Models.Dtos;

public class CreateNotificationRequest
{
    public required Order Order { get; set; }
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
}