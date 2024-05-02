using PlanIT.API.Mappers;
using PlanIT.API.Mappers.Interface;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanITAPI_UnitTests.Mappers;

public class ToDoMapperTest
{
    private readonly IMapper<ToDo, ToDoDTO> _toDoMapper = new ToDoMapper();

    [Fact]
    public void MaptoDTO_ToDoEntity_ShouldReturnToDoDTO()
    {
        // ARRANGE
        ToDo toDo = new ToDo()
        {
            Id = 1,
            UserId = 1,
            Name = "Wash laundry",
        };
       

        // ACT
        var toDoDTO = _toDoMapper.MapToDTO(toDo);


        // ASSERT
        Assert.NotNull(toDoDTO);
        Assert.Equal(toDo.Id, toDoDTO.Id);
        Assert.Equal(toDo.UserId, toDoDTO.UserId);
        Assert.Equal(toDo.Name, toDoDTO.Name);
        
    }

    [Fact]
    public void MapToModel_ToDoDTO_ShouldReturnToDoEntity()
    {
        // ARRANGE
        ToDoDTO toDoDTO = new ToDoDTO(1, 1, "Wash laundry");

        // ACT
        var toDoEntity = _toDoMapper.MapToModel(toDoDTO);

        // ASSERT
        Assert.NotNull(toDoEntity);
        Assert.Equal(toDoDTO.Id, toDoEntity.Id);
        Assert.Equal(toDoDTO.UserId, toDoEntity.UserId);
        Assert.Equal(toDoDTO.Name, toDoEntity.Name);
    }

}



