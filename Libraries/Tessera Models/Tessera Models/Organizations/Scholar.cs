namespace Tessera.Models
{
    /***************************************************
    * SCHOLAR CLASS
    * - Represents a user who has access to a Book. 
    ***************************************************/
    public class ScholarEntity
    {
        // Unique identifier.
        public int Id { get; set; }
        // Id reference to the Scribe
        public string UserId { get; set; }
        // Reference to the Role
        public int roleId { get; set; }
    }
}
