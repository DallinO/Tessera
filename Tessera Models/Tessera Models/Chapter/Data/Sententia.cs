using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera_Models.Chapter.Data
{
    public abstract class SententiaDto
    {
    }

    public class ShortText : SententiaDto
    {
        public string Text {  get; set; }
    }
    
    public class LongText : SententiaDto
    {
        public string Text { get; set; }
    }

}
