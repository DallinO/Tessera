using System.ComponentModel.DataAnnotations;
using Tessera.Constants;

namespace Tessera.Models.Chapter
{
    public class NotificationDto
    {
        public string Message { get; set; }
        public DateTime Schedule { get; set; }
        public bool IsShown { get; set; }
    }

    public class NotificationEntity
    {
        [Key]
        public int Id { get; set; }
        public int BookId { get; set; }
        public int EntityId { get; set; }
        public int EntityType { get; set; }
        public string Message { get; set; }
        public DateTime Schedule { get; set; }
    }


}
