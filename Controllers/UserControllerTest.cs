using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PlanIT.API.Controllers;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Services.Interfaces;

namespace PlanITAPI_UnitTests.Controllers;

public class UserControllerTest
{
    private readonly UsersController _usersController;
    private readonly Mock<IUserService> _userServiceMOCK = new Mock<IUserService>();
    private readonly ILogger<UsersController> _loggerMock;

    public UserControllerTest()
    {
        _loggerMock = new Mock<ILogger<UsersController>>().Object;
        _usersController = new UsersController(_userServiceMOCK.Object, _loggerMock);
    }


    [Fact]
    public async Task GetUserById_ShouldReturn_UserDTO_WhenIdGiven()
    {
        // ARRANGE
        var userId = 1;
        var userDTO = new UserDTO(userId, "Jane", "Jane.Doe@email.com");
        _userServiceMOCK.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(userDTO);


        // ACT 
        var res = await _usersController.GetUsersByIdASync(userId);



        // ASSERT 
        var actionResult = Assert.IsType<ActionResult<UserDTO>>(res);
        var returnValue = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<UserDTO>(returnValue.Value);

        Assert.NotNull(userDTO);
        Assert.Equal(dto.Id, userDTO.Id);
        Assert.Equal(dto.Name, userDTO.Name);
        Assert.Equal(dto.Email, userDTO.Email);


    }


    [Fact]
    public async Task GetAllUsers_ShouldReturn_UserDTOs()
    {
        // ARRANGE
        List<UserDTO> dtos = new() 
        {
        new UserDTO(1, "Jane", "Jane.Doe@email.com"),
        new UserDTO(2, "John", "John.Doe@email.com"),
        new UserDTO(3, "Jack", "Jack.Doe@email.com"),
        new UserDTO(4, "Jessica", "Jessica.Doe@email.com")
        };


        _userServiceMOCK.Setup(x => x.GetAllAsync(1, 10)).ReturnsAsync(dtos);


        // ACT 
        var res = await _usersController.GetUsersAsync(1,10);



        // ASSERT 
        var actionResult = Assert.IsType<ActionResult<IEnumerable<UserDTO>>>(res);
        var returnValue = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos_result = Assert.IsType<List<UserDTO>>(returnValue.Value);

        Assert.Equal(4, dtos.Count());

        // Test for user 1
        var userWithId1 = dtos_result.FirstOrDefault(dto => dto.Id == 1);
        Assert.NotNull(userWithId1);
        Assert.Equal("Jane", userWithId1.Name);
        Assert.Equal("Jane.Doe@email.com", userWithId1.Email);

        // Test for user 2
        var userWithId2 = dtos_result.FirstOrDefault(dto => dto.Id == 2);
        Assert.NotNull(userWithId2); 
        Assert.Equal("John", userWithId2.Name);
        Assert.Equal("John.Doe@email.com", userWithId2.Email);

        // Test for user 3
        var userWithId3 = dtos_result.FirstOrDefault(dto => dto.Id == 3);
        Assert.NotNull(userWithId3);
        Assert.Equal("Jack", userWithId3.Name);
        Assert.Equal("Jack.Doe@email.com", userWithId3.Email);

        // Test for user 4
        var userWithId4 = dtos_result.FirstOrDefault(dto => dto.Id == 4);
        Assert.NotNull(userWithId4);
        Assert.Equal("Jessica", userWithId4.Name);
        Assert.Equal("Jessica.Doe@email.com", userWithId4.Email);


    }
}