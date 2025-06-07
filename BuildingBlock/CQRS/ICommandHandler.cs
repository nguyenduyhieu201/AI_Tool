namespace BuildingBlock.CQRS
{
    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit> where TCommand : ICommand<Unit>
    { }
    public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        // The interface does not need to define any additional members,
        // as it inherits from IRequestHandler which already defines the necessary methods.
    }
}
