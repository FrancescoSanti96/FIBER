using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Template.EntityModel.Models;
using Template.Infrastructure;

namespace Template.Services.Bulletins
{
    public class BollettiniListQuery
    {
        public Expression<Func<Bulletin, bool>> FilterExpression { get; set; }
        public List<int> ColtureFilter { get; set; }
        public List<int> ProvinceFilter { get; set; }
        public Paging Paging { get; set; }
    }

    public class BollettinoDto
    {
        public int Id { get; init; }
        public string AuthorName { get; init; }
        public string AuthorEmail { get; init; }
        public string Titolo { get; init; }
        public string Contenuto { get; init; }
        public DateOnly? Scadenza { get; init; }
        public DateTime? DataPubblicazione { get; init; }
        public bool Pubblicato { get; init; }
        public List<string> Colture { get; init; }
        public List<string> Province { get; init; }

        public BollettinoDto(Bulletin entity)
        {
            Id = entity.Id;
            AuthorName = $"{entity.User.FirstName} {entity.User.LastName}";
            AuthorEmail = entity.User.Email;
            Titolo = entity.Summary;
            Contenuto = entity.Body;
            Scadenza = entity.ExpireDate;
            DataPubblicazione = entity.PublishDate;
            Pubblicato = entity.Published;
            Colture = [.. entity.Coltures.Select(x => x.Name)];
            Province = [.. entity.Provinces.Select(x => x.Name)];
        }
    }

    public partial class BollettiniService
    {
        public async Task<IEnumerable<BollettinoDto>> Query(BollettiniListQuery query)
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

            var bulletins = await querable
                .Include(b => b.User)
                .Include(b => b.Coltures)
                .Include(b => b.Provinces)
                .ApplyPaging(query.Paging)
                .Select(x => new BollettinoDto(x)).ToListAsync();

            return bulletins;
        }
    }
}
