using Backend.Controllers;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services;

using Microsoft.AspNetCore.Mvc;

namespace Backend.tests.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersControllerTests
{
    private readonly UsersController controller;
    private readonly IUserService userService;

    public UsersControllerTests()
    {
        userService = Mock.Of<IUserService>();
        controller = new UsersController(userService);
    }

    [Fact]
    public async Task Get_ShouldReturnListOfUsers()
    {
        // Arrange
        var expectedUsers = new List<UserDTO>
        {
            new() { Id = 1, Forename = "Bob", Surname = "Smith" },
            new() { Id = 2, Forename = "Brian", Surname = "Jones" }
        };
        Mock.Get(userService).Setup(service => service.GetAllAsync()).ReturnsAsync(expectedUsers);

        // Act
        var result = await controller.Get();

        // Assert
        result.Value.Should().BeEquivalentTo(expectedUsers);
    }

    [Fact]
    public async Task Get_WithValidId_ShouldReturnBook()
    {
        // Arrange
        int userId = 1;
        var expectedUser = new UserDTOWithBooks { Id = userId, Forename = "Bob", Surname = "Smith", Books = [new() { Id = 1, Title = "Book1" }] };
        Mock.Get(userService).Setup(service => service.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

        // Act
        var result = await controller.Get(userId);

        // Assert
        result.Value.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task Get_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        int userId = 1;
        Mock.Get(userService).Setup(service => service.GetByIdAsync(userId)).ReturnsAsync((UserDTOWithBooks?)null);

        // Act
        var result = await controller.Get(userId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Post_WithValidBook_ShouldReturnCreatedBook()
    {
        // Arrange
        var userToAdd = new UserAddDTO { Forename = "Bob", Surname = "Smith" };
        var createdUser = new UserDTO { Id = 1, Forename = "Bob", Surname = "Smith" };
        Mock.Get(userService).Setup(service => service.AddAsync(userToAdd)).ReturnsAsync(createdUser);

        // Act
        var result = await controller.Post(userToAdd);

        // Assert
        result.Value.Should().BeEquivalentTo(createdUser);
    }

    [Fact]
    public async Task Put_WithValidBook_ShouldReturnUpdatedBook()
    {
        // Arrange
        var userToEdit = new UserEditDTO { Id = 1, Forename = "Bob", Surname = "Smith" };
        var updatedUser = new UserDTO { Id = 1, Forename = "Bob", Surname = "Smith" };
        Mock.Get(userService).Setup(service => service.UpdateAsync(userToEdit)).ReturnsAsync(updatedUser);

        // Act
        var result = await controller.Put(userToEdit);

        // Assert
        result.Value.Should().BeEquivalentTo(updatedUser);
    }

    [Fact]
    public async Task Put_WithInvalidBook_ShouldReturnNotFound()
    {
        // Arrange
        var userToEdit = new UserEditDTO { Id = 1, Forename = "Bob", Surname = "Smith" };
        Mock.Get(userService).Setup(service => service.UpdateAsync(userToEdit)).Throws<NotFoundException>();

        // Act
        var result = await controller.Put(userToEdit);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        int userId = 1;
        Mock.Get(userService).Setup(service => service.DeleteAsync(userId)).Verifiable();

        // Act
        var result = await controller.Delete(userId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        Mock.Get(userService).Verify(service => service.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        int userId = 1;
        Mock.Get(userService).Setup(service => service.DeleteAsync(userId)).Throws<NotFoundException>();

        // Act
        var result = await controller.Delete(userId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Borrow_ServiceOk_ShouldReturnNoContent()
    {
        // Arrange
        var borrow = new BorrowDTO(1, 2);
        Mock.Get(userService).Setup(service => service.BorrowAsync(1, 2)).Verifiable();

        // Act
        var result = await controller.Borrow(borrow);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        Mock.Get(userService).Verify(service => service.BorrowAsync(borrow.UserId, borrow.BookId), Times.Once);
    }

    [Fact]
    public async Task Borrow_NotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var borrow = new BorrowDTO(1, 2);
        Mock.Get(userService).Setup(service => service.BorrowAsync(borrow.UserId, borrow.BookId)).ThrowsAsync(new NotFoundException());

        // Act
        var result = await controller.Borrow(borrow);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        Mock.Get(userService).Verify(service => service.BorrowAsync(borrow.UserId, borrow.BookId), Times.Once);
    }

    [Fact]
    public async Task Borrow_BookNotAvailableException_ShouldReturnBadRequest()
    {
        // Arrange
        var borrow = new BorrowDTO(1, 2);
        Mock.Get(userService).Setup(service => service.BorrowAsync(borrow.UserId, borrow.BookId)).ThrowsAsync(new BookNotAvailableException());

        // Act
        var result = await controller.Borrow(borrow);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        Mock.Get(userService).Verify(service => service.BorrowAsync(borrow.UserId, borrow.BookId), Times.Once);
    }

    [Fact]
    public async Task Return_ServiceOk_ShouldReturnNoContent()
    {
        // Arrange
        const int bookId = 1;
        Mock.Get(userService).Setup(service => service.ReturnAsync(1)).Verifiable();

        // Act
        var result = await controller.Return(bookId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        Mock.Get(userService).Verify(service => service.ReturnAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task Return_NotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        const int bookId = 1;
        Mock.Get(userService).Setup(service => service.ReturnAsync(bookId)).ThrowsAsync(new NotFoundException());

        // Act
        var result = await controller.Return(bookId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        Mock.Get(userService).Verify(service => service.ReturnAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task Return_BookNotAvailableException_ShouldReturnBadRequest()
    {
        // Arrange
        const int bookId = 1;
        Mock.Get(userService).Setup(service => service.ReturnAsync(bookId)).ThrowsAsync(new BookNotCheckedOutException());

        // Act
        var result = await controller.Return(bookId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        Mock.Get(userService).Verify(service => service.ReturnAsync(bookId), Times.Once);
    }
}
