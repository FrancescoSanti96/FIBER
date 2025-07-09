using System;
using System.Collections.Generic;
using System.Linq;
using Template.Services.Bulletins;
using Template.Web.Utils;

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

        public BollettinoCardViewModel(BulletinDto dto) { 
            Id = dto.Id;
            Titolo = dto.Summary;
            Anteprima = HtmlUtils.StripHtml(dto.Body, maxLength: 50);
            Scadenza = dto.ExpireDate;
            EmailTecnico = dto.AuthorEmail;
            Pubblicato = dto.Published;
            Colture = dto.Coltures;
            Province = dto.Provinces;
        }
    }
}