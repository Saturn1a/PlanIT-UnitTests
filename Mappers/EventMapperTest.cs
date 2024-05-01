using PlanIT.API.Mappers.Interface;
using PlanIT.API.Mappers;
using PlanIT.API.Models.DTOs;
using PlanIT.API.Models.Entities;


namespace PlanITAPI_UnitTests.Mappers;

public class EventMapperTest
{

    private readonly IMapper<Event, EventDTO> _eventMapper = new EventMapper();


    [Fact]
    public void MaptoDTO_EventEntity_ShouldReturnEventDTO()
    {
        // ARRANGE
        Event plannedEvent = new Event()
        {
            Id = 1,
            UserId = 1,
            Name = "Birthday Party",
            Location = "Leos Lekland",
            Date = new DateOnly(2024, 07, 16),
            Time = new TimeOnly(17,30)   

        };
        

        // ACT
        var eventDTO = _eventMapper.MapToDTO(plannedEvent);


        // ASSERT
        Assert.NotNull(eventDTO);
        Assert.Equal(plannedEvent.Id, eventDTO.Id);
        Assert.Equal(plannedEvent.UserId, eventDTO.UserId);
        Assert.Equal(plannedEvent.Name, eventDTO.Name);
        Assert.Equal(plannedEvent.Location, eventDTO.Location);
        Assert.Equal(plannedEvent.Date, eventDTO.Date);
        Assert.Equal(plannedEvent.Time, eventDTO.Time);




    }
}
