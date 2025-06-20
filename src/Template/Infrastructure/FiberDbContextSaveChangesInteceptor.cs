using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Template.Services.DataPersister;

namespace Template.Infrastructure
{
    public sealed class FiberDbContextSaveChangesInteceptor : SaveChangesInterceptor
    {
        private readonly IDataPersister _dataPersister;

        public FiberDbContextSaveChangesInteceptor(IDataPersister dataPersister)
        {
            _dataPersister = dataPersister;
        }

        public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
        {
#if DEBUG
            await _dataPersister.SaveOnFileAsync();
#endif
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
