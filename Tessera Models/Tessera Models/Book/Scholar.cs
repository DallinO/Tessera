using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Book
{
    /***************************************************
    * SCHOLAR CLASS
    * - Represents a user who has access to a Book. 
    ***************************************************/
    public class ScholarEntity
    {
        [Key]
        public string ScribeId { get; set; }
        public int RoleId { get; set; }
    }
}
