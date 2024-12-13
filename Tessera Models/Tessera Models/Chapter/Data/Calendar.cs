using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.Constants;

namespace Tessera.Models.Chapter
{
    public class CalendarDay
    {
        public int DayNumber { get; set; }
        public bool IsEmpty => DayNumber == 0;
        public bool IsToday { get; set; }
        public List<EventDto> Events { get; set; } = new List<EventDto>();
    }

    public class EventDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsComplete { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly? Start { get; set; }
        public TimeOnly? Finish { get; set; }
        public EventType EventType { get; set; }
    }

    public class EventEntity
    {
        [Key]
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsComplete { get; set; } = false;
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
        public TimeOnly? Start { get; set; }
        public TimeOnly? Finish { get; set; }
        public int EventType { get; set; }
    }

    public class DisplayEventArgs
    {
        public CalendarDay Day { get; set; }
        public int Index { get; set; }
    }
}
