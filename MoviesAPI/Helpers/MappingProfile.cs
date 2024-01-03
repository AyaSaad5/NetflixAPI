using AutoMapper;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using MoviesAPI.Identity;
using MoviesAPI.Services;

namespace MoviesAPI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movie,MovieDetailsDTO>().ReverseMap();
            CreateMap<Movie, MovieDTO>().ReverseMap()
                                        .ForMember(s => s.Poster, opt => opt.Ignore());

            CreateMap<ApplicationUser,RegisterModel>().ReverseMap();

        }
    }
}
