using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Template.EntityModel.Models;

namespace Template.Services.Users
{
    public class AddOrUpdateUserCommand
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RoleId { get; set; }
    }

    public partial class UserService
    {
        public async Task Login(int idUser)
        {
            var user = await _dbContext.Users
                .FindAsync(idUser);

            if (user != null)
            {
                user.LastLoggedAt = DateTime.Now;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<int> AddOrUpdate(AddOrUpdateUserCommand cmd)
        {
            var user = await _dbContext.Users
                .Where(x => x.Id == cmd.Id)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                user = new User
                {
                    Id = cmd.Id,
                    Email = cmd.Email,
                    FirstName = cmd.FirstName,
                    LastName = cmd.LastName,
                    RoleId = cmd.RoleId
                };
                _dbContext.Users.Add(user);
            }

            user.FirstName = cmd.FirstName;
            user.LastName = cmd.LastName;

            await _dbContext.SaveChangesAsync();

            return user.Id;
        }

        public async Task<UserDto> SaveUserSettingsAsync(int idUser, IReadOnlyList<int> coltures, IReadOnlyList<int> provinces)
        {
            var user = await _dbContext.Users
                .Where(x => x.Id == idUser)
                .FirstOrDefaultAsync()
                ?? throw new System.Exception($"User with id {idUser} not found");

            var selectedColtures = await _dbContext.Coltures.Where(x => coltures.Contains(x.Id)).ToListAsync();
            var selectedProvinces = await _dbContext.Provinces.Where(x => provinces.Contains(x.Id)).ToListAsync();

            user.Coltures = selectedColtures;
            user.Provinces = selectedProvinces;
            
            // Marca l'onboarding come completato quando l'utente salva le preferenze
            user.OnboardingComplete = true;

            await _dbContext.SaveChangesAsync();

            return new UserDto(user);
        }

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

        public async Task UpdateBulletinAsync(Bulletin bulletin, IEnumerable<int> provinceIds, IEnumerable<int> coltureIds)
        {
            // Recupera il bollettino esistente con le relazioni
            var existingBulletin = await _dbContext.Bulletins
                .Include(b => b.Provinces)
                .Include(b => b.Coltures)
                .FirstOrDefaultAsync(b => b.Id == bulletin.Id);

            if (existingBulletin == null)
            {
                throw new ArgumentException("Bollettino non trovato", nameof(bulletin.Id));
            }

            // Aggiorna i campi del bollettino
            existingBulletin.Summary = bulletin.Summary;
            existingBulletin.Body = bulletin.Body;
            existingBulletin.Published = bulletin.Published;
            existingBulletin.ExpireDate = bulletin.ExpireDate;
            existingBulletin.PublishDate = bulletin.Published ? DateTime.Now : null;

            // Aggiorna le province associate
            existingBulletin.Provinces.Clear();
            var provinces = await _dbContext.Provinces
                .Where(p => provinceIds.Contains(p.Id))
                .ToListAsync();
            foreach (var province in provinces)
            {
                existingBulletin.Provinces.Add(province);
            }

            // Aggiorna le colture associate
            existingBulletin.Coltures.Clear();
            var coltures = await _dbContext.Coltures
                .Where(c => coltureIds.Contains(c.Id))
                .ToListAsync();
            foreach (var colture in coltures)
            {
                existingBulletin.Coltures.Add(colture);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}