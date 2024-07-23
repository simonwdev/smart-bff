namespace SmartBff.EntityFrameworkCore;

public class SmartBffSession
{
    public Guid Id { get; set; }
    
    public string? Name { get; set; }
    
    public string Type { get; set; } = string.Empty;

    public string RegistrationId { get; set; } = string.Empty;

    public byte[] Payload { get; set; } = [];

    public DateTime CreatedOn { get; set; }
    
    public DateTime? ExpiresOn { get; set; }
    
    public Guid ConcurrencyToken { get; set; }
}