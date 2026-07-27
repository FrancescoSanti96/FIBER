using Template.EntityModel;

namespace Template.Services.Bulletins
{
    public partial class BollettiniService(FiberDbContext dbContext)
    {
        private readonly FiberDbContext _dbContext = dbContext;
    }
}
