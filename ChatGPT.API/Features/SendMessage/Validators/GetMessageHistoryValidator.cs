using ChatGPT.API.Features.SendMessage.Queries;
using FluentValidation;

namespace ChatGPT.API.Features.SendMessage.Validators;

public class GetMessageHistoryValidator : AbstractValidator<GetMessageHistoryQuery>
{
    public GetMessageHistoryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.ConversationId)
            .NotEmpty();

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(500);
    }
}
