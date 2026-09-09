using MediatR;

namespace CrateSync.Platform.BuildingBlocks.Application.Commands;

public interface ICommand : IRequest;
public interface ICommand<TResponse> : IRequest<TResponse>;
