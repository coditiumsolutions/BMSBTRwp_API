namespace BMSBTRwp_API.Models;

/// <summary>
/// Strongly typed model for dbo.Operators as defined in db.txt.
/// </summary>
public class Operator
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Project { get; set; }
    public string? Email { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
}

