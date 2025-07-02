using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Template.Web.Areas.Tecnico.ViewModels
{
    public class NuovoBollettinoViewModel
    {
        #region Dati di pre-popolamento
        public List<SelectListItem> OpzioniColture { get; set; }
        public List<SelectListItem> OpzioniProvince { get; set; }
        #endregion

        #region Dati per submit form
        public IReadOnlyList<int> IdColtureSelezionate { get; set; }
        public IReadOnlyList<int> IdProvinceSelezionate { get; set; }
        public string ContenutoBollettino { get; set; }
        public string TitoloBollettino { get; set; } = null;
        public DateOnly Scadenza { get; set; }
        public bool Pubblicato { get; set; }
        #endregion
    }
}
