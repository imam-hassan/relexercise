using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using user.Models;
using user.Data;

namespace user.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly AppDbContext _context;

    public JobsController(AppDbContext context) => _context = context;

    // 1. CREATE: api/jobs (Creates a job independently)
    [HttpPost]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
    {
        var job = new Job
        {
            Title = request.Title,
            Description = request.Description
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, job);
    }

    // 2. READ ALL: api/jobs
// READ ALL: api/jobs
[HttpGet]
public async Task<IActionResult> GetJobs()
{
    var jobs = await _context.Jobs
        .AsNoTracking()
        .Include(j => j.Assignments)
        .ThenInclude(a => a.User)
        .ToListAsync();

    // Clean up the response to remove the nested EF Core loops
    var response = jobs.Select(job => new
    {
        JobId = job.Id,
        job.Title,
        job.Description,
        // Only return the user information you actually care about
        AssignedUsers = job.Assignments
            .Where(a => a.User != null)
            .Select(a => new
            {
                a.User.Id,
                a.User.Name,
                a.User.Email
            }).ToList()
    });

    return Ok(response);
}


    // 3. READ BY ID: api/jobs/{id} (Accesses all users assigned to this job)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobById(int id)
    {
        var job = await _context.Jobs
            .Include(j => j.Assignments)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (job == null) return NotFound($"Job with ID {id} not found.");

        // Clean response showing the job details and a list of all assigned users
        var response = new
        {
            JobId = job.Id,
            job.Title,
            job.Description,
            AssignedUsers = job.Assignments.Select(a => new
            {
                a.User.Id,
                a.User.Name,
                a.User.Email
            }).ToList()
        };

        return Ok(response);
    }

    // 4. UPDATE: api/jobs/{id} (Updates only the job details)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobRequest request)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job == null) return NotFound($"Job with ID {id} not found.");

        job.Title = request.Title;
        job.Description = request.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // 5. DELETE: api/jobs/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _context.Jobs.FindAsync(id);
        if (job == null) return NotFound($"Job with ID {id} not found.");

        _context.Jobs.Remove(job);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

// Clean Request DTOs (Removed UserId since Jobs are handled independently)
public class CreateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateJobRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
