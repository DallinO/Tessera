using Aegis.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Tessera.Models.Authentication;
using Tessera.Models.Chapter;
using Microsoft.Data.SqlClient;
using Tessera.Constants;
using Tessera.Models.Book;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Tessera.CodeGenerators;
using System.Net;
using System.Reflection.Metadata;
using Microsoft.Identity.Client;
using System.Linq.Expressions;

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
         * GET CHAPTER LIST ASYNC
         * - 
         ***************************************************/
        public async Task<List<ChapterDto>> GetChapterListAsync(string connectionStringName, int bookId)
        {
            // Create a DbContext instance using the provided connection string
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                try
                {
                    // Query for chapters filtered by bookId.
                    var chapters = await dbContext.Chapters
                        .Where(chapter => chapter.BookId == bookId)
                        .ToListAsync();

                    // Convert to DTOs.
                    var chapterDtos = chapters.Select(chapter => new ChapterDto
                    {
                        Title = chapter.Title,
                        ChapterId = chapter.Id,
                        Description = chapter.Description,
                        Type = (LeafType)chapter.Type
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
         * GET CHAPTER DATA ASYNC
         * - 
         ***************************************************/
        public async Task<DocumentDto> GetDocumentDataAsync(string connectionStringName, int bookId, int chapterId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                try
                {
                    var document = await dbContext.Chapters
                    .Where(c => c.Id == chapterId)
                    .Join(
                        dbContext.Documents,
                        c => c.Id,          
                        d => d.ChapterId,   
                        (c, d) => new DocumentDto
                        {
                            DocumentId = d.Id,
                            Title = c.Title,
                            Description = c.Description,
                            Content = d.Content
                        })
                    .FirstOrDefaultAsync();

                    return document;

                }
                catch (Exception ex)
                {
                    // Log exception
                    Console.WriteLine($"Error fetching document data: {ex.Message}");
                    return null;
                }
            }
        }

        /***************************************************
         * SAVE CHAPTER DATA ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> SaveDocumentDataAsync(string connectionStringName, SaveDocumentRequest request)
        {
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                try
                {
                    // Retrieve the document using the DocumentId from the request
                    var document = await dbContext.Documents
                        .FirstOrDefaultAsync(d => d.Id == request.Document.DocumentId);

                    if (document == null)
                    {
                        return new ApiResponse
                        {
                            Success = false,
                            Errors = new List<string>
                            {
                                "404: Document not found."
                            }
                        };
                    }

                    // Update the content of the document
                    document.Content = request.Document.Content;

                    // Save the changes to the database
                    await dbContext.SaveChangesAsync();
                    return new ApiResponse
                    {
                        Success = true
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            $"500: Error saving document data: {ex.Message}"
                        }
                    };
                }
            }
        }



        /***************************************************
         * ADD CHAPTERS ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> AddChaptersAsync(string dbName, AddChapterRequest request)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                // Generate a unique nine digit chapter id. Attempts five time.
                var (chapterSuccess, chapterId) = await GenerateId<ChapterEntity>(dbContext, async (set, id) => await set.AnyAsync(c => c.Id == id));

                // Check if chapterId generation was successful
                if (!chapterSuccess)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string>
                        {
                            "409 Conflict: ID already exists"
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

                switch (request.Type)
                {
                    case LeafType.d:
                        // Generate a unique nine digit document id. Attempts five time.
                        var (bookSuccess, bookId) = await GenerateId<DocumentEntity>(dbContext, async (set, id) => await set.AnyAsync(d => d.Id == id));

                        // Check if chapterId generation was successful
                        if (!bookSuccess)
                        {
                            return new ApiResponse
                            {
                                Success = false,
                                Errors = new List<string>
                                {
                                    "409 Conflict: ID already exists"
                                }
                            };
                        }

                        // Create new document entity.
                        var newDocument = new DocumentEntity
                        {
                            Id = bookId,
                            ChapterId = chapterId
                        };

                        dbContext.Documents.Add(newDocument);
                        break;

                    default:
                        return (new ApiResponse
                        {
                            Success = false,
                            Errors = new List<string>()
                            {
                                "400 Type Property Is Null"
                            }
                        });
                }


                // Save changes to the database
                try
                {
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

        public static async Task<(bool, int)> GenerateId<T>(DbContext dbContext, Func<DbSet<T>, int, Task<bool>> checkIdExists)
        where T : class
        {
            int count = 0;
            bool idExists = false;
            int id;

            do
            {
                id = int.Parse(CodeGen.GenerateNineDigitId());
                idExists = await checkIdExists(dbContext.Set<T>(), id); // Check if the ID exists in the specified DbSet.
                count++;
            }
            while (idExists && count < 5);

            return idExists ? (false, 0) : (true, id);
        }
    }
}
