using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template.EntityModel.Models;

namespace Template.Services.Provinces
{
    public partial class ProvinceService
    {
        /// <summary>
        /// Restituisce la lista di tutte le province
        /// </summary>
        /// <returns></returns>
        public async Task<List<Province>> Query()
            => await _dbContext.Provinces
                .AsNoTracking()
                .ToListAsync();
    }
}
