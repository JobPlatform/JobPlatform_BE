namespace JobPlatform.Domain.Entities.Profiles;
public class EmployerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public required string CompanyName { get; set; }
    public string? Website { get; set; }
    public string? Location { get; set; }
}