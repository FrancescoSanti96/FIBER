using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Template.Web.Areas.Tecnico.ViewModels
{
    public class NuovoBollettinoViewModel
    {
        #region Dati di pre-popolamento
        public List<SelectListItem> OpzioniColture { get; set; }
        public List<SelectListItem> OpzioniProvince { get; set; }
        #endregion

        #region Dati per submit form
        [Required(ErrorMessage = "Selezionare almeno una coltura")]
        [MinLength(1, ErrorMessage = "Selezionare almeno una coltura")]
        public List<int> IdColtureSelezionate { get; set; } = [];

        public List<int> IdProvinceSelezionate { get; set; } = [];

        [Required(ErrorMessage = "Contenuto del bollettino mancante")]
        public string ContenutoBollettino { get; set; }

        public string TitoloBollettino { get; set; } = null;

        [BindRequired]
        [Required(ErrorMessage = "Specificare una data di scadenza")]
        public DateOnly? Scadenza { get; set; } = null;

        public bool Pubblicato { get; set; } = false;
        #endregion
    }
}
