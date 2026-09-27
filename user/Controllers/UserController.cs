using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using user.Models;
using user.Data; // Added assuming your AppDbContext is in this namespace

namespace user.Controllers;

[ApiController]
[Route("api/user")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context) => _context = context;

    // 1. CREATE: api/user
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = new User 
        { 
            Name = request.Name, 
            Email = request.Email 
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    // 2. READ ALL: api/user
 // 2. READ ALL: api/user (Gets all users with their assigned jobs)
[HttpGet]
public async Task<IActionResult> GetUsers()
{
    var users = await _context.Users
        .AsNoTracking() // Bypasses cache to get fresh database records
        .Include(u => u.Assignments)
            .ThenInclude(a => a.Job)
        .ToListAsync();

    // Map the list into a clean, readable structure
    var response = users.Select(user => new
    {
        UserId = user.Id,
        user.Name,
        user.Email,
        AssignedJobs = user.Assignments
            .Where(a => a.Job != null)
            .Select(a => new
            {
                a.Job.Id,
                a.Job.Title,
                a.Job.Description
            }).ToList()
    });

    return Ok(response);
}


    // 3. READ BY ID: api/user/{id} (Includes all jobs assigned to this user)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users
            .Include(u => u.Assignments)
                .ThenInclude(a => a.Job) // Navigates through the middle table to get Job details
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound($"User with ID {id} not found.");

        // Custom clean response showing user info and their array of assigned jobs
        var response = new
        {
            UserId = user.Id,
            user.Name,
            user.Email,
            AssignedJobs = user.Assignments.Select(a => new
            {
                a.Job.Id,
                a.Job.Title,
                a.Job.Description
            }).ToList()
        };

        return Ok(response);
    }

    // 4. UPDATE: api/user/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound($"User with ID {id} not found.");

        user.Name = request.Name;
        user.Email = request.Email;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 5. DELETE: api/user/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound($"User with ID {id} not found.");

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public class CreateUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
