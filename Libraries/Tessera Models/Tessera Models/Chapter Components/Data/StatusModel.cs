namespace Tessera.Models
{
    public class StatusModel
    {
        public string Name { get; set; }
        public Status Open {  get; set; }
        public List<Status> Status { get; set; } = new List<Status>();
        public Status Closed { get; set; }
    }

    public class Status
    {
        public string Name { get; set; }
        public string Color { get; set; }

    }
}
