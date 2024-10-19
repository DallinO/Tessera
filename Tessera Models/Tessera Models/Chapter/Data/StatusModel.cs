namespace Tessera.Models.Chapter
{
    public class StatusModel
    {
        public string Name { get; set; }
        public StatusDto Open {  get; set; }
        public List<StatusDto> Status { get; set; } = new List<StatusDto>();
        public StatusDto Closed { get; set; }
    }

    public class StatusDto
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }
}
