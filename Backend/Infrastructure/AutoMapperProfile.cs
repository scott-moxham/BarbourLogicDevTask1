using AutoMapper;

namespace Backend.Infrastructure;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Models.BookAddDTO, EntityFramework.Book>();
        CreateMap<Models.BookEditDTO, EntityFramework.Book>();
        CreateMap<Models.BookDTO, EntityFramework.Book>()
            .ReverseMap();
        CreateMap<EntityFramework.Book, Models.BookDTOWithUser>();

        CreateMap<Models.UserAddDTO, EntityFramework.User>();
        CreateMap<Models.UserEditDTO, EntityFramework.User>();
        CreateMap<Models.UserDTO, EntityFramework.User>()
            .ReverseMap();
        CreateMap<EntityFramework.User, Models.UserDTOWithBooks>();
    }
}
