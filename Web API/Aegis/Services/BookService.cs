using Aegis.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tessera.Models.Authentication;
using Tessera.Models.Chapter;
using Tessera.Constants;
using Tessera.CodeGenerators;
using System;
using System.Net;
using Tessera.Models.Chapter.Data;

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
                            ChapterId = chapterId,
                            Content = "<p></p>"
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
                    // Retrieve the document using the DocumentId from the n
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
                // Create a new chapter entity
                var newRow = new RowEntity
                {
                    Name = null,
                    Description = null,
                    Priority = 0,
                    IsComplete = request.Row.IsComplete,
                    Created = request.Row.Created,
                    Due = null,
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
                    string error = ex.ToString();
                    Console.WriteLine(ex.ToString());
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
         * 
         * - 
         ***************************************************/
        public async Task<ApiNotificationResponse> GetNotificationsAsync(string dbName, int bookId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                DateTime end = DateTime.UtcNow.AddHours(1);
                DateTime now = DateTime.UtcNow.AddSeconds(30);
                try
                {
                    var notifications = await dbContext.Notifications
                    .Where(n => n.BookId == bookId 
                        && n.Schedule <= end
                        && n.Schedule >= now)
                    .Select(n => new NotificationDto
                    {
                        Message = n.Message,
                        Schedule = n.Schedule,
                    })
                    .ToListAsync();

                    return new ApiNotificationResponse()
                    {
                        Success = true,
                        Notifications = notifications
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
         * 
         * - 
         ***************************************************/
        public async Task<ApiResponse> AddNotificationAsync(string dbName, NotificationEntity notification)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {

                // Add the new chapter to the context
                dbContext.Notifications.Add(notification);

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
                    string error = ex.ToString();
                    Console.WriteLine(ex.ToString());
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
                        Id = row.Id,
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
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                // Find the existing n by its ID or some other unique identifier
                var existingRow = await dbContext.Rows.FirstOrDefaultAsync(r => r.Id == request.Row.Id);

                // If the n does not exist, return a 404 error
                if (existingRow == null)
                {
                    return new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "Row not found" }
                    };
                }

                // Update properties with values from the notification
                existingRow.Name = request.Row.Name;
                existingRow.Description = request.Row.Description;
                existingRow.Priority = (int)request.Row.Priority;
                existingRow.IsComplete = request.Row.IsComplete;
                existingRow.Due = request.Row.Due;

                // Save changes to the database
                try
                {
                    await dbContext.SaveChangesAsync();
                    return new ApiResponse
                    {
                        Success = true
                    };
                }
                catch (Exception ex)
                {
                    // Log the exception details for debugging purposes
                    Console.WriteLine(ex.ToString());
                    return new ApiResponse
                    {
                        Success = false,
                        Errors = new List<string> { "500 Internal Server Error" }
                    };
                }
            }
        }

        /***************************************************
         * DELETE ROW ASYNC
         * - 
         ***************************************************/
        public async Task<ApiResponse> DeleteRowAsync(string dbName, int chapterId, int rowId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                try
                {
                    var row = await dbContext.Rows
                        .FirstOrDefaultAsync(r => r.Id == rowId && r.ChapterId == chapterId);

                    if (row != null)
                    {
                        dbContext.Rows.Remove(row);
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
         * GET ROWS ASYNC
         * - 
         ***************************************************/
        public async Task<ApiUpcomingTasksResponse> GetUpcomingTasksAsync(string dbName, int bookId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                try
                {
                    var upcomingTasks = await dbContext.Rows
                    .Where(row => row.Chapter.BookId == bookId &&
                                  row.Due.HasValue &&
                                  row.Due.Value <= DateTime.Now.AddDays(3) &&
                                  row.Due.Value >= DateTime.Now)
                    .Select(row => new UpcomingTask
                    {
                        Name = row.Name,
                        Due = row.Due.Value
                    })
                    .ToListAsync();

                    return new ApiUpcomingTasksResponse
                    {
                        Success = true,
                        Tasks = upcomingTasks
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
        * GET ROWS ASYNC
        * - 
        ***************************************************/
        public async Task<ApiUpcomingEventsResponse> GetUpcomingEventsAsync(string dbName, int bookId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                try
                {
                    var upcomingEvents = await dbContext.Events
                    .Where(e => e.BookId == bookId &&
                                  e.Date <= DateOnly.FromDateTime(DateTime.Now.AddDays(3)) &&
                                  e.Date >= DateOnly.FromDateTime(DateTime.Now))
                    .Select(e => new UpcomingEvent
                    {
                        Name = e.Title,
                        Date = e.Date
                    })
                    .ToListAsync();

                    return new ApiUpcomingEventsResponse
                    {
                        Success = true,
                        Events = upcomingEvents
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
        * GET ROWS ASYNC
        * - 
        ***************************************************/
        public async Task<ApiPriorityTasksResponse> GetPriorityTasksAsync(string dbName, int bookId)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                try
                {
                    var upcomingTasks = await dbContext.Rows
                    .Where(row => row.Chapter.BookId == bookId &&
                                  row.Priority == (int)Priority.High)
                    .Select(row => new PriorityTask
                    {
                        Name = row.Name,
                        Priority = (Priority)row.Priority
                    })
                    .ToListAsync();

                    return new ApiPriorityTasksResponse
                    {
                        Success = true,
                        Tasks = upcomingTasks
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
        * GET ROWS ASYNC
        * - 
        ***************************************************/
        public async Task<ApiCalendarResponse> GetDayEventsAsync(string dbName, int bookId, DateOnly date)
        {
            using (var dbContext = _dbFactory.CreateDbContext(dbName))
            {
                try
                {
                    // Fetch events for the given BookId from the Calendar DbSet
                    var events = await dbContext.Events
                    .Where(e => e.BookId == bookId && e.Date == date) 
                    .Select(e => new EventDto
                    {
                        Title = e.Title,
                        Description = e.Description,
                        IsComplete = e.IsComplete,
                        Date = e.Date,
                        Start = e.Start,
                        Finish = e.Finish,
                        EventType = (EventType)e.EventType
                    })
                    .ToListAsync();

                    // Return the response with the fetched events
                    return new ApiCalendarResponse()
                    {
                        Success = true,
                        Events = events
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
