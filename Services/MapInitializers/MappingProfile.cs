using AutoMapper;
using Domain.Entites;
using Services.DTO_s.Request;
using Services.DTO_s.Response;

namespace Services.MapInitializers
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<UserCreationRequestDto, AppUser>();
            CreateMap<AppUser, UserResponse>();
        }
    }
}
