using AutoMapper;
using Domain.Entites;
using Services.DTO_s.Request;
using Services.DTO_s.Response;
using Services.Helpers.TransactionHelpers;

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
                .ForMember(x=>x.RecipientUserName, opt=>opt.MapFrom(src=>src.RecipientUserWallet.AppUser.UserName))
                .ForMember(m=>m.SenderUserName, opt =>opt.MapFrom(src=>src.SenderUserWallet.AppUser.UserName));
            CreateMap<UserWallet, WalletResponseDto>()
                .ForMember(u=>u.UserName, opt => opt.MapFrom(src=>src.AppUser.UserName));
            CreateMap<WalletUpdateDto, UserWallet>();
            CreateMap<WalletCreationDto, UserWallet>();
            CreateMap<InitializeTransferResponse, PaymentRecord>().ForMember(p =>p.PublicId, opt=>opt.MapFrom(src =>src.data.Id));
            CreateMap<TransactionCreationRequestDto, Transaction>();
            CreateMap<TransactionUpdateRequestDto, Transaction>();
        }
    }
}
