using PlanIT.API.Mappers;
using PlanIT.API.Mappers.Interface;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Models.Entities;

namespace PlanITAPI_UnitTests.Mappers;

public class UserMapperTests
{
    private readonly IMapper<User, UserDTO> _userMapper = new UserMapper();


    [Fact]
    public void MaptoDTO_UserEntity_ShouldReturnUserDTO()
    {
        // ARRANGE
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        User user = new User()
        {
            Id = 1,
            Email = "Jane@mail.com",
            Name = "Jane",
            HashedPassword = BCrypt.Net.BCrypt.HashPassword("Jane123!",salt),
            Salt = salt

        };

        // ACT
        var userDTO = _userMapper.MapToDTO(user);


        // ASSERT
        Assert.NotNull(userDTO);
        Assert.Equal(user.Id, userDTO.Id);
        Assert.Equal(user.Name, userDTO.Name);
        Assert.Equal(user.Email, userDTO.Email);
        

    }

    

}