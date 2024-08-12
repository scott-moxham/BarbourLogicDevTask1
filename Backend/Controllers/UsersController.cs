using Backend.Exceptions;
using Backend.Models;
using Backend.Services;

using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IUserService userService;

    public UsersController(IUserService userService)
    {
        this.userService = userService;
    }

    // GET: api/<UsersController>
    /// <summary>
    /// Get a list of all users
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDTO>>> Get()
    {
        return await userService.GetAllAsync();
    }

    // GET api/<UsersController>/5
    /// <summary>
    /// Get details of a single user (including books they have borrowed)
    /// </summary>
    /// <param name="id">User Id</param>
    /// <returns>User details</returns>
    /// <response code="404">User not found</response>
    [ProducesResponseType(404)]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTOWithBooks>> Get(int id)
    {
        var book = await userService.GetByIdAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        return book.Value;
    }

    // POST api/<UsersController>
    /// <summary>
    /// Add a user
    /// </summary>
    /// <param name="book">User details</param>
    /// <returns>User details as saved</returns>
    [HttpPost]
    public async Task<ActionResult<UserDTO>> Post([FromBody] UserAddDTO book)
    {
        return await userService.AddAsync(book);
    }

    // PUT api/<UsersController>
    /// <summary>
    /// Edit a user
    /// </summary>
    /// <param name="book">User details to update</param>
    /// <returns>User details as saved</returns>
    /// <response code="404">User not found</response>
    [ProducesResponseType(404)]
    [HttpPut]
    public async Task<ActionResult<UserDTO>> Put([FromBody] UserEditDTO book)
    {
        try
        {
            return await userService.UpdateAsync(book);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE api/<UsersController>/5
    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">User Id</param>
    /// <response code="204">Deleted successfully</response>
    /// <response code="404">User not found</response>
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await userService.DeleteAsync(id);

            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    // POST api/<UsersController>/Borrow
    /// <summary>
    /// Borrow a book
    /// </summary>
    /// <param name="borrow">Includes User Id and Book Id for loan</param>
    /// <response code="204">Borrowed successfully</response>
    /// <response code="400">Book already checked out</response>
    /// <response code="404">Book / User not found</response>
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [HttpPost("Borrow")]
    public async Task<IActionResult> Borrow([FromBody] BorrowDTO borrow)
    {
        try
        {
            await userService.BorrowAsync(borrow.UserId, borrow.BookId);

            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BookNotAvailableException)
        {
            return BadRequest("Book has been checked out by someone else");
        }
    }

    // POST api/<UsersController>/Return
    /// <summary>
    /// Return a book
    /// </summary>
    /// <param name="bookId">Book Id to return</param>
    /// <response code="204">Returned successfully</response>
    /// <response code="400">Book not checked out</response>
    /// <response code="404">Book not found</response>
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [HttpPost("Return")]
    public async Task<IActionResult> Return(int bookId)
    {
        try
        {
            await userService.ReturnAsync(bookId);

            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BookNotCheckedOutException)
        {
            return BadRequest("Book has not been checked out");
        }
    }
}