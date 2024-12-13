using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.Models.Chapter.Data
{
    public class ReportEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Issue { get; set; }
    }

    public class ReportDto
    {
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Issue { get; set; }
    }

}
