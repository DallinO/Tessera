using Tessera.Models.Chapter;
using Tessera.Models.Chapter.Data;

namespace Tessera.Models.Authentication
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public ApiResponse() { }
        public ApiResponse(List<string> errors)
        {
            Errors = errors;
        }
    }


    public class ApiLoginResponse : ApiResponse
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public AppUserDto AppUser { get; set; }

        public ApiLoginResponse() { }
        public ApiLoginResponse(List<string> errors) : base(errors) { }
    }


    public class ApiBookResponse : ApiResponse
    {
        public int BookId { get; set; }

        public ApiBookResponse() { }
        public ApiBookResponse(List<string> errors) : base(errors) { }
    }

    public class ApiChapterIndex : ApiResponse
    {
        public List<ChapterDto> Chapters { get; set; }
        public ApiChapterIndex() { }
        public ApiChapterIndex(List<string> errors) : base(errors) { }
    }

    public class ApiChapterData : ApiResponse
    {
        public ChapterDto Chapter { get; set; }
        public ApiChapterData() { }
        public ApiChapterData(List<string> errors) : base(errors) { }
    }

    public class ApiDocumentResponse : ApiResponse
    {
        public DocumentDto Document { get; set; }
        public ApiDocumentResponse() { }
        public ApiDocumentResponse(List<string> errors) : base(errors) { }
    }

    public class ApiListResponse : ApiResponse
    {
        public ListDto List { get; set; }
        public ApiListResponse() { }
        public ApiListResponse(List<string> errors) : base(errors) { }
    }

    public class ApiCalendarResponse : ApiResponse
    {
        public List<EventDto> Events { get; set; }
        public ApiCalendarResponse() { }
        public ApiCalendarResponse(List<string> errors) : base(errors) { }
    }

    public class ApiUpcomingTasksResponse : ApiResponse
    {
        public List<UpcomingTask> Tasks { get; set; } = new List<UpcomingTask>();
        public ApiUpcomingTasksResponse() { }
        public ApiUpcomingTasksResponse(List<string> errors) : base(errors) { }
    }

    public class ApiUpcomingEventsResponse : ApiResponse
    {
        public List<UpcomingEvent> Events { get; set; } = new List<UpcomingEvent>();
        public ApiUpcomingEventsResponse() { }
        public ApiUpcomingEventsResponse(List<string> errors) : base(errors) { }
    }

    public class ApiPriorityTasksResponse : ApiResponse
    {
        public List<PriorityTask> Tasks { get; set; } = new List<PriorityTask>();
        public ApiPriorityTasksResponse() { }
        public ApiPriorityTasksResponse(List<string> errors) : base(errors) { }
    }

    public class ApiNotificationResponse : ApiResponse
    {
        public List<NotificationDto> Notifications { get; set; } = new ();
        public ApiNotificationResponse() { }
        public ApiNotificationResponse(List<string> errors) : base(errors) { }
    }

}