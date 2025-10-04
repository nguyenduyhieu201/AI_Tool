using ChatGPT.API.Features.StreamChat.Commands;
using FluentValidation;

namespace ChatGPT.API.Features.StreamChat.Validators;

public class StreamChatValidator : AbstractValidator<StreamChatCommand>
{
    public StreamChatValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(8000);
        // ConversationId optional
    }
}
