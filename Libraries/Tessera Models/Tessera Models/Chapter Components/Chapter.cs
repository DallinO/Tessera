using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Models.ChapterComponents
{
    public class ChapterEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description{ get; set; }
    }
    
    public class ChapterDto
    {
        public string Title { get; set; }
        public string Description{ get; set; }
        public List<LeafDto> Contents { get; set; } = new List<LeafDto>();
    }


}
