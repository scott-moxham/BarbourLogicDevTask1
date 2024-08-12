using Backend.Exceptions;
using Backend.Models;
using Backend.Services;

using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Controller for handling books
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class BooksController : ControllerBase
{
    private readonly IBookService bookService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="bookService">Book Service</param>
    public BooksController(IBookService bookService)
    {
        this.bookService = bookService;
    }

    // GET: api/<BooksController>
    /// <summary>
    /// Get a list of all books
    /// </summary>
    /// <returns>List of books</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDTO>>> Get()
    {
        return await bookService.GetAllAsync();
    }

    // GET api/<BooksController>/5
    /// <summary>
    /// Get details of a single book (including who has borrowed it)
    /// </summary>
    /// <param name="id">Book id</param>
    /// <returns>Book details</returns>
    /// <response code="404">Book not found</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDTOWithUser>> Get(int id)
    {
        var book = await bookService.GetByIdAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        return book.Value;
    }

    // POST api/<BooksController>
    /// <summary>
    /// Add a book
    /// </summary>
    /// <param name="book">Book details</param>
    /// <returns>The book details saved, including the Id</returns>
    /// <response code="404">Book not found</response>
    [HttpPost]
    public async Task<ActionResult<BookDTO>> Post([FromBody] BookAddDTO book)
    {
        return await bookService.AddAsync(book);
    }

    // PUT api/<BooksController>
    /// <summary>
    /// Edit an existing book
    /// </summary>
    /// <param name="book">Book details</param>
    /// <returns>Book details saved</returns>
    /// <response code="404">Book not found</response>
    [HttpPut]
    public async Task<ActionResult<BookDTO>> Put([FromBody] BookEditDTO book)
    {
        try
        {
            return await bookService.UpdateAsync(book);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE api/<BooksController>/5
    /// <summary>
    /// Delete a book
    /// </summary>
    /// <param name="id">Book id to delete</param>
    /// <response code="204">Deleted successfully</response>
    /// <response code="404">Book not found</response>
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await bookService.DeleteAsync(id);

            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
