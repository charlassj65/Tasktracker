using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskTracker.Api.Controllers;
using TaskTracker.Application.DTOs;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ITaskSummaryService> _taskSummaryServiceMock;
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _taskSummaryServiceMock = new Mock<ITaskSummaryService>();
        _controller = new TasksController(_taskServiceMock.Object, _taskSummaryServiceMock.Object);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction_WhenRequestIsValid()
    {
        var request = new CreateTaskRequest
        {
            Title = "Complete assignment",
            Status = TaskItemStatus.Todo
        };

        var response = new TaskResponse
        {
            Id = 1,
            Title = "Complete assignment",
            Status = TaskItemStatus.Todo
        };

        _taskServiceMock
            .Setup(service => service.CreateAsync(request))
            .ReturnsAsync(response);

        var result = await _controller.Create(request);

        var createdResult = result.Result.Should()
            .BeOfType<CreatedAtActionResult>()
            .Subject;

        createdResult.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(TasksController.GetById));
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        _taskServiceMock
            .Setup(service => service.GetByIdAsync(100))
            .ReturnsAsync((TaskResponse?)null);

        var result = await _controller.GetById(100);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenUpdateIsSuccessful()
    {
        var request = new UpdateTaskRequest
        {
            Title = "Updated task",
            Status = TaskItemStatus.InProgress
        };

        _taskServiceMock
            .Setup(service => service.UpdateAsync(1, request))
            .ReturnsAsync(true);

        var result = await _controller.Update(1, request);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        var request = new UpdateTaskRequest
        {
            Title = "Updated task",
            Status = TaskItemStatus.InProgress
        };

        _taskServiceMock
            .Setup(service => service.UpdateAsync(1, request))
            .ReturnsAsync(false);

        var result = await _controller.Update(1, request);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenDeleteIsSuccessful()
    {
        _taskServiceMock
            .Setup(service => service.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        _taskServiceMock
            .Setup(service => service.DeleteAsync(99))
            .ReturnsAsync(false);

        var result = await _controller.Delete(99);

        result.Should().BeOfType<NotFoundResult>();
    }
}
