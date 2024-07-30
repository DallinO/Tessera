namespace Tessera.Models.Chapter
{

    public class CustomerListDto : LeafDto
    {
        public List<CustomerDto> Customers { get; set; }

        public CustomerListDto()
        {
            Name = "Customers";
            Description = "Customer account information.";
            Customers = new List<CustomerDto>();
        }
    }


    public class CustomerDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int WorkNumber { get; set; }
        public int CellNumber { get; set; }
        public int HomeNumber { get; set; }
        public string SelectedContact { get; set; } = "Home"; // Default to "Home"
        public HousingType HousingType { get; set; }
        public AddressDto Address { get; set; }
    }


    public enum HousingType
    {
        Owner,
        Renter
    }

}
