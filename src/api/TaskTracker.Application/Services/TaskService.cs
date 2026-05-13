using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request)
    {
        ValidateBusinessRules(request.Title, request.Status);

        var taskItem = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Status = request.Status,
            DueDate = request.DueDate
        };

        var createdTask = await _taskRepository.CreateAsync(taskItem);

        return MapToResponse(createdTask);
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();

        return tasks.Select(MapToResponse).ToList();
    }

    public async Task<TaskResponse?> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);

        return task is null ? null : MapToResponse(task);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskRequest request)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);

        if (existingTask is null)
        {
            return false;
        }

        ValidateBusinessRules(request.Title, request.Status);

        existingTask.Title = request.Title.Trim();
        existingTask.Description = request.Description;
        existingTask.Status = request.Status;
        existingTask.DueDate = request.DueDate;

        await _taskRepository.UpdateAsync(existingTask);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _taskRepository.DeleteAsync(id);
    }

    private static void ValidateBusinessRules(string? title, TaskItemStatus status)
    {
        if (status == TaskItemStatus.Done && string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidOperationException(
                "A task cannot be marked as Done if the Title is empty or whitespace.");
        }
    }

    private static TaskResponse MapToResponse(TaskItem task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            DueDate = task.DueDate
        };
    }
}
