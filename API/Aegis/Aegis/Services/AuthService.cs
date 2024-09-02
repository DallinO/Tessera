using Microsoft.AspNetCore.Identity;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Aegis.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Aegis.Services
{
    public class AuthService
    {
        private readonly UserManager<Scribe> _userManager;
        private readonly SignInManager<Scribe> _signInManager;
        private readonly TesseraDbContext _dbContext;
        private readonly BookService _bookService;

        public AuthService(UserManager<Scribe> userManager, SignInManager<Scribe> signInManager, TesseraDbContext dbContext, BookService bookService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _bookService = bookService;
        }


        /***************************************************
         * REGISTER ASYNC
         * - Add a user to the Tessera AspNetUsers
         * table.
         ***************************************************/
        public async Task<IdentityResult> RegisterAsync(RegisterDefaultModel model)
        {
            var user = new Scribe
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserName = model.Email,
                Email = model.Email,
                Database = "tessera-pm-01"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            return result;
        }


        /***************************************************
         * LOGIN ASYNC
         * - Verify user credentials.
         ***************************************************/
        public async Task<(SignInResult, Scribe)> LoginAsync(LoginRequest model)
        {
            // Attempt to sign in the user
            var signInResult = await _signInManager.PasswordSignInAsync(
                model.Email, 
                model.Password, 
                model.RememberMe, 
                lockoutOnFailure: false );

            if (signInResult.Succeeded)
            {

                // Get the user from the database
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return (signInResult, user);
                }
                else
                {
                    return (signInResult, user);
                }
            }

            return (signInResult, null);
        }

        private string GenerateJwtToken(Scribe user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                // Add other claims as needed
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("@Andromeda1789&Sagittarius0476&Centuarus247"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "http://localhost",
                audience: "http://localhost",
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            var handler = new JwtSecurityTokenHandler().WriteToken(token);
            return handler;
        }


        /***************************************************
         * CREATE BOOK ASYNC
         * - Create a book.
         ***************************************************/
        /// <summary>
        /// Creates a new book entry in the database and associates it with a catalog entry.
        /// </summary>
        /// <param name="book">The book entity to be created.</param>
        /// <returns>
        /// A tuple where the first item is a boolean indicating success or failure,
        /// and the second item is a string message providing additional information.
        /// Returns <c>false</c> with a descriptive error message if the book already exists,
        /// or if there is an error saving to the database. Returns <c>true</c> with <c>null</c> 
        /// if the operation succeeds.
        /// </returns>
        public async Task<(bool, string)> CreateBookAsync(BookEntity book)
        {
            bool bookExists = await _dbContext.Library.AnyAsync(o => o.Title == book.Title);
            if (bookExists)
            {
                return (false, "Organization Already Exists In Master Database");
            }

            _dbContext.Library.Add(book);
            try
            {
                await _dbContext.SaveChangesAsync();

                // Create the entry in the UserOrganization join table
                var catalogEntry = new Catalog
                {
                    ScribeId = book.ScribeId,
                    BookId = book.Id,
                    IsOwner = true
                };

                _dbContext.Catalog.Add(catalogEntry);

                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return (false, "Error occured while saving to the database");
            }
            catch (OperationCanceledException)
            {
                return (false, "Save changes operation was cancelled");
            }

            return (true, null);
        }


        /***************************************************
         * GET BOOKS
         * - Retrieves the Books associated with the
         * user.
         ***************************************************/
        /// <summary>
        /// Retrieves a list of books associated with the user identified by the specified email.
        /// </summary>
        /// <param name="email">The email address of the user whose books are to be retrieved.</param>
        /// <returns>
        /// A list of <see cref="BookDto"/> objects representing the books associated with the user. Returns null if the user is not found or has no books.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="email"/> parameter is null or whitespace.</exception>
        public async Task<List<BookDto>> GetBooks(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            // Check if the user has any organizations assigned
            var books = await (from cl in _dbContext.Catalog
                                    join bk in _dbContext.Library
                                    on cl.BookId equals bk.Id
                                    where cl.ScribeId == user.Id
                                    select new BookDto
                                    {
                                        Id = bk.Id,
                                        Title = bk.Title,
                                        IsOwner = cl.IsOwner
                                    })
                                    .ToListAsync();

            return books.Count == 0 ? null : books;
            
        }


        /***************************************************
         * GET AUTHOR
         * - Retrieves the scibe data associated with the
         * users email.
         ***************************************************/
        /// <summary>
        /// Gets the author data from the master database.
        /// </summary>
        /// <param name="email">The email of the target user</param>
        /// <returns>Returns <see cref="ScribeDto"/> or null if the user could not be found.</returns>
        /// <exception cref="ArgumentNullException"> if 'email' is null, empty, or a whitespace.</exception>
        public async Task<ScribeDto> GetAuthor(string email)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(email);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            return new ScribeDto()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Database = user.Database,
                Email = email
            };
        }


        /***************************************************
         * GET BOOK ID BY TITLE
         * - Add a user to the Tessera AspNetUsers
         * table
         ***************************************************/
        public async Task<Guid?> GetBookIdByTitleAsync(string title)
        {
            var book = await _dbContext.Library
                .FirstOrDefaultAsync(b => b.Title == title);

            return book?.Id;
        }
    }
}
