using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hymnal.Core.Extensions;
using Hymnal.Core.Models;
using Hymnal.StorageModels.Models;
using Microsoft.AppCenter.Crashes;
using MvvmCross.Logging;
using Newtonsoft.Json;

namespace Hymnal.Core.Services
{
    public class HymnsService : IHymnsService
    {
        private readonly IFilesService filesService;
        private readonly IMvxLog log;

        /// <summary>
        /// <see cref="Hymn"/> cache
        /// </summary>
        private static readonly Dictionary<string, IEnumerable<Hymn>> HymnsDictionary = new Dictionary<string, IEnumerable<Hymn>>();

        /// <summary>
        /// <see cref="Thematic"/> cache
        /// </summary>
        private static readonly Dictionary<string, IEnumerable<Thematic>> ThematicDictionary = new Dictionary<string, IEnumerable<Thematic>>();

        public HymnsService(
            IFilesService filesService,
            IMvxLog log
            )
        {
            this.filesService = filesService;
            this.log = log;
        }

        /// <summary>
        /// Get list of hymns
        /// Here is where you read all the hymns
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Hymn>> GetHymnListAsync(HymnalLanguage language)
        {
            if (!HymnsDictionary.ContainsKey(language.Id))
            {
                try
                {
                    var file = await filesService.ReadFileAsync(language.HymnsFileName).ConfigureAwait(false);
                    List<Hymn> hymns = JsonConvert.DeserializeObject<List<Hymn>>(file);

                    // Set Id of the language to know allways where it is from
                    hymns.ForEach(h => h.HymnalLanguageId = language.Id);

                    lock (HymnsDictionary)
                    {
                        if (!HymnsDictionary.ContainsKey(language.Id))
                            HymnsDictionary.Add(language.Id, hymns);
                    }
                }
                catch (Exception ex)
                {
                    var properties = new Dictionary<string, string>()
                    {
                        { "File", nameof(HymnsService) },
                        { "Hymnal Version", language.Id }
                    };

                    log.TraceException("Exception reading hymnbook", ex, properties);
                    Crashes.TrackError(ex, properties);

                    return new List<Hymn>();
                }
            }

            return HymnsDictionary[language.Id];
        }

        public async Task<Hymn> GetHymnAsync(int number, HymnalLanguage language)
        {
            IEnumerable<Hymn> hymns = await GetHymnListAsync(language).ConfigureAwait(false);

            return hymns.First(h => h.Number == number);
        }

        /// <summary>
        /// Get Hymn from a storage reference of a hymn
        /// </summary>
        /// <param name="hymnReference"></param>
        /// <returns></returns>
        public Task<Hymn> GetHymnAsync(IHymnReference hymnReference)
        {
            return GetHymnAsync(hymnReference.Number, HymnalLanguage.GetHymnalLanguageWithId(hymnReference.HymnalLanguageId));
        }

        public async Task<IEnumerable<Thematic>> GetThematicListAsync(HymnalLanguage language)
        {
            if (!language.SupportThematicList)
            {
                return new List<Thematic>();
            }

            if (!ThematicDictionary.ContainsKey(language.Id))
            {
                var file = await filesService.ReadFileAsync(language.ThematicHymnsFileName).ConfigureAwait(false);
                List<Thematic> thematicList = JsonConvert.DeserializeObject<List<Thematic>>(file);

                lock (ThematicDictionary)
                {
                    if (!ThematicDictionary.ContainsKey(language.Id))
                        ThematicDictionary.Add(language.Id, thematicList);
                }
            }

            return ThematicDictionary[language.Id];
        }
    }
}
