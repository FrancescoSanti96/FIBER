using System;
using System.Collections.Generic;

namespace Template.Web.Models
{
    public class BollettinoAgricoltoreViewModel
    {
        public int Id { get; set; }
        public string Titolo { get; set; }
        public string ContenutoHTML { get; set; }
        public string AutoreNome { get; set; }
        public string AutoreCognome { get; set; }
        public string AutoreEmail { get; set; }
        public DateTime? DataPubblicazione { get; set; }
        public DateOnly? DataScadenza { get; set; }
        public List<string> Province { get; set; } = [];
        public List<string> Colture { get; set; } = [];
        public bool Pubblicato { get; set; }
        
        // Proprietà calcolate
        public string NomeCompletoAutore => $"{AutoreNome} {AutoreCognome}".Trim();
        public string DataPubblicazioneFormattata => DataPubblicazione?.ToString("dd/MM/yyyy") ?? "";
        public string DataScadenzaFormattata => DataScadenza?.ToString("dd/MM/yyyy") ?? "";
    }
} 