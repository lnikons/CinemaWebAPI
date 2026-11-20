using CinemaWebAPI.Dtos;
using FluentValidation;

namespace CinemaWebAPI.Validators
{
    public class ReserveBookingRequestDtoValidator : AbstractValidator<ReserveBookingRequestDto>
    {
        public const string EmailPattern = @"^[\w.\-]+@[\w\-]+(\.[\w\-]+)*\.[A-Za-z]{2,}$";
        public const string EmailMessage = "Email має бути у форматі name@domain.com.";

        public ReserveBookingRequestDtoValidator()
        {
            RuleFor(x => x.MovieSessionId)
                .Matches(ObjectIdRules.Pattern).WithMessage(ObjectIdRules.Message("MovieSessionId"));

            RuleFor(x => x.SeatIds)
                .NotEmpty().WithMessage("Потрібно обрати хоча б одне місце.")
                .Must(ids => ids.Count <= 10).WithMessage("За одне замовлення можна забронювати не більше 10 місць.")
                .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Місця в замовленні не повинні повторюватися.");

            RuleForEach(x => x.SeatIds)
                .Matches(ObjectIdRules.Pattern).WithMessage(ObjectIdRules.Message("SeatIds"));

            RuleFor(x => x.UserEmail)
                .NotEmpty().WithMessage("Поле 'UserEmail' є обов'язковим.")
                .Matches(EmailPattern).WithMessage(EmailMessage);

            RuleForEach(x => x.BarItems).ChildRules(item =>
            {
                item.RuleFor(i => i.Product)
                    .IsInEnum().WithMessage("Такої позиції немає в меню бару.");

                item.RuleFor(i => i.Quantity)
                    .InclusiveBetween(1, 10).WithMessage("Кількість однієї позиції бару має бути від 1 до 10.");
            });
        }
    }

    public class UpdateTicketBookingDtoValidator : AbstractValidator<UpdateTicketBookingDto>
    {
        public UpdateTicketBookingDtoValidator()
        {
            RuleFor(x => x.UserEmail)
                .NotEmpty().WithMessage("Поле 'UserEmail' є обов'язковим.")
                .Matches(ReserveBookingRequestDtoValidator.EmailPattern).WithMessage(ReserveBookingRequestDtoValidator.EmailMessage);
        }
    }
}
