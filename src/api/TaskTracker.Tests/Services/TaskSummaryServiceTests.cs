using FluentAssertions;
using Moq;
using TaskTracker.Application.Interfaces;
using TaskTracker.Application.Services;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Tests.Services;

public class TaskSummaryServiceTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly Mock<IAiSummaryProvider> _aiProviderMock;
    private readonly TaskSummaryService _taskSummaryService;

    public TaskSummaryServiceTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
        _aiProviderMock = new Mock<IAiSummaryProvider>();
        _taskSummaryService = new TaskSummaryService(
            _repositoryMock.Object,
            _aiProviderMock.Object);
    }

    [Fact]
    public async Task SummarizeTodayTasksAsync_ShouldReturnDefaultMessage_WhenNoTasksExist()
    {
        _repositoryMock
            .Setup(repo => repo.GetTasksDueTodayAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TaskItem>());

        var result = await _taskSummaryService.SummarizeTodayTasksAsync();

        result.TotalTasks.Should().Be(0);
        result.Summary.Should().Be("You do not have any tasks due today.");

        _aiProviderMock.Verify(
            provider => provider.SummarizeTasksAsync(It.IsAny<IEnumerable<TaskItem>>()),
            Times.Never);
    }

    [Fact]
    public async Task SummarizeTodayTasksAsync_ShouldCallAiProviderOnce_WhenTasksExist()
    {
        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task A", Status = TaskItemStatus.Todo, DueDate = DateTime.UtcNow },
            new() { Id = 2, Title = "Task B", Status = TaskItemStatus.Done, DueDate = DateTime.UtcNow }
        };

        _repositoryMock
            .Setup(repo => repo.GetTasksDueTodayAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(tasks);

        _aiProviderMock
            .Setup(provider => provider.SummarizeTasksAsync(It.IsAny<IEnumerable<TaskItem>>()))
            .ReturnsAsync("You have 2 tasks today. 1 completed, 0 in progress, 1 pending.");

        await _taskSummaryService.SummarizeTodayTasksAsync();

        _aiProviderMock.Verify(
            provider => provider.SummarizeTasksAsync(It.IsAny<IEnumerable<TaskItem>>()),
            Times.Once);
    }

    [Fact]
    public async Task SummarizeTodayTasksAsync_ShouldReturnGeneratedSummary_WhenTasksExist()
    {
        const string expectedSummary = "You have 3 tasks today. 2 completed, 1 in progress, 0 pending.";

        var tasks = new List<TaskItem>
        {
            new() { Id = 1, Title = "Task A", Status = TaskItemStatus.Done, DueDate = DateTime.UtcNow },
            new() { Id = 2, Title = "Task B", Status = TaskItemStatus.Done, DueDate = DateTime.UtcNow },
            new() { Id = 3, Title = "Task C", Status = TaskItemStatus.InProgress, DueDate = DateTime.UtcNow }
        };

        _repositoryMock
            .Setup(repo => repo.GetTasksDueTodayAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(tasks);

        _aiProviderMock
            .Setup(provider => provider.SummarizeTasksAsync(It.IsAny<IEnumerable<TaskItem>>()))
            .ReturnsAsync(expectedSummary);

        var result = await _taskSummaryService.SummarizeTodayTasksAsync();

        result.TotalTasks.Should().Be(3);
        result.Summary.Should().Be(expectedSummary);
        result.Date.Should().Be(DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task SummarizeTodayTasksAsync_ShouldSetDateToToday()
    {
        _repositoryMock
            .Setup(repo => repo.GetTasksDueTodayAsync(It.IsAny<DateTime>()))
            .ReturnsAsync(new List<TaskItem>());

        var result = await _taskSummaryService.SummarizeTodayTasksAsync();

        result.Date.Should().Be(DateTime.UtcNow.Date);
    }
}
