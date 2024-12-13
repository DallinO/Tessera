using System.ComponentModel.DataAnnotations;
using Tessera.Constants;

namespace Tessera.Models.Chapter
{
    public class ListDto : ChapterDto
    {
        public List<RowDto> Rows {get; set; } = new List<RowDto>();
    }

    public class RowDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public Priority Priority { get; set; }
        public bool IsComplete { get; set; } = false;
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime? Due { get; set; }
        public int ChapterId { get; set; }

        public RowDto() { }
        public RowDto(RowDto rhs)
        {
            Name = rhs.Name;
            Description = rhs.Description;
            Priority = rhs.Priority;
            IsComplete = rhs.IsComplete;
            Created = rhs.Created;
            Due = rhs.Due;
            ChapterId = rhs.ChapterId;
        }
    }

    public class RowEntity
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Priority { get; set; }
        public bool IsComplete { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Due {  get; set; }
        public int ChapterId { get; set; }

        // Navigation properties
        public ChapterEntity Chapter { get; set; }
    }



    public class DashboardItem
    {
        public string Name { get; set; }
    }

    public class UpcomingTask : DashboardItem
    {
        public DateTime Due { get; set; }
    }

    public class PriorityTask : DashboardItem
    {
        public Priority Priority { get; set; }
    }

    public class UpcomingEvent : DashboardItem
    {
        public DateOnly Date { get; set; }
    }

}
