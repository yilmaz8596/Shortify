
using AutoMapper;
using Shortify.Client.Data.ViewModels;
using Shortify.Data.Models;


namespace Shortify.Client.Data.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Url, GetUrlVM>().ReverseMap();
            CreateMap<AppUser, GetUserVM>().ReverseMap();
        }
    }
}
