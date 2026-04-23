namespace matchmaking.Domain.Entities;

public class User
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PreferredLocation { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string Education { get; set; } = string.Empty;
    public string Resume { get; set; } = string.Empty;
    public string PreferredEmploymentType { get; set; } = string.Empty;
}
