using AutoMapper;
using CinemaWebAPI.Documents;
using CinemaWebAPI.Models;

namespace CinemaWebAPI.Mapping
{
    public class DocumentMappingProfile : Profile
    {
        public DocumentMappingProfile()
        {
            CreateMap<MovieSession, MovieSessionDocument>().ReverseMap();
            CreateMap<CinemaHallSeat, CinemaHallSeatDocument>().ReverseMap();
            CreateMap<BarItem, BarItemDocument>().ReverseMap();
            CreateMap<TicketBooking, TicketBookingDocument>().ReverseMap();
            CreateMap<User, UserDocument>().ReverseMap();
        }
    }
}
