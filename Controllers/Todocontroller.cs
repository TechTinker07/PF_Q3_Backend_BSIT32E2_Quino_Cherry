using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _context;

    public TodoController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/todo
    [HttpGet]
    public async Task<IActionResult> GetTodos()
    {
        // Use DbSet<T> instead of Database.SqlQueryRaw
        var todos = await _context.Todos.ToListAsync();
        return Ok(todos);
    }

    // POST: api/todo
    [HttpPost]
    public async Task<IActionResult> CreateTodoItem([FromBody] TodoItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Task))
        {
            return BadRequest("Task cannot be empty.");
        }

        // Add via EF Core instead of raw SQL
        _context.Todos.Add(item);
        await _context.SaveChangesAsync(); // Saves and auto-generates ID

        return CreatedAtAction(nameof(GetTodos), new { id = item.Id }, item);
    }

    // PUT: api/todo/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodoItem(int id, [FromBody] TodoItem item)
    {
        if (id != item.Id)
        {
            return BadRequest("ID mismatch.");
        }

        var existing = await _context.Todos.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }



        // Update fields
        existing.Task = item.Task;
        existing.IsCompleted = item.IsCompleted;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/todo/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodoItem(int id)
    {
        int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM Todos WHERE Id = {0}", id
        );

        if (rowsAffected == 0)
            return NotFound();

        return NoContent(); // 204
    }
}