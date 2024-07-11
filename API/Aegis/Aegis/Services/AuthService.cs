using Microsoft.AspNetCore.Identity;
using Tessera.Models;
using Aegis.Data;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Microsoft.EntityFrameworkCore;
using Tessera.Constants;
using Aegis.Migrations;

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
                Email = model.Email
            };

            return await _userManager.CreateAsync(user, model.Password);
        }


        /***************************************************
         * LOGIN ASYNC
         * - Verify user credentials.
         ***************************************************/
        public async Task<SignInResult> LoginAsync(LoginDefaultModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        }


        /***************************************************
         * REGISTER ASYNC
         * - Create an Organization.
         ***************************************************/
        public async Task<(bool Success, string ErrorMessage)> CreateBookAsync(string ownerId, BookModel model)
        {
            bool bookExists = await _dbContext.Library.AnyAsync(o => o.Title == model.Name);
            if (bookExists)
            {
                return (false, "Organization Already Exists");
            }

            // Create the organization
            var organization = new Preface
            {
                Title = model.Name,
                OwnerId = ownerId
            };

            _dbContext.Library.Add(organization);
            await _dbContext.SaveChangesAsync();

            // Create the entry in the UserOrganization join table
            var userOrganization = new Catalog
            {
                ScribeId = ownerId,
                BookId = organization.Id,
                IsOwner = true
            };

            _dbContext.Catalog.Add(userOrganization);
            await _dbContext.SaveChangesAsync();

            await CreateBookDatabase(organization.Id);

            return (true, null);
        }


        /***************************************************
         * GET ORGANIZATIONS
         * - Retrieves the Organizations associated with the
         * user.
         ***************************************************/
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
         * REGISTER ASYNC
         * - Add a user to the Tessera AspNetUsers
         * table
         ***************************************************/
        private async Task CreateBookDatabase(Guid bookId)
        {
            string dbName = $"BOOK_{bookId}";

            string createDatabaseSql = $"CREATE DATABASE [{dbName}]";
            await _dbContext.Database.ExecuteSqlRawAsync(createDatabaseSql);
            await _bookService.InitializeBookDatabase(dbName);
        }

        public async Task<Guid?> GetBookIdByTitleAsync(string title)
        {
            var book = await _dbContext.Library
                .FirstOrDefaultAsync(b => b.Title == title);

            return book?.Id;
        }
    }
}
