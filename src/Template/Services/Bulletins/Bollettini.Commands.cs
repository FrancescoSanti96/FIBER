using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template.EntityModel.Models;

namespace Template.Services.Bulletins
{
    public class UpdateBulletinDto
    {
        public int Id { get; set; }
        public int IdUser { get; set; }
        public string Body { get; set; }
        public bool Published { get; set; }
        public List<int> ColtureIds { get; set; }
        public List<int> ProvinceIds { get; set; }
        public DateOnly? ExpireDate { get; set; }
        public string Summary { get; set; }
    }

    public partial class BollettiniService
    {
        public async Task<int> AddNewBulletinAsync(Bulletin bulletin, IEnumerable<int> provinceIds, IEnumerable<int> coltureIds)
        {
            // Associa le province
            var provinces = await _dbContext.Provinces
                .Where(p => provinceIds.Contains(p.Id))
                .ToListAsync();

            // Associa le colture
            var coltures = await _dbContext.Coltures
                .Where(c => coltureIds.Contains(c.Id))
                .ToListAsync();

            bulletin.Provinces = provinces;
            bulletin.Coltures = coltures;
            bulletin.PublishDate = bulletin.Published ? DateTime.Now : null;

            _dbContext.Bulletins.Add(bulletin);
            await _dbContext.SaveChangesAsync();

            return bulletin.Id;
        }

        public async Task<int> UpdateBulletinAsync(UpdateBulletinDto cmd)
        {
            var bulletin = await _dbContext.Bulletins
                .Include(b => b.User)
                .Include(b => b.Coltures)
                .Include(b => b.Provinces)
                .FirstOrDefaultAsync(b => b.Id == cmd.Id)
                ?? throw new InvalidOperationException($"Nessuna corrispondenza trovata nei bollettini per l'id: {cmd.Id}");

            // Associa le province
            var provinces = await _dbContext.Provinces
                .Where(p => cmd.ProvinceIds.Contains(p.Id))
                .ToListAsync();

            // Associa le colture
            var coltures = await _dbContext.Coltures
                .Where(c => cmd.ColtureIds.Contains(c.Id))
                .ToListAsync();

            bulletin.IdUser = cmd.IdUser;
            bulletin.Coltures = coltures;
            bulletin.Provinces = provinces;
            bulletin.ExpireDate = cmd.ExpireDate;
            bulletin.Body = cmd.Body;
            bulletin.Summary = cmd.Summary;
            bulletin.Published = cmd.Published;
            bulletin.PublishDate = bulletin.Published ? DateTime.Now : null;

            await _dbContext.SaveChangesAsync();
            return bulletin.Id;
        }
    }
}
