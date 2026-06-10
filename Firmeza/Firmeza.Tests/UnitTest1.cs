using Firmeza.Web.ViewModels.Products;
using System.ComponentModel.DataAnnotations;

namespace Firmeza.Tests;

public class UnitTest1
{
    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void ProductCreate_NegativePrice_FailsValidation()
    {
        var vm = new ProductCreateViewModel
        {
            Name = "Test",
            Unit = "u",
            Price = -1m,
            StockInput = "10"
        };

        var errors = Validate(vm);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(vm.Price)));
    }
}
