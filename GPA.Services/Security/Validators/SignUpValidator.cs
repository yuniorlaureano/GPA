using FluentValidation;
using GPA.Data;
using GPA.Dtos.Security;
using Microsoft.EntityFrameworkCore;

namespace GPA.Services.Security.Validators
{
    public class SignUpValidator : AbstractValidator<SignUpDto>
    {
        public SignUpValidator(GPADbContext db)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .NotNull().WithMessage("El nombre es requerido.")
                .MaximumLength(100).WithMessage("El nombre no puede tener más de 100 caracteres.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("El apellido es requerido.")
                .NotNull().WithMessage("El apellido es requerido.")
                .MaximumLength(100).WithMessage("El apellido no puede tener más de 100 caracteres.");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("El nombre de usuario es requerido.")
                .NotNull().WithMessage("El nombre de usuario es requerido.")
                .MaximumLength(30).WithMessage("El nombre de usuario no puede tener más de 30 caracteres.");

            RuleFor(x => x.UserName).MustAsync(async (userName, _) =>
            {
                return !(await db.Users.AnyAsync(x => x.UserName == userName));
            }).WithMessage((dto) => $"El usuario: {dto.UserName}, ya existe");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .NotNull().WithMessage("El email es requerido.")
                .MaximumLength(254).WithMessage("El email no puede tener más de 254 caracteres.");

            RuleFor(x => x.Email).MustAsync(async (email, _) =>
            {
                return !(await db.Users.AnyAsync(x => x.Email == email));
            }).WithMessage((dto) => $"El email: {dto.Email}, ya existe");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .NotNull().WithMessage("La contraseña es requerida");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Confirmar contraseña es requerida")
                .NotNull().WithMessage("Confirmar contraseña es requerida")
                .MaximumLength(128).WithMessage("La contraseña no puede tener más de 128 caracteres");

            RuleFor(x => x.Password).Equal(x => x.ConfirmPassword).WithMessage("Las contraseñas no coinciden");
        }
    }
}
