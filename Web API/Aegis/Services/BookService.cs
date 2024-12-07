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
using Azure.Core;

namespace Aegis.Services
{
    public class BookService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly DbContextFactory _dbFactory;

        public BookService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, DbContextFactory dbFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbFactory = dbFactory;
        }


        /***************************************************
         * ADD CHAPTERS ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> AddChapterAsync(string dbName, ApiChapterRequest request)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                // Generate a unique nine digit chapter id. Attempts five time.
                var (chapterSuccess, chapterId) = await GenerateIdAsync<ChapterEntity>(dbContext, 9, async (set, id) => await set.AnyAsync(c => c.Id == id));

                // Check if rowId generation was successful
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
                    Title = request.Chapter.Title,
                    Description = request.Chapter.Description,
                    BookId = request.BookId,
                    Type = (int)request.Chapter.Type
                };

                // Add the new chapter to the context
                dbContext.Chapters.Add(newChapter);

                switch (request.Chapter.Type)
                {
                    case LeafType.Document:
                        // Generate a unique nine digit document id. Attempts five time.
                        var (bookSuccess, bookId) = await GenerateIdAsync<DocumentEntity>(dbContext, 9, async (set, id) => await set.AnyAsync(d => d.Id == id));

                        // Check if rowId generation was successful
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

                    case LeafType.List:
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


        /***************************************************
         * GET CHAPTER LIST ASYNC
         * - 
         ***************************************************/
        public async Task<List<ChapterDto>> GetChaptersAsync(string connectionStringName, int bookId)
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
         * DELETE CHAPTER ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse?> DeleteChapterAsync(string connectionStringName, int bookId, int chapterId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                try
                {
                    var chapterToDelete = await dbContext.Chapters
                        .FirstOrDefaultAsync(c => c.Id == chapterId && c.BookId == bookId);

                    if (chapterToDelete != null)
                    {
                        dbContext.Chapters.Remove(chapterToDelete);
                        await dbContext.SaveChangesAsync();

                        return new ApiResponse
                        { 
                            Success = true
                        };

                    }

                    return null;

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
        public async Task<DocumentDto> GetDocumentAsync(string connectionStringName, int chapterId)
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
         * SAVE DOCUMENT DATA ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> SaveDocumentAsync(string connectionStringName, SaveDocumentRequest request)
        {
            using (var dbContext = _dbFactory.CreateDbContext(connectionStringName))
            {
                try
                {
                    // Retrieve the document using the DocumentId from the row
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
         * GET LIST DATA ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> AddRowAsync(string dbName, ApiRowRequest request)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                // Generate a unique nine digit chapter id. Attempts five time.
                var (chapterSuccess, rowId) = await GenerateIdAsync<ChapterEntity>(dbContext, 4, async (set, id) => await set.AnyAsync(c => c.Id == id));

                // Check if rowId generation was successful
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
                var newRow = new RowEntity
                {
                    Id = rowId,
                    Name = request.Row.Name,
                    Description = request.Row.Description,
                    Priority = (int)request.Row.Priority,
                    IsComplete = request.Row.IsComplete,
                    Created = request.Row.Created,
                    Due = request.Row.Due,
                    ChapterId = request.ChapterId
                };

                // Add the new chapter to the context
                dbContext.Rows.Add(newRow);

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


        /***************************************************
         * GET ROWS ASYNC
         * - 
         ***************************************************/
        public async Task<ApiListResponse> GetRowsAsync(string dbName, int chapterId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                try
                {
                    var rows = await dbContext.Rows
                    .Where(row => row.ChapterId == chapterId) // Filter by ChapterId
                    .Select(row => new RowDto
                    {
                        RowId = row.Id,
                        Name = row.Name,
                        Description = row.Description,
                        Priority = (Priority)row.Priority, // Cast if Priority is an enum
                        IsComplete = row.IsComplete,
                        Created = row.Created,
                        Due = row.Due
                    })
                    .ToListAsync();

                    return new ApiListResponse()
                    {
                        List = new ListDto()
                        {
                            Rows = rows,
                        },
                        Success = true
                    };
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
         * UPDATE ROW ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> UpdateRowAsync(string dbName, ApiRowRequest request)
        {
            return new ApiResponse();
        }

        /***************************************************
         * DELETE ROW ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> DeleteRowAsync(string dbName, int chapterId, int rowId)
        {
            return new ApiResponse();
        }

        /***************************************************
         * GENERATE ID ASYNC
         * - 
         ***************************************************/
        public static async Task<(bool, int)> GenerateIdAsync<T>(DbContext dbContext, int length,Func<DbSet<T>, int, Task<bool>> checkIdExists)
        where T : class
        {
            int count = 0;
            bool idExists = false;
            int id;

            do
            {
                id = int.Parse(CodeGen.GenerateNumberOfLength(length));
                idExists = await checkIdExists(dbContext.Set<T>(), id); // Check if the ID exists in the specified DbSet.
                count++;
            }
            while (idExists && count < 5);

            return idExists ? (false, 0) : (true, id);
        }
    }
}
