using System;
using Hymnal.Core.Models;

namespace Hymnal.Core.Services
{
    public interface IPreferencesService
    {
        int HymnalsFontSize { get; set; }

        HymnalLanguage ConfiguratedHymnalLanguage { get; set; }
        event EventHandler<HymnalLanguage> HymnalLanguageConfiguratedChanged;

        string LastVersionOpened { get; set; }
    }
}
