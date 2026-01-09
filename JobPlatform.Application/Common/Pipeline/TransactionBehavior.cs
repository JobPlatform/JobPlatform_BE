using JobPlatform.Application.Common.Interfaces;

namespace JobPlatform.Application.Common.Pipeline;

using MediatR;
using Microsoft.EntityFrameworkCore;

public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITransactionalRequest, IRequest<TResponse>
{
    private readonly DbContext _db; // inject AppDbContext

    public TransactionBehavior(DbContext db) => _db = db;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);
            var response = await next();
            await tx.CommitAsync(ct);
            return response;
        });
    }
}
