using Microsoft.AspNetCore.Mvc;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ITaskSummaryService _taskSummaryService;

    public TasksController(ITaskService taskService, ITaskSummaryService taskSummaryService)
    {
        _taskService = taskService;
        _taskSummaryService = taskSummaryService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request)
    {
        var createdTask = await _taskService.CreateAsync(request);

        return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll()
    {
        var tasks = await _taskService.GetAllAsync();

        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);

        if (task is null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateTaskRequest request)
    {
        var updated = await _taskService.UpdateAsync(id, request);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("summary/today")]
    [ProducesResponseType(typeof(TaskSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskSummaryResponse>> GetTodaySummary()
    {
        var summary = await _taskSummaryService.SummarizeTodayTasksAsync();

        return Ok(summary);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _taskService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
