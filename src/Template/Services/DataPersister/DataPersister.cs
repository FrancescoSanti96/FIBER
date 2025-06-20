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

            // skippo il salvataggio dei seed data (che rimangono sempre gli stessi)

            var usersColtures = await _dbContext.Users
                .SelectMany(u => u.Coltures.Select(c => new
                {
                    IdUser = u.Id,
                    IdColture = c.Id
                }))
                .ToListAsync();

            var usersProvinces = await _dbContext.Users
                .SelectMany(u => u.Provinces.Select(p => new
                {
                    IdUser = u.Id,
                    IdProvince = p.Id
                }))
                .ToListAsync();

            await File.WriteAllTextAsync(Path.Combine(_contextStateFolderPath, "UserColture.json"), JsonSerializer.Serialize(usersColtures));
            await File.WriteAllTextAsync(Path.Combine(_contextStateFolderPath, "UserProvince.json"), JsonSerializer.Serialize(usersProvinces));
        }


    }
}
