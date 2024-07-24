namespace Tessera.Models
{
    public class Catalog
    {
        public string ScribeId { get; set; }
        public Scribe Scribe { get; set; }
        public Guid BookId { get; set; }
        public Preface Preface { get; set; }
        public bool IsOwner { get; set; }
    }
}
