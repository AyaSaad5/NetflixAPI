using AutoMapper;
using MoviesAPI.Data;
using MoviesAPI.DTOs;

namespace MoviesAPI.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movie,MovieDetailsDTO>().ReverseMap();
            CreateMap<Movie, MovieDTO>().ReverseMap()
                                        .ForMember(s => s.Poster, opt => opt.Ignore());

        }
    }
}
