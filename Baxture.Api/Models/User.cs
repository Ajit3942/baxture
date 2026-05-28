namespace Baxture.Api.Models;

public sealed class User
{
    public required string Id { get; init; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public bool IsAdmin { get; set; }
    public required int Age { get; set; }
    public required List<string> Hobbies { get; set; }
    public required string CreatedBy { get; set; }
    public required DateTime CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
