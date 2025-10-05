using ChatGPT.API.Features.CreateConversation.Commands;
using FluentValidation;

namespace ChatGPT.API.Features.CreateConversation.Validators;

public class CreateConversationValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Title)
            .MaximumLength(120)
            .When(x => !string.IsNullOrWhiteSpace(x.Title));
    }
}
