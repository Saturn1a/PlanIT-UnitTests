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
            Time = new TimeOnly(18, 00, 00)


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

  
    [Fact]
    public void MapToModel_EventDTO_ShouldReturnEventEntity()
    {
        // ARRANGE
        EventDTO eventDTO = new EventDTO(1, 1, "Birthday Party", new DateOnly(2024, 07, 16), new TimeOnly(18, 00, 00), "Leos Lekland");

        // ACT
        var eventEntity = _eventMapper.MapToModel(eventDTO);

        // ASSERT
        Assert.NotNull(eventEntity);
        Assert.Equal(eventDTO.Id, eventEntity.Id);
        Assert.Equal(eventDTO.UserId, eventEntity.UserId);
        Assert.Equal(eventDTO.Name, eventEntity.Name);
        Assert.Equal(eventDTO.Location, eventEntity.Location);
        Assert.Equal(eventDTO.Date, eventEntity.Date);
        Assert.Equal(eventDTO.Time, eventEntity.Time);
    }
}
