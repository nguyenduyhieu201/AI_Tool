using ChatGPT.Application.DTOs;
using FluentValidation;

namespace ChatGPT.Application.Validation;

public class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Content).NotEmpty().MaximumLength(8000);
    }
}
