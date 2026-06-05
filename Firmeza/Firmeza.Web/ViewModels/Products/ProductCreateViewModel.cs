using System.ComponentModel.DataAnnotations;

namespace Firmeza.Web.ViewModels.Products;

public class ProductCreateViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150, ErrorMessage = "Máximo 150 caracteres")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "La unidad es obligatoria")]
    [MaxLength(50)]
    [Display(Name = "Unidad")]
    public string Unit { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
    [Display(Name = "Precio")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    [Display(Name = "Stock")]
    public string StockInput { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Categoría")]
    public string Category { get; set; } = string.Empty;
}
