using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Permissions> Permissions { get; set; }

    }
}
