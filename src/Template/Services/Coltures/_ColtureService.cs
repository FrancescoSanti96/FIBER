using Template.EntityModel;

namespace Template.Services.Coltures
{
    public partial class ColtureService(FiberDbContext dbContext)
    {
        private readonly FiberDbContext _dbContext = dbContext;
    }
}
