using AutoMapper;
using Firmeza.Api.DTOs.Customers;
using Firmeza.Api.DTOs.Products;
using Firmeza.Api.DTOs.Sales;
using Firmeza.Web.Models;

namespace Firmeza.Api.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Products
        CreateMap<Product, ProductDto>();
        CreateMap<ProductCreateDto, Product>();
        // ProductUpdateDto uses nullable fields — applied manually in controller

        // Customers
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerCreateDto, Customer>();

        // Sales
        CreateMap<Sale, SaleDto>()
            .ForMember(d => d.CustomerName,
                o => o.MapFrom(s => $"{s.Customer.FirstName} {s.Customer.LastName}"))
            .ForMember(d => d.CustomerDocument,
                o => o.MapFrom(s => s.Customer.DocumentNumber));

        CreateMap<SaleDetail, SaleDetailDto>()
            .ForMember(d => d.ProductName,  o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.Subtotal,     o => o.MapFrom(s => s.Quantity * s.UnitPrice));
    }
}
