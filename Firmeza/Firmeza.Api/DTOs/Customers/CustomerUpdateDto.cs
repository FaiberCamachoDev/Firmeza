using System.ComponentModel.DataAnnotations;

namespace Firmeza.Api.DTOs.Customers;

public class CustomerUpdateDto
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    public bool? IsActive { get; set; }
}
