using System;
using System.Collections.Generic;
using System.Text;

namespace CrateSync.Platform.TradingPartners.Application.Abstractions
{
    public interface ITradingPartnersUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
