using Tessera.Constants;

namespace Tessera.Models
{
    public class ServiceCallDto : LeafDto
    {
        public string ServiceCallId { get; set; }
        public CustomerDto Customer { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ServiceDate { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public Status Status { get; set; }
        public Priority Priority { get; set; }
    }

    
}
