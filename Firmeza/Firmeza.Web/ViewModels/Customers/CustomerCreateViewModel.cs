using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels.Customers;

public class CustomerCreateViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [MaxLength(100)]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento es obligatorio")]
    [MaxLength(30)]
    [RegularExpression(@"^[0-9]{6,15}$", ErrorMessage = "Documento: solo números, entre 6 y 15 dígitos")]
    [Display(Name = "Documento")]
    public string DocumentNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    [RegularExpression(@"^[\d\s\+\-\(\)]{7,20}$", ErrorMessage = "Teléfono inválido")]
    [Display(Name = "Teléfono")]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    [EmailAddress(ErrorMessage = "Correo inválido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(300)]
    [Display(Name = "Dirección")]
    public string Address { get; set; } = string.Empty;
}
