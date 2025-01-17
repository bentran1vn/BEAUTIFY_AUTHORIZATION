using AutoMapper;

namespace BEAUTIFY_AUTHORIZATION.APPLICATION.Mapper;

public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        // V1
        // CreateMap<Product, Response.ProductResponse>().ReverseMap();
        // CreateMap<PagedResult<Product>, PagedResult<Response.ProductResponse>>().ReverseMap();

        //// V2
        //CreateMap<Product, Contract.Services.V2.Product.Response.ProductResponse>().ReverseMap();
    }
}
