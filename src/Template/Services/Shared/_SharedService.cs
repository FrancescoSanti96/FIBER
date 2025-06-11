using Template.EntityModel;

namespace Template.Services.Shared
{
    public partial class SharedService
    {
        FiberDbContext _dbContext;

        public SharedService(FiberDbContext dbContext)
        {
            _dbContext = dbContext;
        }
    }
}
