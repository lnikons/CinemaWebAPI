using CinemaWebAPI.Dtos;
using FluentValidation;

namespace CinemaWebAPI.Validators
{
    public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
    {
        public RegisterRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Поле 'Email' є обов'язковим.")
                .Matches(ReserveBookingRequestDtoValidator.EmailPattern).WithMessage(ReserveBookingRequestDtoValidator.EmailMessage);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Поле 'FullName' є обов'язковим.")
                .MaximumLength(100).WithMessage("Ім'я не може бути довшим за 100 символів.");

            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("Пароль має містити щонайменше 8 символів.")
                .Matches("[A-Za-z]").WithMessage("Пароль має містити хоча б одну літеру.")
                .Matches("[0-9]").WithMessage("Пароль має містити хоча б одну цифру.");
        }
    }

    public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDtoValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Поле 'Email' є обов'язковим.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Поле 'Password' є обов'язковим.");
        }
    }

    public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenRequestDtoValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty().WithMessage("Поле 'AccessToken' є обов'язковим.");
            RuleFor(x => x.RefreshToken).NotEmpty().WithMessage("Поле 'RefreshToken' є обов'язковим.");
        }
    }

    public class ChangeUserRoleDtoValidator : AbstractValidator<ChangeUserRoleDto>
    {
        public ChangeUserRoleDtoValidator()
        {
            RuleFor(x => x.Role).IsInEnum().WithMessage("Допустимі ролі: User, Admin.");
        }
    }
}
