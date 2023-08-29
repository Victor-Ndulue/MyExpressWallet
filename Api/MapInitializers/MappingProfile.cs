using AutoMapper;
using Domain.Entites;
using Services.DTO_s.Request;

namespace Api.MapInitializers
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<UserCreationRequestDto, AppUser>();
        }
    }
}
