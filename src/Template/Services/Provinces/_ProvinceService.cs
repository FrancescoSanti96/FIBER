using Template.EntityModel;

namespace Template.Services.Provinces
{
    public partial class ProvinceService(FiberDbContext dbContext)
    {
        private readonly FiberDbContext _dbContext = dbContext;
    }
}
