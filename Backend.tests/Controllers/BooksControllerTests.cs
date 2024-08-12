using Backend.Controllers;
using Backend.Exceptions;
using Backend.Models;
using Backend.Services;

using Microsoft.AspNetCore.Mvc;

namespace Backend.tests.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BooksControllerTests
{
    private readonly BooksController controller;
    private readonly IBookService bookService;

    public BooksControllerTests()
    {
        bookService = Mock.Of<IBookService>();
        controller = new BooksController(bookService);
    }

    [Fact]
    public async Task Get_ShouldReturnListOfBooks()
    {
        // Arrange
        var expectedBooks = new List<BookDTO>
        {
            new() { Id = 1, Title = "Book 1" },
            new() { Id = 2, Title = "Book 2" }
        };
        Mock.Get(bookService).Setup(service => service.GetAllAsync()).ReturnsAsync(expectedBooks);

        // Act
        var result = await controller.Get();

        // Assert
        result.Value.Should().BeEquivalentTo(expectedBooks);
    }

    [Fact]
    public async Task Get_WithValidId_ShouldReturnBook()
    {
        // Arrange
        int bookId = 1;
        var expectedBook = new BookDTOWithUser { Id = bookId, Title = "Book 1" };
        Mock.Get(bookService).Setup(service => service.GetByIdAsync(bookId)).ReturnsAsync(expectedBook);

        // Act
        var result = await controller.Get(bookId);

        // Assert
        result.Value.Should().BeEquivalentTo(expectedBook);
    }

    [Fact]
    public async Task Get_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        int bookId = 1;
        Mock.Get(bookService).Setup(service => service.GetByIdAsync(bookId)).ReturnsAsync((BookDTOWithUser?)null);

        // Act
        var result = await controller.Get(bookId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Post_WithValidBook_ShouldReturnCreatedBook()
    {
        // Arrange
        var bookToAdd = new BookAddDTO { Title = "New Book" };
        var createdBook = new BookDTO { Id = 1, Title = "New Book" };
        Mock.Get(bookService).Setup(service => service.AddAsync(bookToAdd)).ReturnsAsync(createdBook);

        // Act
        var result = await controller.Post(bookToAdd);

        // Assert
        result.Value.Should().BeEquivalentTo(createdBook);
    }

    [Fact]
    public async Task Put_WithValidBook_ShouldReturnUpdatedBook()
    {
        // Arrange
        var bookToEdit = new BookEditDTO { Id = 1, Title = "Updated Book" };
        var updatedBook = new BookDTO { Id = 1, Title = "Updated Book" };
        Mock.Get(bookService).Setup(service => service.UpdateAsync(bookToEdit)).ReturnsAsync(updatedBook);

        // Act
        var result = await controller.Put(bookToEdit);

        // Assert
        result.Value.Should().BeEquivalentTo(updatedBook);
    }

    [Fact]
    public async Task Put_WithInvalidBook_ShouldReturnNotFound()
    {
        // Arrange
        var bookToEdit = new BookEditDTO { Id = 1, Title = "Updated Book" };
        Mock.Get(bookService).Setup(service => service.UpdateAsync(bookToEdit)).Throws<NotFoundException>();

        // Act
        var result = await controller.Put(bookToEdit);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        int bookId = 1;
        Mock.Get(bookService).Setup(service => service.DeleteAsync(bookId)).Verifiable();

        // Act
        var result = await controller.Delete(bookId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        Mock.Get(bookService).Verify(service => service.DeleteAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        int bookId = 1;
        Mock.Get(bookService).Setup(service => service.DeleteAsync(bookId)).Throws<NotFoundException>();

        // Act
        var result = await controller.Delete(bookId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
