using CinemaWebAPI.Dtos;
using FluentValidation;

namespace CinemaWebAPI.Validators
{
    public class CreateCinemaHallSeatDtoValidator : AbstractValidator<CreateCinemaHallSeatDto>
    {
        public CreateCinemaHallSeatDtoValidator()
        {
            RuleFor(x => x.MovieSessionId)
                .Matches(ObjectIdRules.Pattern).WithMessage(ObjectIdRules.Message("MovieSessionId"));

            RuleFor(x => x.RowNumber)
                .InclusiveBetween(1, 20).WithMessage("Номер ряду має бути від 1 до 20.");

            RuleFor(x => x.SeatNumber)
                .InclusiveBetween(1, 30).WithMessage("Номер місця має бути від 1 до 30.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Невідома категорія місця.");
        }
    }

    public class UpdateCinemaHallSeatDtoValidator : AbstractValidator<UpdateCinemaHallSeatDto>
    {
        public UpdateCinemaHallSeatDtoValidator()
        {
            RuleFor(x => x.RowNumber)
                .InclusiveBetween(1, 20).WithMessage("Номер ряду має бути від 1 до 20.");

            RuleFor(x => x.SeatNumber)
                .InclusiveBetween(1, 30).WithMessage("Номер місця має бути від 1 до 30.");

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Невідома категорія місця.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Невідомий статус місця.");
        }
    }

    public class PatchCinemaHallSeatDtoValidator : AbstractValidator<PatchCinemaHallSeatDto>
    {
        public PatchCinemaHallSeatDtoValidator()
        {
            RuleFor(x => x.RowNumber)
                .InclusiveBetween(1, 20).WithMessage("Номер ряду має бути від 1 до 20.")
                .When(x => x.RowNumber.HasValue);

            RuleFor(x => x.SeatNumber)
                .InclusiveBetween(1, 30).WithMessage("Номер місця має бути від 1 до 30.")
                .When(x => x.SeatNumber.HasValue);

            RuleFor(x => x.Category)
                .IsInEnum().WithMessage("Невідома категорія місця.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Невідомий статус місця.");
        }
    }

    public static class ObjectIdRules
    {
        public const string Pattern = "^[0-9a-fA-F]{24}$";

        public static string Message(string field) => $"Поле '{field}' має бути ідентифікатором MongoDB (24 шістнадцяткові символи).";
    }
}
