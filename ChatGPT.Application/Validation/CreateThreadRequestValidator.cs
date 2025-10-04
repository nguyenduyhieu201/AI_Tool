using ChatGPT.Application.DTOs;
using FluentValidation;

namespace ChatGPT.Application.Validation;

public class CreateThreadRequestValidator : AbstractValidator<CreateThreadRequest>
{
    public CreateThreadRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserName).NotEmpty();
        RuleFor(x => x.Provider).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Title).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Title));
    }
}
