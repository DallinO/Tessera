using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Models.ChapterComponents
{
    // Database Entity
    public abstract class Leaf
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string ParentTable { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public abstract class LeafModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
