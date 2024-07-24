using Aegis.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tessera.Models;
using Microsoft.Data.SqlClient;
using Tessera.Constants;

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

        public async Task InitializeBookDatabase(string dbName)
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


        public async Task<List<ChapterDto>> GetChapters(Guid? bookId)
        {
            string dbName = $"BOOK_{bookId}";
            string connectionString = Keys.SQL_SERVER_ROOT + $"Database = {dbName}; Trusted_Connection = True; Encrypt = False;";

            using (var dbContext = _dbFactory.CreateDbContext(connectionString))
            {
                try
                {
                    var chapters = await dbContext.Chapters.ToListAsync();

                    var chapterDtos = chapters.Select(chapter => new ChapterDto
                    {
                        Title = chapter.Title,
                        Description = chapter.Description,
                        //Contents = new List<LeafDto>()
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
    }
}
