using System.ComponentModel.DataAnnotations;
using Tessera.CodeGenerators;

namespace Tessera.Models.WorkspaceComponents.Business
{
    public class AddressEntity
    {
        [Key]
        public string Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public class AddressDto
    {
        [Required(ErrorMessage = "Street is required.")]
        [StringLength(100, ErrorMessage = "Street can't be longer than 100 characters.")]
        public string Street { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(50, ErrorMessage = "City can't be longer than 50 characters.")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required.")]
        [StringLength(2, ErrorMessage = "State should be 2 characters long.")]
        public string State { get; set; }

        [Required(ErrorMessage = "Postal Code is required.")]
        [StringLength(10, ErrorMessage = "Postal Code can't be longer than 10 characters.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(50, ErrorMessage = "Country can't be longer than 50 characters.")]
        public string Country { get; set; }
    }

}
