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
        var todos = await _context.Database
            .SqlQueryRaw<TodoItem>("SELECT Id, Task, IsCompleted FROM Todos")
            .ToListAsync();

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

        var newId = await _context.Database
            .SqlQueryRaw<int>(
                "INSERT INTO Todos (Task, IsCompleted) VALUES ({0}, {1}) RETURNING Id",
                item.Task,
                item.IsCompleted ? 1 : 0
            )
            .SingleAsync();

        item.Id = newId;

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

        int rowsAffected = await _context.Database.ExecuteSqlRawAsync(
            "UPDATE Todos SET Task = {0}, IsCompleted = {1} WHERE Id = {2}",
            item.Task,
            item.IsCompleted ? 1 : 0,
            item.Id
        );

        if (rowsAffected == 0)
        {
            return NotFound();
        }

        return NoContent();
    }
}