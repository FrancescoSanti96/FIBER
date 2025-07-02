using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template.EntityModel.Models;

namespace Template.Services.Coltures
{
    public partial class ColtureService
    {
        /// <summary>
        /// Restituisce la lista di tutte le colture
        /// </summary>
        /// <returns></returns>
        public async Task<List<Colture>> Query()
            => await _dbContext.Coltures
                .AsNoTracking()
                .ToListAsync();
    }
}
