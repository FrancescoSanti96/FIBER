using System.Threading.Tasks;

namespace Template.Services.DataPersister
{
    public interface IDataPersister
    {
        Task SaveOnFileAsync();
    }
}
