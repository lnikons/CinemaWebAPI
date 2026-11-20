using CinemaWebAPI.Dtos;
using FluentValidation;

namespace CinemaWebAPI.Validators
{
    public class CreateMovieSessionDtoValidator : AbstractValidator<CreateMovieSessionDto>
    {
        public CreateMovieSessionDtoValidator()
        {
            RuleFor(x => x.MinAge)
                .NotEmpty().WithMessage("Мінімальний вік для сеансу є обов'язковим.")
                .InclusiveBetween(0, 21).WithMessage("Мінімальний вік для сеансу має бути від 0 до 21 року.");
            RuleFor(x => x.MovieTitle)
                .NotEmpty().WithMessage("Назва фільму є обов'язковою.")
                .Length(2, 100).WithMessage("Назва фільму має містити від 2 до 100 символів.");

            RuleFor(x => x.BasePrice)
                .InclusiveBetween(50, 1000).WithMessage("Базова ціна квитка має бути від 50 до 1000 грн.");

            RuleFor(x => x.HallNumber)
                .InclusiveBetween(1, 10).WithMessage("Номер залу має бути від 1 до 10.");

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(30, 300).WithMessage("Тривалість сеансу має бути від 30 до 300 хвилин.");

            RuleFor(x => x.StartTime)
                .GreaterThan(_ => DateTime.Now).WithMessage("Час початку сеансу має бути в майбутньому.");
        }
    }

    public class UpdateMovieSessionDtoValidator : AbstractValidator<UpdateMovieSessionDto>
    {
        public UpdateMovieSessionDtoValidator()
        {
            RuleFor(x => x.MovieTitle)
                .NotEmpty().WithMessage("Назва фільму є обов'язковою.")
                .Length(2, 100).WithMessage("Назва фільму має містити від 2 до 100 символів.");

            RuleFor(x => x.BasePrice)
                .InclusiveBetween(50, 1000).WithMessage("Базова ціна квитка має бути від 50 до 1000 грн.");

            RuleFor(x => x.HallNumber)
                .InclusiveBetween(1, 10).WithMessage("Номер залу має бути від 1 до 10.");

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(30, 300).WithMessage("Тривалість сеансу має бути від 30 до 300 хвилин.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Невідомий статус сеансу.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Час початку сеансу є обов'язковим.");
        }
    }

    public class PatchMovieSessionDtoValidator : AbstractValidator<PatchMovieSessionDto>
    {
        public PatchMovieSessionDtoValidator()
        {
            RuleFor(x => x.MovieTitle)
                .Length(2, 100).WithMessage("Назва фільму має містити від 2 до 100 символів.")
                .When(x => x.MovieTitle != null);

            RuleFor(x => x.BasePrice)
                .InclusiveBetween(50, 1000).WithMessage("Базова ціна квитка має бути від 50 до 1000 грн.")
                .When(x => x.BasePrice.HasValue);

            RuleFor(x => x.HallNumber)
                .InclusiveBetween(1, 10).WithMessage("Номер залу має бути від 1 до 10.")
                .When(x => x.HallNumber.HasValue);

            RuleFor(x => x.DurationMinutes)
                .InclusiveBetween(30, 300).WithMessage("Тривалість сеансу має бути від 30 до 300 хвилин.")
                .When(x => x.DurationMinutes.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Невідомий статус сеансу.");
        }
    }
}
