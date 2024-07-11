
using Tessera.Models.ChapterComponents;

namespace Tessera.Models.WorkspaceComponents.Business
{
    public class Customer : Leaf
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int WorkNumber { get; set; }
        public int CellNumber { get; set; }
        public int HomeNumber { get; set; }
        public HousingType HousingType { get; set; }
        public AddressDto Address { get; set; }

    }


    public enum HousingType
    {
        Owner,
        Renter
    }

}
