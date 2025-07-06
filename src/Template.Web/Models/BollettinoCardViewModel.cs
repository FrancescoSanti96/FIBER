using System;
using System.Collections.Generic;
using System.Linq;
using Template.Services.Bulletins;

namespace Template.Web.Models
{
    public class BollettinoCardViewModel
    {
        public int Id { get; init; }
        public string Titolo { get; init; }
        public string Anteprima { get; init; }
        public DateOnly? Scadenza { get; init; }
        public string EmailTecnico { get; init; }
        public bool Pubblicato { get; init; }
        public IReadOnlyList<string> Colture {  get; init; }
        public IReadOnlyList<string> Province {  get; init; }

        public BollettinoCardViewModel() { }

        public BollettinoCardViewModel(BollettinoDto dto) { 
            Id = dto.Id;
            Titolo = dto.Titolo;
            Anteprima = dto.Contenuto;
            Scadenza = dto.Scadenza;
            EmailTecnico = dto.AuthorEmail;
            Pubblicato = dto.Pubblicato;
            Colture = dto.Colture;
            Province = dto.Province;
        }
    }
}