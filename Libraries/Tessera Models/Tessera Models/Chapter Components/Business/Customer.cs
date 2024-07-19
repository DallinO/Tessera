
using Tessera.Models.ChapterComponents;

namespace Tessera.Models.ChapterComponents
{

    public class CustomerListDto : LeafDto
    {
        public List<CustomerDto> Customers { get; set; }

        public CustomerListDto()
        {
            Name = "Customers";
            Description = "Customer account information.";
        }
    }


    public class CustomerDto
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
