using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera_Models.Chapter.Data
{
    public class StatusModel
    {
        public string Name { get; set; }
        public StatusDto Open { get; set; }
        public List<StatusDto> Status { get; set; } = new List<StatusDto>();
        public StatusDto Closed { get; set; }
    }


    public class StatusDto
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }


    public class StatusCollection
    {
        private readonly HashSet<string> _statuses;

        public StatusCollection()
        {
            _statuses = new HashSet<string>() { "TO DO", "COMPLETED"};
        }

        public void AddStatus(string status)
        {
            _statuses.Add(status);
        }

        public void RemoveStatus(string status)
        {
            _statuses.Remove(status);
        }

        public bool Contains(string status) => _statuses.Contains(status);

        public IEnumerable<string> GetStatuses() => _statuses.AsEnumerable();
    }

    public class StatusCollectionEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        // Navigation properties
        public ICollection<StatusCollectionStatuses> StatusCollectionStatuses { get; set; }
        public ICollection<StatusEntity> Statuses { get; set; }
    }

    public class StatusEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation properties
        public ICollection<StatusCollectionStatuses> StatusCollectionStatuses { get; set; }
        public ICollection<StatusCollectionEntity> StatusCollections { get; set; }
    }

    public class StatusCollectionStatuses
    {
        [Key]
        public int StatusCollectionId { get; set; }
        [Key]
        public int StatusId { get; set; }

        // Navigation properties
        public StatusCollectionEntity StatusCollection {  get; set; }
        public StatusEntity Status { get; set; }
    }
}
