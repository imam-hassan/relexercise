using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using user.Models;
using user.Data;

namespace user.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AssignmentsController(AppDbContext context) => _context = context;

    // 1. ASSIGN: api/assignments
    [HttpPost]
    public async Task<IActionResult> AssignJob([FromBody] AssignmentRequest request)
    {
        // Check if the User exists
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
        if (!userExists) return BadRequest($"User with ID {request.UserId} does not exist.");

        // Check if the Job exists
        var jobExists = await _context.Jobs.AnyAsync(j => j.Id == request.JobId);
        if (!jobExists) return BadRequest($"Job with ID {request.JobId} does not exist.");

        // Check if this assignment already exists to prevent duplicates
        var alreadyAssigned = await _context.Assignments
            .AnyAsync(a => a.UserId == request.UserId && a.JobId == request.JobId);
        
        if (alreadyAssigned) return BadRequest("This job is already assigned to this user.");

        // Create the linkage row
        var assignment = new Assign
        {
            UserId = request.UserId,
            JobId = request.JobId
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Job successfully assigned to user." });
    }

    // 2. UNASSIGN: api/assignments
    [HttpDelete]
    public async Task<IActionResult> UnassignJob([FromBody] AssignmentRequest request)
    {
        // Find the existing assignment row
        var assignment = await _context.Assignments
            .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.JobId == request.JobId);

        if (assignment == null) 
        {
            return NotFound("No assignment found linking this user and job.");
        }

        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// Data Transfer Object (DTO) used for both assignment operations
public class AssignmentRequest
{
    public int UserId { get; set; }
    public int JobId { get; set; }
}
