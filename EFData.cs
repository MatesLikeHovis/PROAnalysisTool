using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.IO;
using System.Security.Policy;
using WPFBMI.Models;
using System.Threading.Channels;
using System.Windows.Controls.Primitives;
using Microsoft.VisualBasic;
using Microsoft.Extensions.Options;

namespace WPFBMI
{
    internal class EFData
    {
        public static void ApplyMigrations()
        {
            using var context = new ApplicationDbContext();

            context.Database.EnsureCreated();
            context.Database.Migrate();
        }

        public class ApplicationDbContext : DbContext
        {
            public DbSet<EFStatement> Statements { get; set; }
            public DbSet<EFChannel> Channels { get; set; }
            public DbSet<EFShow> Shows { get; set; }
            public DbSet<EFTrack> Tracks { get; set; }
            public DbSet<EFUsage> Usages { get; set; }
            public DbSet<RootTrack> RootTracks { get; set; }
            public DbSet<Genre> Genres { get; set; }
            public DbSet<Subgenre> Subgenres { get; set; }
            public DbSet<Library> Libraries { get; set; }
            public DbSet<Album> Albums { get; set; }
            public DbSet<EFCountry> Countries {  get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                // Specify SQLite file location
                string debugPath = @"D:\VisualStudioProjects\WPFBMI\WPFBMI\Data\dataset.db";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "dataset.db");
                optionsBuilder.UseSqlite($"Data Source={debugPath}");
            }
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // Configure the relationships between EFUsage and other entities

                modelBuilder.Entity<EFTrack>()
                    .HasOne(u => u.root_track)
                    .WithMany()
                    .HasForeignKey(u => u.root_id)
                    .OnDelete(DeleteBehavior.Cascade);

                // EFUsage -> EFTrack (Many-to-One)
                modelBuilder.Entity<EFUsage>()
                    .HasOne(u => u.track)               // EFUsage has one EFTrack
                    .WithMany()                          // EFTrack has many EFUsages
                    .HasForeignKey(u => u.track_id)      // Foreign key in EFUsage is track_id
                    .OnDelete(DeleteBehavior.Cascade);   // Define how to handle deletions (e.g., cascade delete)

                // EFUsage -> EFChannel (Many-to-One)
                modelBuilder.Entity<EFUsage>()
                    .HasOne(u => u.channel)             // EFUsage has one EFChannel
                    .WithMany()                          // EFChannel has many EFUsages
                    .HasForeignKey(u => u.channel_id)    // Foreign key in EFUsage is channel_id
                    .OnDelete(DeleteBehavior.Cascade);   // Define how to handle deletions

                // EFUsage -> EFShow (Many-to-One)
                modelBuilder.Entity<EFUsage>()
                    .HasOne(u => u.show)                // EFUsage has one EFShow
                    .WithMany()                          // EFShow has many EFUsages
                    .HasForeignKey(u => u.show_id)       // Foreign key in EFUsage is show_id
                    .OnDelete(DeleteBehavior.Cascade);   // Define how to handle deletions

                // EFUsage -> EFStatement (Many-to-One)
                modelBuilder.Entity<EFUsage>()
                    .HasOne(u => u.statement)           // EFUsage has one EFStatement
                    .WithMany()                          // EFStatement has many EFUsages
                    .HasForeignKey(u => u.statement_id)  // Foreign key in EFUsage is statement_id
                    .OnDelete(DeleteBehavior.Cascade);   // Define how to handle deletions

                modelBuilder.Entity<EFUsage>()
                    .HasOne(u => u.country)
                    .WithMany()
                    .HasForeignKey(u => u.country_id)
                    .OnDelete(DeleteBehavior.Cascade);
            }

        }

        public static async Task DeleteDatabase()
        {
            string debugPath = @"D:\VisualStudioProjects\WPFBMI\WPFBMI\Data\dataset.db";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "dataset.db");
            if (File.Exists(debugPath))
            {
                File.Delete(debugPath);
            }
        }

        public static async Task<int> CountUsages()
        {
            var db = new ApplicationDbContext();
            return db.Usages.Count();
        }

        public static async Task<int> CountCountries()
        {
            var db = new ApplicationDbContext();
            return db.Countries.Count();
        }
        public static async Task<int> CountStatements()
        {
            var db = new ApplicationDbContext();
            return db.Statements.Count();
        }

        public static async Task<int> CountLibraries()
        {
            var db = new ApplicationDbContext();
            return db.Libraries.Count();
        }

        public static async Task<int> CountGenres()
        {
            var db = new ApplicationDbContext();
            return db.Genres.Count();
        }

        public static async Task<int> CountAlbums()
        {
            var db = new ApplicationDbContext();
            return db.Albums.Count();
        }

        public static async Task<int> CountSubgenres()
        {
            var db = new ApplicationDbContext();
            return db.Subgenres.Count();
        }

        public static async Task<int> CountTracks()
        {
            var db = new ApplicationDbContext();
            return db.Tracks.Count();
        }

        public static async Task<int> CountRootTracks()
        {
            var db = new ApplicationDbContext();
            return db.RootTracks.Count();
        }

        public static async Task<List<EFUsage>> GetUsageQuery(List<string> types, List<string> filters)
        {
            StringBuilder query = new();
            for (int i = 0; i < types.Count; i++)
            {
                string type = types[i];
                string filter = filters[i];
                StringBuilder thisFilter = new();
                switch (type)
                {
                    case "RT":
                        thisFilter.Append("track.root_track.trackName == ");
                        break;
                    case "GN":
                        thisFilter.Append("track.root_track.genre.name == ");
                        break;
                    case "SG":
                        thisFilter.Append("track.root_track.subgenre.name == ");
                        break;
                    case "LI":
                        thisFilter.Append("track.root_track.library.name == ");
                        break;
                    case "AL":
                        thisFilter.Append("track.root_track.album.name == ");
                        break;
                    case "CH":
                        thisFilter.Append("channel.source_name == ");
                        break;
                }
                thisFilter.Append($"\"{ filter}\"");
                query.Append(thisFilter.ToString());
                if (i != types.Count - 1) { query.Append(" && "); }
            }
            string queryString = query.ToString();
            using (var context = new ApplicationDbContext())
            {
                // Example: "RootTrackName == \"Example\"" or "GenreName == \"Test\""
                var response = context.Usages.Where(queryString)
                    .Include(t => t.statement);

                return await response.ToListAsync();
            }
        }

        public static async Task<RootTrack> GetRootTrackFromName(string name)
        {
            using var context = new ApplicationDbContext();
            RootTrack track = context.RootTracks
                .Include(t => t.genre)
                .Include(t => t.library)
                .Include(t => t.subgenre)
                .Include(t => t.album)
                .FirstOrDefault(s => s.trackName == name);
            return track;
        }

        public static async Task<int> SaveEntrySet(List<StatementEntry> entries)
        {
            List<EFUsage> usages = new();
            List<EFChannel> channels = new();
            List<EFShow> shows = new();
            List<EFStatement> statements = new();
            List<EFTrack> tracks = new();
            List<EFCountry> countries = new();
            Dictionary<EFUsage, int> usageChannels = new();
            Dictionary<EFUsage, int> usageShows = new();
            Dictionary<EFUsage, int> usageStatements = new();
            Dictionary<EFUsage, int> usageTracks = new();
            Dictionary<EFUsage, int> usageCountries = new();
            Dictionary<EFChannel, int> channelIDs = new();
            Dictionary<EFShow, int> showIDS = new();
            Dictionary<EFStatement, int> statementIDs = new();
            Dictionary<EFTrack, int> trackIDs = new();
            Dictionary<EFCountry, int> countryIDs = new();
            Dictionary<EFStatement, bool> preexistingStatements = new();
            Dictionary<EFStatement, bool> allowDuplication = new();
            foreach (var entry in entries)
            {
                var usage = new EFUsage();
                usage.perf_period = entry.perf_period;
                int chindex = channels.FindIndex(s => s.source_name == entry.perf_source);
                if (chindex != -1) { usageChannels[usage] = chindex; } else
                {
                    var channel = new EFChannel();
                    channel.source_name = entry.perf_source;
                    channels.Add(channel);
                    usageChannels[usage] = channels.Count - 1;
                }
                int shindex = shows.FindIndex(s => s.show_name == entry.show_name);
                if (shindex != -1) { usageShows[usage] = shindex; }
                else
                {
                    var show = new EFShow();
                    show.show_name = entry.show_name;
                    show.channel = channels.FirstOrDefault(s => s.source_name == entry.perf_source);
                    shows.Add(show);
                    usageShows[usage] = shows.Count - 1;
                }
                int stindex = statements.FindIndex(s => s.period == entry.statement_period);
                if (stindex != -1) { usageStatements[usage] = stindex; }
                else
                {
                    var statement = new EFStatement();
                    statement.period = entry.statement_period;
                    statement.name = entry.statement_period;
                    statement.participant = entry.participant;
                    statement.participant_num = entry.participant_num;
                    statements.Add(statement);
                    usageStatements[usage] = statements.Count - 1;
                }
                int trindex = tracks.FindIndex(s => s.title_name == entry.title_name);
                if (trindex != -1) { usageTracks[usage] = trindex; }
                else
                {
                    var track = new EFTrack();
                    track.title_name = entry.title_name;
                    track.title_num = entry.title_num;
                    tracks.Add(track);
                    usageTracks[usage] = tracks.Count - 1;
                }
                int cntindex = countries.FindIndex(s => s.country_name == entry.perf_country);
                if (cntindex != -1) { usageCountries[usage] = cntindex; }
                else
                {
                    var country = new EFCountry();
                    country.country_name = entry.perf_country;
                    countries.Add(country);
                    usageCountries[usage] = countries.Count - 1;
                }
                usage.bonus_level = entry.bonus_level;
                usage.theme_bonus = entry.theme_bonus;
                usage.timing_secs = entry.timing_secs;
                usage.hits_bonus = entry.hits_bonus;
                usage.current_activity = entry.current_activity;
                usage.episode_name = entry.episode_name;
                usage.foreign_adjustment = entry.foreign_adj;
                usage.percent = entry.percent;
                usage.perf_counts = entry.perf_counts;
                usage.royalty_amount = entry.royalty_amount;
                usage.use_code = entry.use_code;
                usages.Add(usage);
            }

            var db = new ApplicationDbContext();
            for (int i = 0; i < statements.Count; i++)
            {
                EFStatement realStatement = db.Statements.FirstOrDefault(s => s.period == statements[i].period);
                if (realStatement == null) { db.Statements.Add(statements[i]); preexistingStatements[statements[i]] = false; }
                else
                {
                    preexistingStatements[realStatement] = true;
                    var result = MessageBox.Show($"Existing Statement Found: {realStatement.period}\nAllow Duplication?\nThis is usually a very bad idea", "Warning - Statement Found", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes) { allowDuplication[realStatement] = true; } else { allowDuplication[realStatement] = false; }
                    statements[i] = db.Statements.FirstOrDefault(s => s.period == statements[i].period);
                }
            }
            for (int i = 0; i < channels.Count; i++)
            {
                EFChannel realChannel = db.Channels.FirstOrDefault(s=>s.source_name == channels[i].source_name);
                if (realChannel == null) { db.Channels.Add(channels[i]); } else {
                    channels[i] = db.Channels.FirstOrDefault(s => s.source_name == channels[i].source_name); }
            }
            for (int i = 0; i < shows.Count; i++)
            {
                EFShow realShow = db.Shows.FirstOrDefault(s => s.show_name == shows[i].show_name);
                if (realShow == null) { db.Shows.Add(shows[i]); } else {
                    shows[i] = db.Shows.FirstOrDefault(s => s.show_name == shows[i].show_name); }
            }
            for (int i = 0; i < tracks.Count; i++)
            {
                EFTrack realTrack = db.Tracks.FirstOrDefault(s => s.title_name == tracks[i].title_name);
                if (realTrack == null) { db.Tracks.Add(tracks[i]); } else {
                    tracks[i] = db.Tracks.FirstOrDefault(s => s.title_name == tracks[i].title_name); }
            }
            for (int i = 0; i < countries.Count; i++)
            {
                EFCountry realCountry = db.Countries.FirstOrDefault(s => s.country_name == countries[i].country_name);
                if (realCountry == null) { db.Countries.Add(countries[i]); } else {
                    countries[i] = db.Countries.FirstOrDefault(s => s.country_name == countries[i].country_name); }
            }
            int current = await db.SaveChangesAsync();
            for (int i = 0; i < usages.Count; i++)
            {
                var usage = usages[i];
                usage.channel = channels[usageChannels[usage]];
                usage.channel_id = usage.channel.id;
                usage.show = shows[usageShows[usage]];
                usage.show_id = usage.show.id;
                usage.statement = statements[usageStatements[usage]];
                usage.statement_id = usage.statement.id;
                usage.track = tracks[usageTracks[usage]];
                usage.track_id = usage.track.id;
                usage.country = countries[usageCountries[usage]];
                usage.country_id = usage.country.id;
                usages[i] = usage;
                if (!preexistingStatements.ContainsKey(usage.statement) || !preexistingStatements[usage.statement])
                {
                    db.Usages.Add(usage);
                }
                else if (preexistingStatements.ContainsKey(usage.statement) && preexistingStatements[usage.statement])
                {
                    if (allowDuplication.ContainsKey(usage.statement) && allowDuplication[usage.statement])
                    {
                        db.Usages.Add(usage);
                    }
                }
            }
            int val = await db.SaveChangesAsync();
            return val + current;
        }

        public static async Task<int> SaveEntrySetOld(List<StatementEntry> entries)
        {
            int updatedAmount = 0;
            var context = new ApplicationDbContext();
            foreach (var entry in entries)
            {
                int val = await SaveIndividualStatementEntry(entry, context);
                updatedAmount+=val;
            }
            return updatedAmount;
        }

        public static async Task<int> SaveIndividualStatementEntry(StatementEntry entry, ApplicationDbContext context)
        {
            var usage = new EFUsage();
            usage.perf_period = entry.perf_period;
            var channel = await context.Channels.FirstOrDefaultAsync(c => c.source_name == entry.perf_source);
            if (channel == null)
            {
                channel = new EFChannel();
                channel.source_name = entry.perf_source;
                channel.source_country = entry.perf_country;
                context.Channels.Add(channel);
                await context.SaveChangesAsync();
                channel = await context.Channels.FirstOrDefaultAsync(c => c.source_name == entry.perf_source);
            }
            usage.channel = channel;
            usage.channel_id = channel.id;
            var show = await context.Shows.FirstOrDefaultAsync(c => c.show_name == entry.show_name);
            if (show == null)
            {
                show = new EFShow();
                show.show_name = entry.show_name;
                show.show_num = entry.show_num;
                show.channel = channel;
                show.channel_id = channel.id;
                context.Shows.Add(show);
                await context.SaveChangesAsync();
                show = await context.Shows.FirstOrDefaultAsync(c => c.show_name == entry.show_name);
            }
            usage.show = show;
            usage.show_id = show.id;
            var track = await context.Tracks.FirstOrDefaultAsync(c => c.title_num == entry.title_num);
            if ( track == null)
            {
                track = new EFTrack();
                track.title_name = entry.title_name;
                track.title_num = entry.title_num;
                track.root_id = null;
                track.root_track = null;
                context.Tracks.Add(track);
                await context.SaveChangesAsync();
                track = await context.Tracks.FirstOrDefaultAsync(c => c.title_num == entry.title_num);
            }
            usage.track = track;
            usage.track_id = track.id;
            var statement = await context.Statements.FirstOrDefaultAsync(c => c.period == entry.statement_period);
            if ( statement == null )
            {
                statement = new();
                statement.participant = entry.participant;
                statement.participant_num = entry.participant_num;
                statement.period = entry.statement_period;
                statement.name = entry.statement_period;
                context.Statements.Add(statement);
                await context.SaveChangesAsync();
                statement = await context.Statements.FirstOrDefaultAsync(c => c.name == entry.statement_period);
            }
            usage.statement = statement;
            usage.bonus_level = entry.bonus_level;
            usage.theme_bonus = entry.theme_bonus;
            usage.timing_secs = entry.timing_secs;
            usage.hits_bonus = entry.hits_bonus;
            usage.current_activity = entry.current_activity;
            usage.episode_name = entry.episode_name;
            usage.foreign_adjustment = entry.foreign_adj;
            usage.percent = entry.percent;
            usage.perf_counts = entry.perf_counts;
            usage.royalty_amount = entry.royalty_amount;
            usage.use_code = entry.use_code;
            context.Usages.Add(usage);
            int amt = await context.SaveChangesAsync();
            return amt;
        }

        public static async Task<EFTrack> GetTrackFromID(int id)
        {
            using var context = new ApplicationDbContext();
            EFTrack track = context.Tracks.FirstOrDefault(s=>s.id == id);
            return track;
        }

        public static async Task<RootTrack> GetRootTrackFromID(int id)
        {
            using var context = new ApplicationDbContext();
            RootTrack track = context.RootTracks
                .Include(t => t.genre)
                .Include(t => t.library)
                .Include(t => t.subgenre)
                .Include(t => t.album)
                .FirstOrDefault(s=>s.id == id);
            return track;
        }

        public static async Task<List<EFTrack>> GetTrackList()
        {
            using var context = new ApplicationDbContext();
            List<EFTrack> tracks = context.Tracks
                .Include(t => t.root_track)
                .ToList();
            return tracks;
        }

        public static async Task<List<RootTrack>> GetRootTrackList()
        {
            using var context = new ApplicationDbContext();
            List<RootTrack> tracks = context.RootTracks
                .Include(t => t.genre)
                .Include(t => t.library)
                .Include(t => t.subgenre)
                .Include(t => t.album)
                .ToList();
            return tracks;
        }

        public static async Task<List<Genre>> GetGenresList()
        {
            using var context = new ApplicationDbContext();
            List<Genre> genres = context.Genres.ToList();
            return genres;
        }

        public static async Task<List<Subgenre>> GetSubgenresList()
        {
            using var context = new ApplicationDbContext();
            List<Subgenre> subgenres = context.Subgenres.ToList();
            return subgenres;
        }

        public static async Task<List<Library>> GetLibrariesList()
        {
            using var context = new ApplicationDbContext();
            List<Library> libraries = context.Libraries.ToList();
            return libraries;
        }

        public static async Task<List<Album>> GetAlbumsList()
        {
            using var context = new ApplicationDbContext();
            List<Album> albums = context.Albums.ToList();
            return albums;
        }
        public static async Task<List<EFChannel>> GetChannelsList()
        {
            using var context = new ApplicationDbContext();
            var items = context.Channels.ToList();
            return items;
        }

        public static async Task<List<EFShow>> GetShowsList()
        {
            using var context = new ApplicationDbContext();
            var items = context.Shows
                .Include(t => t.channel)
                .ToList();
            return items;
        }

        public static async Task<List<EFStatement>> GetStatementsList()
        {
            using var context = new ApplicationDbContext();
            var items = context.Statements.ToList();
            return items;
        }

        public static async Task<List<EFUsage>> GetUsagesList()
        {
            using var context = new ApplicationDbContext();
            var items = context.Usages
                .Include(t => t.channel)
                .Include(t => t.show.channel)
                .Include(t => t.show)
                .Include(t => t.statement)
                .Include(t => t.track)
                .Include(t => t.country)
                .Include(t => t.track.root_track)
                .Include(t => t.track.root_track.library)
                .Include(t => t.track.root_track.album)
                .Include(t => t.track.root_track.genre)
                .Include(t => t.track.root_track.subgenre)
                .ToList();
            return items;
        }

        public static async Task<int> AutoCreateRootTracks()
        {
            List<EFTrack> tracks = await GetTrackList();
            List<RootTrack> rootTracks = await GetRootTrackList();
            List<EFTrack> unassignedTracks = new();
            List<EFTrack> updatedTracks = new();
            foreach (var track in tracks)
            {
                if (track.root_track == null)
                {
                    unassignedTracks.Add(track);
                }
            }

            foreach (var track in unassignedTracks)
            {
                string trackLetters = track.title_name.Replace(" ", "").ToLower();
                int lowestDistance = Int32.MaxValue;
                RootTrack bestTrack = null;
                foreach (RootTrack rootTrack in rootTracks)
                {
                    int thisDistance = LevenshteinDistance(trackLetters, rootTrack.trackName.Replace(" ","").ToLower());
                    if (thisDistance < lowestDistance)
                    {
                        lowestDistance = thisDistance;
                        bestTrack = rootTrack;
                    }
                }
                if (lowestDistance < trackLetters.Length/2)
                {
                    track.root_track = bestTrack;
                    track.root_id = bestTrack.id;
                    updatedTracks.Add(track);
                }
            }
            var context = new ApplicationDbContext();
            foreach (var track in updatedTracks)
            {
                context.Tracks.Update(track);
            }
            await context.SaveChangesAsync();
            return updatedTracks.Count;
        }

        public static int LevenshteinDistance(string s1, string s2)
        {
            int[,] dp = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                dp[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
                }
            }
            return dp[s1.Length, s2.Length];
        }

        public static async Task RemoveAllRootAssignments()
        {
            List<EFTrack> tracks = await GetTrackList();
            var context = new ApplicationDbContext();
            foreach (var track in tracks)
            {
                track.root_id = null;
                track.root_track = null;
                context.Tracks.Update(track);
            }
            await context.SaveChangesAsync();
        }

        public static async Task CreateAndAssignNewRootTrack(EFTrack track, RootTrack rootTrack)
        {
            using var context = new ApplicationDbContext();
            if (rootTrack.genre != null)
            {
                var existingGenre = await context.Genres
                    .FirstOrDefaultAsync(g => g.id == rootTrack.genre.id);
                if (existingGenre != null)
                {
                    rootTrack.genre = existingGenre;
                }
                else
                {
                    context.Genres.Add(rootTrack.genre);
                    await context.SaveChangesAsync();
                }
                rootTrack.genre_id = rootTrack.genre.id;
            }
            if (rootTrack.subgenre != null)
            {
                var existingSubGenre = await context.Subgenres
                    .FirstOrDefaultAsync(g => g.id == rootTrack.subgenre.id);

                if (existingSubGenre != null)
                {
                    rootTrack.subgenre = existingSubGenre;
                }
                else
                {
                    context.Subgenres.Add(rootTrack.subgenre);
                    await context.SaveChangesAsync();
                }
                rootTrack.subgenre_id = rootTrack.subgenre.id;
            }
            if (rootTrack.library != null)
            {
                var existingLibrary = await context.Libraries
                    .FirstOrDefaultAsync(g => g.id == rootTrack.library.id);
                if (existingLibrary != null)
                {
                    rootTrack.library = existingLibrary;
                } else
                {
                    context.Libraries.Add(rootTrack.library);
                    await context.SaveChangesAsync();
                }
                rootTrack.library_id = rootTrack.library.id;
            }
            if (rootTrack.album != null)
            {
                var existingAlbum = await context.Albums.FirstOrDefaultAsync(g => g.id == rootTrack.album.id);
                if (existingAlbum != null)
                {
                    rootTrack.album = existingAlbum;
                } else
                {
                    context.Albums.Add(rootTrack.album);
                    await context.SaveChangesAsync();
                }
            }
            // Add the RootTrack to the database
            context.RootTracks.Add(rootTrack);
            await context.SaveChangesAsync();

            // Retrieve the new RootTrack ID
            int newId = context.RootTracks
                .FirstOrDefault(s => s.trackName == rootTrack.trackName)?.id ?? 0;

            // Make sure the ID was retrieved successfully
            if (newId > 0)
            {
                // Update the track with the new root_id
                track.root_id = newId;
                track.root_track = rootTrack;
                context.Tracks.Update(track);
                await context.SaveChangesAsync();
            }
            else
            {
                // Handle case where the RootTrack was not found or not saved properly
                throw new InvalidOperationException("The RootTrack could not be found after being added.");
            }
        }
        public static async Task<int> SaveTrackLists(List<RootTrack> tracks, List<EFTrack> subtracks)
        {
            int changes = 0;
            using var context = new ApplicationDbContext();
            foreach (RootTrack track in tracks)
            {
                RootTrack DBTrack = context.RootTracks.Where(s => s.id == track.id).FirstOrDefault();
                bool changed = false;
                if (DBTrack.subgenre != track.subgenre) { DBTrack.subgenre = track.subgenre; changed = true; }
                if (DBTrack.genre != track.genre) { DBTrack.genre = track.genre; changed = true; }
                if (DBTrack.library != track.library) { DBTrack.library = track.library; changed = true; }
                if (changed) { changes++; }
            }
            foreach (EFTrack subtrack in subtracks)
            {
                EFTrack track = context.Tracks.Where(s => s.id == subtrack.id).FirstOrDefault();
                bool changed = false;
                if (track.root_id != subtrack.root_id) { track.root_id = subtrack.root_id; changed = true; }
                if (changed) { changes++; }
            }
            await context.SaveChangesAsync();
            return changes;
        }

        public static async Task UpdateTrack(EFTrack track)
        {
            if (track == null) { return; }
            using var context = new ApplicationDbContext();
            context.Tracks.Update(track);
            await context.SaveChangesAsync();
        }
        public static async Task UpdateRootTrack(RootTrack track)
        {
            if (track == null) { return; }
            using var context = new ApplicationDbContext();
            context.RootTracks.Update(track);
            await context.SaveChangesAsync();
        }
        public static async Task UpdateShow(EFShow show)
        {
            if (show == null) { return; }
            using var context = new ApplicationDbContext();
            context.Shows.Update(show);
            await context.SaveChangesAsync();
        }
        public static async Task UpdateUsage(EFUsage usage)
        {
            if (usage == null) { return; }
            using var context = new ApplicationDbContext();
            context.Usages.Update(usage);
            await context.SaveChangesAsync();
        }

        public static async Task InsertRootTrack(RootTrack track)
        {
            if (track == null) { return; }
            using var context = new ApplicationDbContext();
            if (track.genre != null)
            {
                context.Genres.Attach(track.genre);
            }
            if (track.album != null)
            {
                context.Albums.Attach(track.album);
            }
            if (track.subgenre != null)
            {
                context.Subgenres.Attach(track.subgenre);
            }
            if (track.library != null)
            {
                context.Libraries.Attach(track.library);
            }
            context.RootTracks.Add(track);
            await context.SaveChangesAsync();
        }

        public static async Task<Album> AddAlbumRequest(string albumName)
        {
            using var context = new ApplicationDbContext();
            Album oldAlbum = context.Albums.FirstOrDefault(a => a.name == albumName);
            if (oldAlbum == null)
            {
                Album newAlbum = new();
                newAlbum.name = albumName;
                context.Albums.Add(newAlbum);
                context.SaveChanges();
                Album albumReturn = context.Albums.FirstOrDefault(a => a.name == newAlbum.name);
                return albumReturn;
            }
            MessageBox.Show("Notice - this Album name already exists.", "Existing Name Warning");
            return null;
        }

        public static async Task<Genre> AddGenreRequest(string genreName)
        {
            using var context = new ApplicationDbContext();
            Genre oldGenre = context.Genres.FirstOrDefault(s=>s.name == genreName);
            if (oldGenre == null)
            {
                Genre newGenre = new();
                newGenre.name = genreName;
                context.Genres.Add(newGenre);
                context.SaveChanges();
                Genre genreReturn = context.Genres.FirstOrDefault(s=>s.name == newGenre.name);
                return genreReturn;
            }
            MessageBox.Show("Notice - this Genre name already exists.", "Existing Name Warning");
            return null;
        }

        public static async Task<Subgenre> AddSubgenreRequest(string genreName)
        {
            using var context = new ApplicationDbContext();
            Subgenre oldGenre = context.Subgenres.FirstOrDefault(s => s.name == genreName);
            if (oldGenre == null)
            {
                Subgenre newGenre = new();
                newGenre.name = genreName;
                context.Subgenres.Add(newGenre);
                context.SaveChanges();
                Subgenre genreReturn = context.Subgenres.FirstOrDefault(s => s.name == newGenre.name);
                return genreReturn;
            }
            MessageBox.Show("Notice - this Genre name already exists.", "Existing Name Warning");
            return null;
        }

        public static async Task<Library> AddLibraryRequest(string libraryName)
        {
            using var context = new ApplicationDbContext();
            Library oldLibrary = context.Libraries.FirstOrDefault(s => s.name == libraryName);
            if (oldLibrary == null)
            {
                Library newLib = new();
                newLib.name = libraryName;
                context.Libraries.Add(newLib);
                context.SaveChanges();
                Library genreReturn = context.Libraries.FirstOrDefault(s => s.name == newLib.name);
                return genreReturn;
            }
            MessageBox.Show("Notice - this Genre name already exists.", "Existing Name Warning");
            return null;
        }

        public static async Task<String> CheckLibrary()
        {
            StringBuilder checkResult = new();
            using var context = new ApplicationDbContext();
            List<Library> libraries = await GetLibrariesList();
            List<Album> albums = await GetAlbumsList();
            List<Genre> genres = await GetGenresList();
            List<Subgenre> subgenres = await GetSubgenresList();
            List<EFTrack> tracks = await GetTrackList();
            List<EFUsage> usages = await GetUsagesList();
            List<EFStatement> statements = await GetStatementsList();
            List<EFShow> shows = await GetShowsList();
            List<EFChannel> channels = await GetChannelsList();
            List<RootTrack> rootTracks = await GetRootTrackList();
            foreach (EFShow show in shows)
            {
                if (show.channel == null && show.channel_id != null)
                {
                    show.channel = channels.FirstOrDefault(s => s.id == show.channel_id);
                    UpdateShow(show);
                    checkResult.Append($"show/channel mismatch in {show.show_name}");
                }
            }
            foreach (RootTrack track in rootTracks)
            {
                string libraryName = track.library != null ? track.library.name : "NULL";
                string albumName = track.album != null ? track.album.name : "NULL";
                string genreName = track.genre != null ? track.genre.name : "NULL";
                string subgenreName = track.subgenre != null ? track.subgenre.name : "NULL";
                string libraryID = track.library_id != null ? track.library_id.ToString() : "NULL";
                string albumID = track.album_id != null ? track.album_id.ToString() : "NULL";
                string genreID = track.genre_id != null ? track.genre_id.ToString() : "NULL";
                string subgenreID = track.subgenre_id != null ? track.subgenre_id.ToString() : "NULL";
                if ((track.library == null && track.library_id != null) || (track.library != null && track.library_id == null))
                {
                    checkResult.Append($"mismatch library and id in {track.trackName}\nLibrary is {libraryName}, id is set to {libraryID}\n");
                    if (track.library != null)
                    {
                        track.library_id = track.library.id;
                        UpdateRootTrack(track);
                    }
                    checkResult.Append("Track library id updated");
                }
                if ((track.genre == null && track.genre_id != null) || (track.genre != null && track.genre_id == null))
                {
                    checkResult.Append($"mismatch genre and id in {track.trackName}\nGenre is {genreName}, id is set to {genreID}\n");
                    if (track.genre != null)
                    {
                        track.genre_id = track.genre.id;
                        UpdateRootTrack(track);
                    }
                    checkResult.Append("Track library id updated");
                }
                if ((track.subgenre == null && track.subgenre_id != null) || (track.subgenre != null && track.subgenre_id == null))
                {
                    checkResult.Append($"mismatch subgenre and id in {track.trackName}\nSubgenre is {subgenreName}, id is set to {subgenreID}\n");
                    if (track.library != null)
                    {
                        track.subgenre_id = track.subgenre.id;
                        UpdateRootTrack(track);
                    }
                    checkResult.Append("Track library id updated");
                }
                if ((track.album == null && track.album_id != null) || (track.album != null && track.album_id == null))
                {
                    checkResult.Append($"mismatch album and id in {track.trackName}\nAlbum is {albumName}, id is set to {albumID}\n");
                    if (track.library != null)
                    {
                        track.album_id = track.album.id;
                        UpdateRootTrack(track);
                    }
                    checkResult.Append("Track library id updated");
                }
                int library_id = track.library != null ? libraries.FirstOrDefault(t=>t.name == track.library.name).id : -1;
                int album_id = track.album != null ? albums.FirstOrDefault(t => t.name == track.album.name).id : -1;
                int genre_id = track.genre != null ? genres.FirstOrDefault(t => t.name == track.genre.name).id : -1;
                int subgenre_id = track.subgenre != null ? subgenres.FirstOrDefault(t => t.name == track.subgenre.name).id : -1;
                if (track.album_id != null && track.album_id != album_id) { checkResult.Append($"Album: Track and DB Mismatch in {track.trackName}\n"); }
                if (track.library_id != null && track.library_id != library_id) { checkResult.Append($"Library: Track and DB Mismatch in {track.trackName}\n"); }
                if (track.genre_id != null && track.genre_id != genre_id) { checkResult.Append($"Genre: Track and DB Mismatch in {track.trackName}\n"); }
                if (track.subgenre_id != null && track.subgenre_id != subgenre_id) { checkResult.Append($"Subgenre: Track and DB Mismatch in {track.trackName}\n"); }
            }
            foreach (EFUsage usage in usages)
            {
                if (usage.channel == null && usage.channel_id != null)
                {
                    usage.channel = channels.FirstOrDefault(s => s.id == usage.channel_id);
                    UpdateUsage(usage);
                    checkResult.Append($"usage/channel mismatch in {usage.id.ToString()}\n");
                }
                if (usage.statement == null && usage.statement_id != null)
                {
                    usage.statement = statements.FirstOrDefault(s => s.id == usage.statement_id);
                    UpdateUsage(usage);
                    checkResult.Append($"usage/statement mismatch in {usage.id.ToString()}\n");
                }
                if (usage.track == null && usage.track_id != null)
                {
                    usage.track = tracks.FirstOrDefault(s => s.id == usage.track_id);
                    UpdateUsage(usage);
                    checkResult.Append($"usage/track mismatch in {usage.id.ToString()}\n");
                }
                if (usage.show == null && usage.show_id != null)
                {
                    usage.show = shows.FirstOrDefault(s => s.id == usage.show_id);
                    UpdateUsage(usage);
                    checkResult.Append($"usage/show mismatch in {usage.id.ToString()}\n");
                }
            }
            context.SaveChanges();
            if (checkResult.Length == 0) { checkResult.Append("No discrepancies found"); }
            return checkResult.ToString();
        }
        public static List<EFUsage> GetAllUsages()
        {
            using var context = new ApplicationDbContext();
            List<EFUsage> usages = context.Usages.Include(u => u.track)
                .Include(u => u.track)
                .ToList();
            return usages;
        }

    }
}
