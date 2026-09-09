using MediatR;

namespace CrateSync.Platform.BuildingBlocks.Application.Queries;

public interface IQuery<TResponse> : IRequest<TResponse>;
