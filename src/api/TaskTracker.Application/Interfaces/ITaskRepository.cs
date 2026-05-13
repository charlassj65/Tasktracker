using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem> CreateAsync(TaskItem taskItem);

    Task<IReadOnlyList<TaskItem>> GetAllAsync();

    Task<TaskItem?> GetByIdAsync(int id);

    Task UpdateAsync(TaskItem taskItem);

    Task<bool> DeleteAsync(int id);

    Task<IReadOnlyList<TaskItem>> GetTasksDueTodayAsync(DateTime today);
}
