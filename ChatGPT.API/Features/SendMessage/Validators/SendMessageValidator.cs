using ChatGPT.API.Features.SendMessage.Commands;
using FluentValidation;

namespace ChatGPT.API.Features.SendMessage.Validators;

public class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(8000);

        // ConversationId is optional; no rule needed unless you want specific format
    }
}
