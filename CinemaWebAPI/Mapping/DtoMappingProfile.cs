using AutoMapper;
using CinemaWebAPI.Dtos;
using CinemaWebAPI.Models;

namespace CinemaWebAPI.Mapping
{
    public class DtoMappingProfile : Profile
    {
        public DtoMappingProfile()
        {
            CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<decimal?, decimal>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<DateTime?, DateTime>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<MovieSessionStatus?, MovieSessionStatus>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<SeatCategory?, SeatCategory>().ConvertUsing((src, dest) => src ?? dest);
            CreateMap<SeatStatus?, SeatStatus>().ConvertUsing((src, dest) => src ?? dest);

            CreateMap<MovieSession, MovieSessionResponseDto>();
            CreateMap<CreateMovieSessionDto, MovieSession>();
            CreateMap<UpdateMovieSessionDto, MovieSession>();
            CreateMap<PatchMovieSessionDto, MovieSession>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CinemaHallSeat, CinemaHallSeatResponseDto>();
            CreateMap<CreateCinemaHallSeatDto, CinemaHallSeat>();
            CreateMap<UpdateCinemaHallSeatDto, CinemaHallSeat>();
            CreateMap<PatchCinemaHallSeatDto, CinemaHallSeat>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<BarItem, BarItemResponseDto>();
            CreateMap<BarItemRequestDto, BarItem>();
            CreateMap<TicketBooking, TicketBookingResponseDto>();
            CreateMap<ReserveBookingRequestDto, TicketBooking>();
            CreateMap<UpdateTicketBookingDto, TicketBooking>();

            CreateMap<User, UserResponseDto>();
            CreateMap<RegisterRequestDto, User>();
        }
    }
}
