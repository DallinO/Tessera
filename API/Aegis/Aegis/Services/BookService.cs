using Aegis.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tessera.Models.Authentication;
using Tessera.Models.Chapter;
using Microsoft.Data.SqlClient;
using Tessera.Constants;
using Tessera.Models.Book;

namespace Aegis.Services
{
    public class BookService
    {
        private readonly UserManager<Scribe> _userManager;
        private readonly SignInManager<Scribe> _signInManager;
        private readonly DbContextFactory _dbFactory;

        public BookService(UserManager<Scribe> userManager, SignInManager<Scribe> signInManager, DbContextFactory dbFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbFactory = dbFactory;
        }


        /***************************************************
         * INITIALIZE BOOK DATABASE ASYNC
         * - 
         ***************************************************/
        public async Task InitializeBookDatabaseAsync(string dbName)
        {
            string filePath = "C:\\Users\\Olson\\Documents\\GitHub\\Tessera\\API\\Aegis\\Aegis\\SQL Scripts\\InitializeBook.sql";
            string sqlScript = await File.ReadAllTextAsync(filePath);
            string connectionString = Keys.SQL_SERVER_ROOT + $"Database = {dbName}; Trusted_Connection = True; Encrypt = False;";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlScript;
                    await command.ExecuteNonQueryAsync();
                }
            }
        }


        

        /***************************************************
         * GET CHAPTERS ASYNC
         * - 
         ***************************************************/
        public async Task<List<ChapterDto>> GetChaptersAsync(string connectionStringName, Guid? bookId)
        {
            // Create a DbContext instance using the provided connection string
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                try
                {
                    // Query for chapters filtered by bookId
                    var chapters = await dbContext.Chapters
                        .Where(chapter => chapter.BookId == bookId)
                        .ToListAsync();

                    // Convert to DTOs
                    var chapterDtos = chapters.Select(chapter => new ChapterDto
                    {
                        Title = chapter.Title,
                        Description = chapter.Description,
                        //Contents = new List<LeafDto>() // If needed, populate this as well
                    }).ToList();

                    return chapterDtos;
                }
                catch (Exception ex)
                {
                    // Log exception
                    Console.WriteLine($"Error fetching chapters: {ex.Message}");
                    return null;
                }
            }
        }


        /***************************************************
         * ADD CHAPTERS ASYNC
         * - 
         ***************************************************/
        public async Task<(bool, string)> AddChaptersAsync(string connectionStringName, AddChapterRequest request)
        {
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                // Check if a chapter with the same name already exists for the given bookId
                var existingChapter = await dbContext.Chapters
                    .Where(c => c.Title == request.Title && c.BookId == request.BookId)
                    .FirstOrDefaultAsync();

                if (existingChapter != null)
                {
                    // Chapter already exists
                    return (false, "Chapter with the same name already exists for the given book.");
                }

                // Create a new chapter entity
                var newChapter = new ChapterEntity
                {
                    Title = request.Title,
                    Description = request.Description,
                    BookId = request.BookId
                };

                // Add the new chapter to the context
                dbContext.Chapters.Add(newChapter);

                try
                {
                    // Save changes to the database
                    await dbContext.SaveChangesAsync();
                    return (true, "Chapter added successfully.");
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur
                    return (false, $"An error occurred while adding the chapter: {ex.Message}");
                }
            }
        }
    }
}
