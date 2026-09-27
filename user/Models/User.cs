namespace user.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Navigation Property: Link to the assignment join table
    public List<Assign> Assignments { get; set; } = new();
}
