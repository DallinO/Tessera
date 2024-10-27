using Aegis.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tessera.Models.Authentication;
using Tessera.Models.Chapter;
using Microsoft.Data.SqlClient;
using Tessera.Constants;
using Tessera.Models.Book;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Tessera.CodeGenerators;
using System.Net;

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
        public async Task<List<ChapterDto>> GetChaptersAsync(string connectionStringName, int? bookId)
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

        public async Task<ChapterDto> GetChapterAsync(string connectionStringName, int chapterId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                try
                {
                    // Query for chapter IDs filtered by bookId
                    var chapter = await dbContext.Chapters
                        .Where(ch => ch.Id == chapterId)
                        .Select(ch => new ChapterDto // Use a lambda expression here
                        { 
                            Title = ch.Title,
                            Description = ch.Description
                        })
                        .FirstOrDefaultAsync(); // Fix the method name spelling

                    return chapter;
                }
                catch (Exception ex)
                {
                    // Log exception
                    Console.WriteLine($"Error fetching chapter IDs: {ex.Message}");
                    return null;
                }
            }
        }

        /***************************************************
         * ADD CHAPTERS ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> AddChaptersAsync(string connectionStringName, AddChapterRequest request)
        {
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                int count = 0;
                bool chapterIdExists = false;
                int chapterId;
                do
                {
                    chapterId = int.Parse(CodeGen.GenerateTenDigitId());
                    chapterIdExists = await dbContext.Chapters.AnyAsync(c => c.Id == chapterId);
                    count++;
                }
                while (chapterIdExists && count < 5);

                if (chapterIdExists)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "409 Conflict: Chapter Already Exists"
                        }
                    };
                }

                // Create a new chapter entity
                var newChapter = new ChapterEntity
                {
                    Id = chapterId,
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
                    return (new ApiResponse
                    {
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    return (new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "500 Internal Server Error"
                        }
                    });
                }
            }
        }
    }
}
