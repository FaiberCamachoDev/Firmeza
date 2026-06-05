using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels.Products;

public class ProductEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad es obligatoria")]
    [MaxLength(50)]
    [Display(Name = "Unidad")]
    public string Unit { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
    [Display(Name = "Precio")]
    public decimal Price { get; set; }

    [Display(Name = "Stock")]
    public string StockInput { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Categoría")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool IsActive { get; set; } = true;
}
