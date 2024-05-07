using PlanIT.API.Mappers.Interface;
using PlanIT.API.Mappers;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Models.Entities;


namespace PlanITAPI_UnitTests.Mappers;

public class DinnerMapperDTO
{

    private readonly IMapper<Dinner, DinnerDTO> _dinnerMapper = new DinnerMapper();


    [Fact]
    public void MaptoDTO_EventEntity_ShouldReturnEventDTO()
    {
        // ARRANGE
        Dinner dinner = new Dinner()
        {
            Id = 1,
            UserId = 1,
            Name = "Pizza",
            Date = new DateOnly(2024, 04, 10)

        };
      

        // ACT
        var dinnerDTO = _dinnerMapper.MapToDTO(dinner);


        // ASSERT
        Assert.NotNull(dinnerDTO);
        Assert.Equal(dinner.Id, dinnerDTO.Id);
        Assert.Equal(dinner.UserId, dinnerDTO.UserId);
        Assert.Equal(dinner.Name, dinnerDTO.Name);
        Assert.Equal(dinner.Date, dinnerDTO.Date);
       

    }



}
