using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Template.EntityModel.Models;
using Template.Infrastructure;
using Template.Services.Users;

namespace Template.Services.Bulletins
{
    public class BulletinListQuery
    {
        public Expression<Func<Bulletin, bool>> FilterExpression { get; set; }
        public List<int> ColtureFilter { get; set; }
        public List<int> ProvinceFilter { get; set; }
        public Paging Paging { get; set; }
    }

    public class BulletinDto(Bulletin entity)
    {
        public int Id { get; init; } = entity.Id;
        public string AuthorFirstName { get; init; } = entity.User.FirstName;
        public string AuthorLastName { get; init; } = entity.User.LastName;
        public string AuthorEmail { get; init; } = entity.User.Email;
        public string Summary { get; init; } = entity.Summary;
        public string Body { get; init; } = entity.Body;
        public DateOnly? ExpireDate { get; init; } = entity.ExpireDate;
        public DateTime? PublishDate { get; init; } = entity.PublishDate;
        public bool Published { get; init; } = entity.Published;
        public List<string> Coltures { get; init; } = [.. entity.Coltures.Select(x => x.Name)];
        public List<string> Provinces { get; init; } = [.. entity.Provinces.Select(x => x.Name)];
    }

    public class BulletinListDto
    {
        public IEnumerable<BulletinDto> Bulletins {  get; init; }
        public int Count { get; init; }
    }

    public class GetBulletinByIdQuery
    {
        public int Id { get; set; }
    }

    public partial class BollettiniService
    {
        public async Task<BulletinListDto> Query(BulletinListQuery query)
        {
            var querable = _dbContext.Bulletins
                .AsNoTracking();

            if (query.FilterExpression != null)
            {
                querable = querable.Where(query.FilterExpression);
            }

            if (query.ColtureFilter != null && query.ColtureFilter.Count > 0)
            {
                querable = querable.Where(x => x.Coltures.Select(c => c.Id).Any(c => query.ColtureFilter.Contains(c)));
            }
            
            if (query.ProvinceFilter != null && query.ProvinceFilter.Count > 0)
            {
                querable = querable.Where(x => x.Provinces.Select(p => p.Id).Any(p => query.ProvinceFilter.Contains(p)));
            }

            return new BulletinListDto
            {
                Bulletins = await querable
                .Include(b => b.User)
                .Include(b => b.Coltures)
                .Include(b => b.Provinces)
                .ApplyPaging(query.Paging)
                .Select(x => new BulletinDto(x)).ToListAsync(),
                Count = await querable.CountAsync()
            };
        }

        /// <summary>
        /// Returns a specific bulletin with all related data
        /// </summary>
        /// <param name="qry"></param>
        /// <returns></returns>
        public async Task<BulletinDto> Query(GetBulletinByIdQuery qry)
        {
            var bulletin = await _dbContext.Bulletins
                .Include(b => b.User)
                .Include(b => b.Provinces)
                .Include(b => b.Coltures)
                .Where(b => b.Id == qry.Id)
                .FirstOrDefaultAsync();

            if (bulletin == null)
                return null;

            return new BulletinDto(bulletin);
        }
    }
}
