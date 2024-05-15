using Gw2Sharp.WebApi.Exceptions;
using Gw2Sharp.WebApi.V2;
using Gw2Sharp.WebApi.V2.Clients;
using Gw2Sharp.WebApi.V2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using File = Gw2Sharp.WebApi.V2.Models.File;
using IOFile = System.IO.File;

#nullable enable

namespace Blish_HUD.LocalDb {
    public interface IDbAccess : IDisposable, IAsyncDisposable {
        IDbCollection<int, Skill> Skills { get; }
        IDbCollection<int, Trait> Traits { get; }
        IDbCollection<int, Specialization> Specializations { get; }
        IDbCollection<int, Skin> Skins { get; }
    }

    internal partial class DbHandler {
        // /v2/achievements
        private readonly Collection<int, Achievement> _achievements;
        public IMetaCollection Achievements => _achievements;

        // /v2/achievements/categories
        private readonly Collection<int, AchievementCategory> _achievementCategories;
        public IMetaCollection AchievementCategories => _achievementCategories;

        // /v2/achievements/groups
        private readonly Collection<string, AchievementGroup> _achievementGroups;
        public IMetaCollection AchievementGroup => _achievementGroups;

        // /v2/backstory/answers
        private readonly Collection<string, BackstoryAnswer> _backstoryAnswers;
        public IMetaCollection BackstoryAnswer => _backstoryAnswers;

        // /v2/backstory/questions
        private readonly Collection<int, BackstoryQuestion> _backstoryQuestions;
        public IMetaCollection BackstoryQuestion => _backstoryQuestions;

        // /v2/colors
        private readonly Collection<int, Color> _colors;
        public IMetaCollection Colors => _colors;

        // /v2/continents
        private readonly Collection<int, Continent> _continents;
        public IMetaCollection Continents => _continents;

        // /v2/currencies
        private readonly Collection<int, Currency> _currencies;
        public IMetaCollection Currencies => _currencies;

        // /v2/dailycrafting
        private readonly Collection<string, DailyCrafting> _dailyCrafting;
        public IMetaCollection DailyCrafting => _dailyCrafting;

        // /v2/dungeons
        private readonly Collection<string, Dungeon> _dungeons;
        public IMetaCollection Dungeons => _dungeons;

        // /v2/emotes
        private readonly Collection<string, Emote> _emotes;
        public IMetaCollection Emotes => _emotes;

        // /v2/files
        private readonly Collection<string, File> _files;
        public IMetaCollection Files => _files;

        // /v2/finishers
        private readonly Collection<int, Finisher> _finishers;
        public IMetaCollection Finishers => _finishers;

        // /v2/gliders
        private readonly Collection<int, Glider> _gliders;
        public IMetaCollection Gliders => _gliders;

        // /v2/guild/permissions
        private readonly Collection<string, GuildPermission> _guildPermissions;
        public IMetaCollection GuildPermissions => _guildPermissions;

        // /v2/guild/upgrades
        private readonly Collection<int, GuildUpgrade> _guildUpgrades;
        public IMetaCollection GuildUpgrades => _guildUpgrades;

        // /v2/home/cats
        private readonly Collection<int, Cat> _homeCats;
        public IMetaCollection HomeCats => _homeCats;

        // /v2/home/nodes
        private readonly Collection<string, Node> _homeNodes;
        public IMetaCollection HomeNodes => _homeNodes;

        // /v2/items
        private readonly Collection<int, Item> _items;
        public IMetaCollection Items => _items;

        // /v2/itemstats
        private readonly Collection<int, Itemstat> _itemStats;
        public IMetaCollection ItemStats => _itemStats;

        // /v2/legendaryarmory
        private readonly Collection<int, LegendaryArmory> _legendaryArmory;
        public IMetaCollection LegendaryArmory => _legendaryArmory;

        // /v2/legends
        private readonly Collection<string, Legend> _legends;
        public IMetaCollection Legends => _legends;

        // /v2/mailcarriers
        private readonly Collection<int, MailCarrier> _mailCarriers;
        public IMetaCollection MailCarriers => _mailCarriers;

        // /v2/mapchests
        private readonly Collection<string, MapChest> _mapChests;
        public IMetaCollection MapChests => _mapChests;

        // /v2/maps
        private readonly Collection<int, Map> _maps;
        public IMetaCollection Maps => _maps;

        // /v2/masteries
        private readonly Collection<int, Mastery> _masteries;
        public IMetaCollection Masteries => _masteries;

        // /v2/materials
        private readonly Collection<int, MaterialCategory> _materials;
        public IMetaCollection Materials => _materials;

        // /v2/minis
        private readonly Collection<int, Mini> _minis;
        public IMetaCollection Minis => _minis;

        // /v2/mounts/skins
        private readonly Collection<int, MountSkin> _mountSkins;
        public IMetaCollection MountSkins => _mountSkins;

        // /v2/mounts/types
        private readonly Collection<string, MountType> _mountTypes;
        public IMetaCollection MountTypes => _mountTypes;

        // /v2/novelties
        private readonly Collection<int, Novelty> _novelties;
        public IMetaCollection Novelties => _novelties;

        // /v2/outfits
        private readonly Collection<int, Outfit> _outfits;
        public IMetaCollection Outfits => _outfits;

        // /v2/pets
        private readonly Collection<int, Pet> _pets;
        public IMetaCollection Pets => _pets;

        // /v2/professions
        private readonly Collection<string, Profession> _professions;
        public IMetaCollection Professions => _professions;

        // /v2/pvp/amulets
        private readonly Collection<int, PvpAmulet> _pvpAmulets;
        public IMetaCollection PvpAmulets => _pvpAmulets;

        // /v2/pvp/heroes
        private readonly Collection<string, PvpHero> _pvpHeroes;
        public IMetaCollection PvpHeroes => _pvpHeroes;

        // /v2/pvp/ranks
        private readonly Collection<int, PvpRank> _pvpRank;
        public IMetaCollection PvpRank => _pvpRank;

        // /v2/quaggans
        private readonly Collection<string, Quaggan> _quaggans;
        public IMetaCollection Quaggans => _quaggans;

        // /v2/quests
        private readonly Collection<int, Quest> _quests;
        public IMetaCollection Quests => _quests;

        // /v2/races
        private readonly Collection<string, Race> _races;
        public IMetaCollection Races => _races;

        // /v2/raids
        private readonly Collection<string, Raid> _raids;
        public IMetaCollection Raids => _raids;

        // /v2/recipes
        private readonly Collection<int, Recipe> _recipes;
        public IMetaCollection Recipes => _recipes;

        // /v2/skills
        private readonly Collection<int, Skill> _skills;
        public IMetaCollection Skills => _skills;

        // /v2/skins
        private readonly Collection<int, Skin> _skins;
        public IMetaCollection Skins => _skins;

        // /v2/specializations
        private readonly Collection<int, Specialization> _specializations;
        public IMetaCollection Specializations => _specializations;

        // /v2/stories
        private readonly Collection<int, Story> _stories;
        public IMetaCollection Stories => _stories;

        // /v2/stories/seasons
        private readonly Collection<string, StorySeason> _storySeasons;
        public IMetaCollection StorySeasons => _storySeasons;

        // /v2/titles
        private readonly Collection<int, Title> _titles;
        public IMetaCollection Titles => _titles;

        // /v2/traits
        private readonly Collection<int, Trait> _traits;
        public IMetaCollection Traits => _traits;

        // /v2/worldbosses
        private readonly Collection<string, WorldBoss> _worldBosses;
        public IMetaCollection WorldBosses => _worldBosses;

        // /v2/wvw/abilities
        private readonly Collection<int, WvwAbility> _wvwAbilities;
        public IMetaCollection WvwAbilities => _wvwAbilities;

        // /v2/wvw/objectives
        private readonly Collection<string, WvwObjective> _wvwObjective;
        public IMetaCollection WvwObjective => _wvwObjective;

        // /v2/wvw/ranks
        private readonly Collection<int, WvwRank> _wvwRank;
        public IMetaCollection WvwRank => _wvwRank;

        // /v2/wvw/upgrades
        private readonly Collection<int, WvwUpgrade> _wvwUpgrade;
        public IMetaCollection WvwUpgrade => _wvwUpgrade;

        // TODO:
        // /v2/jadebots
        // /v2/skiffs
        // /v2/emblem/backgrounds
        // /v2/emblem/foregrounds

        public DbHandler(string metaPath, string dbPath) {
            _metaPath = metaPath;
            _dbPath = dbPath;

            if (IOFile.Exists(metaPath)) {
                _logger.Info("Found cache meta file, loading.");
                try {
                    var meta = JsonConvert.DeserializeObject<Meta>(IOFile.ReadAllText(metaPath));
                    if (meta == null) {
                        _logger.Warn("Cache meta load resulted empty or null.");
                    } else {
                        _meta = meta;
                    }
                } catch (Exception e) {
                    _logger.Warn(e, "Exception when loading cache meta.");
                }
            }

            _metaPath = metaPath;
            _meta ??= new Meta();

            Collection<TId, TItem> addCollection<TId, TItem>(
                string name,
                Func<CancellationToken, Task<IEnumerable<TItem>>> load,
                Func<TItem, TId> keyGetter)
                where TId : notnull
                where TItem : class {
                var collection = new Collection<TId, TItem>(
                    name,
                    _meta.Versions.TryGetValue(name, out var version) ? (Version?)version : null,
                    async ct => (await load(ct)).Select(x => (keyGetter(x), x)));
                _collections[name] = collection;
                return collection;
            }

            async Task<IEnumerable<T>> allPages<T>(IPaginatedClient<T> client, CancellationToken ct)
                where T : IApiV2Object {
                var items = new List<T>();
                for (int i = 0; ; i++) {
                    IApiV2ObjectList<T> page;

                    try {
                        page = await client.PageAsync(i, ct);
                    } catch (PageOutOfRangeException) {
                        break;
                    }

                    if (!page.Any()) {
                        break;
                    }

                    items.AddRange(page);
                }

                return items;
            }

            _achievements = addCollection("Achievement",
                async ct => await allPages(GameService.Gw2WebApi.AnonymousConnection.Client.V2.Achievements, ct),
                (Achievement x) => x.Id);
            _achievementCategories = addCollection("AchievementCategory",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Achievements.Categories.AllAsync(ct),
                (AchievementCategory x) => x.Id);
            _achievementGroups = addCollection("AchievementGroup",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Achievements.Groups.AllAsync(ct),
                (AchievementGroup x) => x.Id.ToString());
            _backstoryAnswers = addCollection("BackstoryAnswer",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Backstory.Answers.AllAsync(ct),
                (BackstoryAnswer x) => x.Id);
            _backstoryQuestions = addCollection("BackstoryQuestion",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Backstory.Questions.AllAsync(ct),
                (BackstoryQuestion x) => x.Id);
            _colors = addCollection("Color",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Colors.AllAsync(ct),
                (Color x) => x.Id);
            _continents = addCollection("Continent",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Continents.AllAsync(ct),
                (Continent x) => x.Id);
            _currencies = addCollection("Currency",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Currencies.AllAsync(ct),
                (Currency x) => x.Id);
            _dailyCrafting = addCollection("DailyCrafting",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.DailyCrafting.AllAsync(ct),
                (DailyCrafting x) => x.Id);
            _dungeons = addCollection("Dungeon",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Dungeons.AllAsync(ct),
                (Dungeon x) => x.Id);
            _emotes = addCollection("Emote",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Emotes.AllAsync(ct),
                (Emote x) => x.Id);
            _files = addCollection("File",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Files.AllAsync(ct),
                (File x) => x.Id);
            _finishers = addCollection("Finisher",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Finishers.AllAsync(ct),
                (Finisher x) => x.Id);
            _gliders = addCollection("Glider",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Gliders.AllAsync(ct),
                (Glider x) => x.Id);
            _guildPermissions = addCollection("GuildPermission",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Guild.Permissions.AllAsync(ct),
                (GuildPermission x) => x.Id);
            _guildUpgrades = addCollection("GuildUpgrade",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Guild.Upgrades.AllAsync(ct),
                (GuildUpgrade x) => x.Id);
            _homeCats = addCollection("Cat",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Home.Cats.AllAsync(ct),
                (Cat x) => x.Id);
            _homeNodes = addCollection("Node",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Home.Nodes.AllAsync(ct),
                (Node x) => x.Id);
            _items = addCollection("Item",
                async ct => await allPages(GameService.Gw2WebApi.AnonymousConnection.Client.V2.Items, ct),
                (Item x) => x.Id);
            _itemStats = addCollection("Itemstat",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Itemstats.AllAsync(ct),
                (Itemstat x) => x.Id);
            _legendaryArmory = addCollection("LegendaryArmory",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.LegendaryArmory.AllAsync(ct),
                (LegendaryArmory x) => x.Id);
            _legends = addCollection("Legend",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Legends.AllAsync(ct),
                (Legend x) => x.Id);
            _mailCarriers = addCollection("MailCarrier",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.MailCarriers.AllAsync(ct),
                (MailCarrier x) => x.Id);
            _mapChests = addCollection("MapChest",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.MapChests.AllAsync(ct),
                (MapChest x) => x.Id);
            _maps = addCollection("Map",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Maps.AllAsync(ct),
                (Map x) => x.Id);
            _masteries = addCollection("Mastery",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Masteries.AllAsync(ct),
                (Mastery x) => x.Id);
            _materials = addCollection("MaterialCategory",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Materials.AllAsync(ct),
                (MaterialCategory x) => x.Id);
            _minis = addCollection("Mini",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Minis.AllAsync(ct),
                (Mini x) => x.Id);
            _mountSkins = addCollection("MountSkin",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Mounts.Skins.AllAsync(ct),
                (MountSkin x) => x.Id);
            _mountTypes = addCollection("MountType",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Mounts.Types.AllAsync(ct),
                (MountType x) => x.Id);
            _novelties = addCollection("Novelty",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Novelties.AllAsync(ct),
                (Novelty x) => x.Id);
            _outfits = addCollection("Outfit",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Outfits.AllAsync(ct),
                (Outfit x) => x.Id);
            _pets = addCollection("Pet",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Pets.AllAsync(ct),
                (Pet x) => x.Id);
            _professions = addCollection("Profession",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Professions.AllAsync(ct),
                (Profession x) => x.Id);
            _pvpAmulets = addCollection("PvpAmulet",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Pvp.Amulets.AllAsync(ct),
                (PvpAmulet x) => x.Id);
            _pvpHeroes = addCollection("PvpHero",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Pvp.Heroes.AllAsync(ct),
                (PvpHero x) => x.Id.ToString());
            _pvpRank = addCollection("PvpRank",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Pvp.Ranks.AllAsync(ct),
                (PvpRank x) => x.Id);
            _quaggans = addCollection("Quaggan",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Quaggans.AllAsync(ct),
                (Quaggan x) => x.Id);
            _quests = addCollection("Quest",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Quests.AllAsync(ct),
                (Quest x) => x.Id);
            _races = addCollection("Race",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Races.AllAsync(ct),
                (Race x) => x.Id);
            _raids = addCollection("Raid",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Raids.AllAsync(ct),
                (Raid x) => x.Id);
            _recipes = addCollection("Recipe",
                async ct => await allPages(GameService.Gw2WebApi.AnonymousConnection.Client.V2.Recipes, ct),
                (Recipe x) => x.Id);
            _skills = addCollection("Skill",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Skills.AllAsync(ct),
                (Skill x) => x.Id);
            _skins = addCollection("Skin",
                async ct => await allPages(GameService.Gw2WebApi.AnonymousConnection.Client.V2.Skins, ct),
                (Skin x) => x.Id);
            _specializations = addCollection("Specialization",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Specializations.AllAsync(ct),
                (Specialization x) => x.Id);
            _stories = addCollection("Story",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Stories.AllAsync(ct),
                (Story x) => x.Id);
            _storySeasons = addCollection("StorySeason",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Stories.Seasons.AllAsync(ct),
                (StorySeason x) => x.Id.ToString());
            _titles = addCollection("Title",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Titles.AllAsync(ct),
                (Title x) => x.Id);
            _traits = addCollection("Trait",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Traits.AllAsync(ct),
                (Trait x) => x.Id);
            _worldBosses = addCollection("WorldBoss",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.WorldBosses.AllAsync(ct),
                (WorldBoss x) => x.Id);
            _wvwAbilities = addCollection("WvwAbility",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Wvw.Abilities.AllAsync(ct),
                (WvwAbility x) => x.Id);
            _wvwObjective = addCollection("WvwObjective",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Wvw.Objectives.AllAsync(ct),
                (WvwObjective x) => x.Id);
            _wvwRank = addCollection("WvwRank",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Wvw.Ranks.AllAsync(ct),
                (WvwRank x) => x.Id);
            _wvwUpgrade = addCollection("WvwUpgrade",
                async ct => await GameService.Gw2WebApi.AnonymousConnection.Client.V2.Wvw.Upgrades.AllAsync(ct),
                (WvwUpgrade x) => x.Id);

            SQLiteContext.Create(_dbPath, _collections.Values);

            GameService.Gw2Mumble.Info.BuildIdChanged += BuildIdChanged;
        }

        internal IMetaCollection? GetCollection(string name)
            => _collections.TryGetValue(name, out var collection) ? (IMetaCollection?)collection : null;

        private class DbAccess : IDbAccess {
            public IDbCollection<int, Skill> Skills { get; }
            public IDbCollection<int, Trait> Traits { get; }
            public IDbCollection<int, Specialization> Specializations { get; }
            public IDbCollection<int, Skin> Skins { get; }

            private readonly SQLiteContext _db;

            public DbAccess(SQLiteContext db, DbHandler local) {
                _db = db;
                Skills = local._skills.Access(_db.Connection);
                Traits = local._traits.Access(_db.Connection);
                Specializations = local._specializations.Access(_db.Connection);
                Skins = local._skins.Access(_db.Connection);
            }

            public void Dispose() => _db.Dispose();
            public ValueTask DisposeAsync() => _db.DisposeAsync();
        }
    }
}
