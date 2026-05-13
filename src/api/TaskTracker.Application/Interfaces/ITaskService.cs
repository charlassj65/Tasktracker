using TaskTracker.Application.DTOs;

namespace TaskTracker.Application.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request);

    Task<IReadOnlyList<TaskResponse>> GetAllAsync();

    Task<TaskResponse?> GetByIdAsync(int id);

    Task<bool> UpdateAsync(int id, UpdateTaskRequest request);

    Task<bool> DeleteAsync(int id);
}
