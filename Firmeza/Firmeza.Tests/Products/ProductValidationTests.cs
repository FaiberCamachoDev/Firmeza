using Firmeza.Web.ViewModels.Products;
using System.ComponentModel.DataAnnotations;

namespace Firmeza.Tests.Products;

public class ProductValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ProductCreate_ValidData_PassesValidation()
    {
        var vm = new ProductCreateViewModel
        {
            Name = "Cemento Portland",
            Unit = "Bolsa 50kg",
            Price = 25000m,
            StockInput = "100",
            Category = "Cementos"
        };

        var errors = Validate(vm);
        Assert.Empty(errors);
    }

    [Fact]
    public void ProductCreate_EmptyName_FailsValidation()
    {
        var vm = new ProductCreateViewModel
        {
            Name = "",
            Unit = "Bolsa",
            Price = 100m,
            StockInput = "10"
        };

        var errors = Validate(vm);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(vm.Name)));
    }

    [Fact]
    public void ProductCreate_ZeroPrice_FailsValidation()
    {
        var vm = new ProductCreateViewModel
        {
            Name = "Producto",
            Unit = "Unidad",
            Price = 0m,
            StockInput = "10"
        };

        var errors = Validate(vm);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(vm.Price)));
    }

    [Theory]
    [InlineData("abc", false)]
    [InlineData("-1", false)]
    [InlineData("0", true)]
    [InlineData("50", true)]
    [InlineData("1000", true)]
    public void StockInput_ParseValidation(string input, bool shouldSucceed)
    {
        bool isValid = int.TryParse(input, out int value) && value >= 0;
        Assert.Equal(shouldSucceed, isValid);
    }

    [Fact]
    public void ProductCreate_NameTooLong_FailsValidation()
    {
        var vm = new ProductCreateViewModel
        {
            Name = new string('A', 151),
            Unit = "Unidad",
            Price = 100m,
            StockInput = "10"
        };

        var errors = Validate(vm);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(vm.Name)));
    }
}
