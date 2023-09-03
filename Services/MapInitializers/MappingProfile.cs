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
            CreateMap<UserUpdateRequestDto, AppUser>(); 
            CreateMap<Transaction, TransactionResponseDto>()
                .ForMember(r=>r.RecipientUserName, opt => opt.MapFrom(src=>src.RecipientUserWallet.AppUser.UserName))
                .ForMember(s=>s.SenderUserName, opt => opt.MapFrom(src => src.SenderUserWallet.AppUser.UserName));
        }
    }
}
