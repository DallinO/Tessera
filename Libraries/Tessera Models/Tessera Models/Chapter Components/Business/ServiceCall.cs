using System.ComponentModel.DataAnnotations;
using Tessera.Constants;
using Tessera.Models.ChapterComponents;

namespace Tessera.Models.WorkspaceComponents.Business
{
    public class ServiceCall : Leaf
    {
        public string ServiceCallId { get; set; }
        public Customer Customer { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ServiceDate { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public Status Status { get; set; }
        public Priority Priority { get; set; }
    }

    
}
