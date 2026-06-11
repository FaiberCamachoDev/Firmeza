using System.ComponentModel.DataAnnotations;

namespace Firmeza.Api.DTOs.Customers;

public class CustomerCreateDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string DocumentNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Address { get; set; } = string.Empty;
}
