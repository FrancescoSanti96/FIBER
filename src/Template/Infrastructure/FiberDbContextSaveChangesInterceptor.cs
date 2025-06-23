using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Template.Services.DataPersister;

namespace Template.Infrastructure
{
    public sealed class FiberDbContextSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _provider;

        public FiberDbContextSaveChangesInterceptor(IServiceProvider provider)
        {
            _provider = provider;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
#if DEBUG
            using var scope = _provider.CreateScope();
            var dataPersister = scope.ServiceProvider.GetRequiredService<IDataPersister>();
            await dataPersister.SaveOnFileAsync();
#endif
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
