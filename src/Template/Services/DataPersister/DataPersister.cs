using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Template.EntityModel;
using Template.EntityModel.Models;

namespace Template.Services.DataPersister
{
    public sealed class DataPersister : IDataPersister
    {
        private readonly FiberDbContext _dbContext;
        private readonly string _contextStateFolderPath;

        public DataPersister(FiberDbContext dbContext)
        {
            _dbContext = dbContext;
            _contextStateFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "SolutionItems", "FiberDbState");
        }

        public async Task SaveOnFileAsync()
        {
            if (!Directory.Exists(_contextStateFolderPath))
            {
                throw new DirectoryNotFoundException();
            }

            // Carica gli utenti con le relazioni incluse
            var usersWithRelations = await _dbContext.Users
                .Include(u => u.Coltures)
                .Include(u => u.Provinces)
                .ToListAsync();

            // Salva gli utenti aggiornati (incluso OnboardingComplete)
            var usersToSave = usersWithRelations.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.Password,
                u.RoleId,
                u.LastLoggedAt,
                u.OnboardingComplete
            }).ToList();

            await File.WriteAllTextAsync(Path.Combine(_contextStateFolderPath, "User.json"), JsonSerializer.Serialize(usersToSave, new JsonSerializerOptions { WriteIndented = true }));

            // Salva le relazioni utente-colture
            var usersColtures = usersWithRelations
                .SelectMany(u => u.Coltures.Select(c => new
                {
                    IdUser = u.Id,
                    IdColture = c.Id
                }))
                .ToList();

            // Salva le relazioni utente-province
            var usersProvinces = usersWithRelations
                .SelectMany(u => u.Provinces.Select(p => new
                {
                    IdUser = u.Id,
                    IdProvince = p.Id
                }))
                .ToList();

            await File.WriteAllTextAsync(Path.Combine(_contextStateFolderPath, "UserColture.json"), JsonSerializer.Serialize(usersColtures));
            await File.WriteAllTextAsync(Path.Combine(_contextStateFolderPath, "UserProvince.json"), JsonSerializer.Serialize(usersProvinces));
        }
    }
}
