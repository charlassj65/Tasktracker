using Microsoft.EntityFrameworkCore;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Data;

namespace TaskTracker.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _dbContext;

    public TaskRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskItem> CreateAsync(TaskItem taskItem)
    {
        await _dbContext.Tasks.AddAsync(taskItem);
        await _dbContext.SaveChangesAsync();

        return taskItem;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync()
    {
        return await _dbContext.Tasks
            .AsNoTracking()
            .OrderBy(task => task.Id)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _dbContext.Tasks
            .FirstOrDefaultAsync(task => task.Id == id);
    }

    public async Task UpdateAsync(TaskItem taskItem)
    {
        _dbContext.Tasks.Update(taskItem);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var task = await _dbContext.Tasks.FindAsync(id);

        if (task is null)
        {
            return false;
        }

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksDueTodayAsync(DateTime today)
    {
        var startOfDay = today.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _dbContext.Tasks
            .AsNoTracking()
            .Where(task =>
                task.DueDate.HasValue &&
                task.DueDate.Value >= startOfDay &&
                task.DueDate.Value < endOfDay)
            .OrderBy(task => task.Status)
            .ThenBy(task => task.DueDate)
            .ToListAsync();
    }
}
