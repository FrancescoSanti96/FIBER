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
                .Include(x => x.Coltures)
                .Include(x => x.Provinces)
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
    }
}