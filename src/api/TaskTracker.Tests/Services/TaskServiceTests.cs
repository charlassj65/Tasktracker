using FluentAssertions;
using Moq;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _taskService = new TaskService(_taskRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenStatusIsDoneAndTitleIsWhitespace()
    {
        var request = new CreateTaskRequest
        {
            Title = "   ",
            Status = TaskItemStatus.Done
        };

        Func<Task> act = async () => await _taskService.CreateAsync(request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("A task cannot be marked as Done if the Title is empty or whitespace.");
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenStatusIsDoneAndTitleIsEmpty()
    {
        var request = new CreateTaskRequest
        {
            Title = "",
            Status = TaskItemStatus.Done
        };

        Func<Task> act = async () => await _taskService.CreateAsync(request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("A task cannot be marked as Done if the Title is empty or whitespace.");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTask_WhenRequestIsValid()
    {
        var request = new CreateTaskRequest
        {
            Title = "Complete assignment",
            Description = "Build Task Tracker API",
            Status = TaskItemStatus.Todo,
            DueDate = DateTime.UtcNow.AddDays(2)
        };

        _taskRepositoryMock
            .Setup(repo => repo.CreateAsync(It.IsAny<TaskItem>()))
            .ReturnsAsync((TaskItem task) =>
            {
                task.Id = 1;
                return task;
            });

        var result = await _taskService.CreateAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be("Complete assignment");
        result.Status.Should().Be(TaskItemStatus.Todo);

        _taskRepositoryMock.Verify(
            repo => repo.CreateAsync(It.IsAny<TaskItem>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
    {
        var request = new UpdateTaskRequest
        {
            Title = "Updated task",
            Status = TaskItemStatus.InProgress
        };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync((TaskItem?)null);

        var result = await _taskService.UpdateAsync(1, request);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenStatusIsDoneAndTitleIsWhitespace()
    {
        var existingTask = new TaskItem { Id = 1, Title = "Existing task", Status = TaskItemStatus.Todo };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(existingTask);

        var request = new UpdateTaskRequest
        {
            Title = "   ",
            Status = TaskItemStatus.Done
        };

        Func<Task> act = async () => await _taskService.UpdateAsync(1, request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("A task cannot be marked as Done if the Title is empty or whitespace.");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTask_WhenRequestIsValid()
    {
        var existingTask = new TaskItem
        {
            Id = 1,
            Title = "Old task",
            Description = "Old description",
            Status = TaskItemStatus.Todo
        };

        var request = new UpdateTaskRequest
        {
            Title = "Updated task",
            Description = "Updated description",
            Status = TaskItemStatus.InProgress,
            DueDate = DateTime.UtcNow.AddDays(3)
        };

        _taskRepositoryMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(existingTask);

        _taskRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<TaskItem>()))
            .Returns(Task.CompletedTask);

        var result = await _taskService.UpdateAsync(1, request);

        result.Should().BeTrue();
        existingTask.Title.Should().Be("Updated task");
        existingTask.Description.Should().Be("Updated description");
        existingTask.Status.Should().Be(TaskItemStatus.InProgress);

        _taskRepositoryMock.Verify(
            repo => repo.UpdateAsync(existingTask),
            Times.Once);
    }
}
