using System.Collections.Generic;

namespace Template.Web.Areas.Agricoltore.Dto
{
    public class SavePreferencesDto
    {
        public IReadOnlyList<int> Provinces { get; init; }
        public IReadOnlyList<int> Coltures { get; init; }
    }
}
