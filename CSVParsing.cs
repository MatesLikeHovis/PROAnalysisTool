using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using WPFBMI.Models;
using System.Text.RegularExpressions;

namespace WPFBMI
{
    public class CSVParsing
    {

        public static async Task<string> ParseFolderCSV(string folderPath)
        {
            List<string> csvFiles = new List<string>();
            StringBuilder response = new();
            if (Directory.Exists(folderPath))
            {
                csvFiles.AddRange(Directory.GetFiles(folderPath, "*.csv"));
            }
            else
            {
                Console.WriteLine("Directory does not exist: " + folderPath);
                response.Append("Directory does not exist: " + folderPath);
            }
            foreach (string file in csvFiles)
            {
                string value = await ProcessStatementCsv(file);
                response.Append(value);
            }
            return response.ToString();
        }

        public static async Task<string> ParseMetadataCSV(string filePath)
        {
            StringBuilder response = new();
            response.Append($"File parsing: {filePath}\n");
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                int? genreHeader = null;
                int? subgenreHeader = null;
                int? albumHeader = null;
                int? libraryHeader = null;
                int? trackNameHeader = null;
                List<Genre> genreList = await EFData.GetGenresList();
                List<Subgenre> subgenreList = await EFData.GetSubgenresList();
                List<Album> albumList = await EFData.GetAlbumsList();
                List<Library> libraryList = await EFData.GetLibrariesList();
                List<RootTrack> trackList = await EFData.GetRootTrackList();
                List<RootTrack> updatingTracks = new();
                List<RootTrack> newTracks = new();

                string[] headers = lines.First().Split(',');
                for (int i = 0; i < headers.Length; i++)
                {
                    if (headers[i].ToLower() == "name" || headers[i].ToLower() == "track" || headers[i].ToLower() == "track name")
                    {
                        trackNameHeader = i;
                    }
                    if (headers[i].ToLower() == "genre")
                    {
                        genreHeader = i;
                    }
                    if (headers[i].ToLower() == "subgenre")
                    {
                        subgenreHeader = i;
                    }
                    if (headers[i].ToLower() == "album name" || headers[i].ToLower() == "album")
                    {
                        albumHeader = i;
                    }
                    if (headers[i].ToLower() == "library" || headers[i].ToLower() == "library name")
                    {
                        libraryHeader = i;
                    }
                }
                if (trackNameHeader == null) { return $"Error - no Track Name Column Found in {filePath}"; }
                foreach (var line in lines.Skip(1))
                {
                    string pattern = @",(?=(?:[^""]*""[^""]*"")*[^""]*$)";
                    string[] columns = Regex.Split(line, pattern);
                    string? trackName = columns[trackNameHeader.GetValueOrDefault()];
                    string? albumName = albumHeader != null ? columns[albumHeader.GetValueOrDefault()] : null;
                    string? genreName = genreHeader != null ? columns[genreHeader.GetValueOrDefault()] : null;
                    string? subgenreName = subgenreHeader != null ? columns[subgenreHeader.GetValueOrDefault()] : null;
                    string? libraryName = libraryHeader != null ? columns[libraryHeader.GetValueOrDefault()] : null;
                    Genre thisGenre = null;
                    Album thisAlbum = null;
                    Subgenre thisSubGenre = null;
                    Library thisLibrary = null;
                    if (genreName != null && genreName != "")
                    {
                        (genreList, thisGenre) = await GetGenre(genreName, genreList);
                    }
                    if (albumName != null && albumName != "")
                    { 
                        (albumList, thisAlbum) = await GetAlbum(albumName, albumList);
                    }
                    if (subgenreName != null && subgenreName != "")
                    {
                        (subgenreList, thisSubGenre) = await GetSubgenre(subgenreName, subgenreList);
                    }
                    if (libraryName != null && libraryName != "")
                    {
                        (libraryList, thisLibrary) = await GetLibrary(libraryName, libraryList);
                    }
                    RootTrack? oldTrack = trackList.FirstOrDefault(s=>s.trackName == trackName);

                    if (oldTrack == null)
                    {
                        RootTrack newTrack = new();
                        newTrack.trackName = trackName;
                        newTrack.genre = thisGenre;
                        newTrack.genre_id = thisGenre != null ? thisGenre.id : null;
                        newTrack.subgenre = thisSubGenre;
                        newTrack.subgenre_id = thisSubGenre != null ? thisSubGenre.id : null;
                        newTrack.library = thisLibrary;
                        newTrack.library_id = thisLibrary != null ? thisLibrary.id : null;
                        newTrack.album = thisAlbum;
                        newTrack.album_id = thisAlbum != null ? thisAlbum.id : null;
                        newTracks.Add(newTrack);
                    } else if (trackName != null && trackName != "")
                    {
                        if (thisGenre != null)
                        {
                            oldTrack.genre = thisGenre;
                            oldTrack.genre_id = thisGenre.id;
                        }
                        if (thisSubGenre != null)
                        {
                            oldTrack.subgenre = thisSubGenre;
                            oldTrack.subgenre_id = thisSubGenre.id;
                        }
                        if (thisLibrary != null)
                        {
                            oldTrack.library = thisLibrary;
                            oldTrack.library_id = thisLibrary.id;
                        }
                        if (thisAlbum != null)
                        {
                            oldTrack.album = thisAlbum;
                            oldTrack.album_id = thisAlbum.id;
                        }
                        updatingTracks.Add(oldTrack);
                    }
                }
                int newAdds = 0;
                foreach (RootTrack track in newTracks)
                {
                    await EFData.InsertRootTrack(track);
                    newAdds++;
                }
                if (updatingTracks.Count < 1) { response.Append($"{newAdds} tracks have been added.  There are no tracks to update.\n"); }
                response.Append($"{newAdds} tracks added.  {updatingTracks.Count} tracks  updated.\n");
                foreach (RootTrack track in updatingTracks)
                {
                    EFData.UpdateRootTrack(track);
                }
            }
            catch (Exception ex)
            {
                response.Append($"Error parsing CSV file: {ex.InnerException}\n");
            }
            return response.ToString();
        }

        public static async Task<(List<Genre>? genreList, Genre? newGenre)> GetGenre(string genre, List<Genre> genreList)
        {
            Genre? oldGenre = genreList.FirstOrDefault(s => s.name == genre);
            if (oldGenre != null) { return (genreList, oldGenre); }
            await EFData.AddGenreRequest(genre);
            List<Genre> newGenreList = await EFData.GetGenresList();
            Genre? newGenre = newGenreList.FirstOrDefault(s => s.name == genre);
            return (newGenreList, newGenre);
        }

        public static async Task<(List<Album>? itemList, Album? newAlbum)> GetAlbum(string item, List<Album> itemList)
        {
            Album? oldItem = itemList.FirstOrDefault(s => s.name == item);
            if (oldItem != null) { return (itemList, oldItem); }
            await EFData.AddAlbumRequest(item);
            List<Album> newItemList = await EFData.GetAlbumsList();
            Album? newItem = newItemList.FirstOrDefault(s => s.name == item);
            return (newItemList, newItem);
        }

        public static async Task<(List<Subgenre>? itemList, Subgenre? newItem)> GetSubgenre(string item, List<Subgenre> itemList)
        {
            Subgenre? oldItem = itemList.FirstOrDefault(s => s.name == item);
            if (oldItem != null) { return (itemList, oldItem); }
            await EFData.AddSubgenreRequest(item);
            List<Subgenre> newItemList = await EFData.GetSubgenresList();
            Subgenre? newItem = newItemList.FirstOrDefault(s => s.name == item);
            return (newItemList, newItem);
        }

        public static async Task<(List<Library>? itemList, Library? newItem)> GetLibrary(string item, List<Library> itemList)
        {
            Library? oldItem = itemList.FirstOrDefault(s => s.name == item);
            if (oldItem != null) { return (itemList, oldItem); }
            await EFData.AddLibraryRequest(item);
            List<Library> newItemList = await EFData.GetLibrariesList();
            Library? newItem = newItemList.FirstOrDefault(s => s.name == item);
            return (newItemList, newItem);
        }

        public static async Task<List<StatementEntry>> ParseCsv(string filePath)
        {
            var entries = new List<StatementEntry>();
            Dictionary<string, List<string>> c = CsvPairingSet();

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                string[] headers = lines.First().Split(',');
                Dictionary<string, int> h = GetHeaderColumns(c, headers);
                if (!h.ContainsKey("title_name"))
                {
                    MessageBox.Show($"{filePath} CSV Does not contain a valid 'work title name' field.  This file may not be a statement csv.", "Warning field not found");
                    return null;
                }

                foreach (var line in lines.Skip(1))
                {
                    string[] columns = line.Split(',');

                    int timing_val = columns[h["timing_secs"]].Length == 0 ? 0 : ConvertTimeToSeconds(columns[h["timing_secs"]]);
                    double percent1 = columns[h["percent"]] == "" ? 0 : Double.Parse(columns[h["percent"]]);
                    int perf_counts1 = columns[h["perf_counts"]] == "" ? 0 : Int32.Parse(columns[h["perf_counts"]]);
                    double royalty_amt = columns[h["royalty_amount"]] == "" ? 0 : Double.Parse(columns[h["royalty_amount"]]);
                    int perf_prd = columns[h["perf_period"]] == "" ? 0 : Int32.Parse(columns[h["perf_period"]]);
                    double current_activity1 = columns[h["current_activity"]] == "" ? 0 : Double.Parse(columns[h["current_activity"]]);
                    double hits_bonus1 = columns[h["hits_bonus"]] == "" ? 0 : Double.Parse(columns[h["hits_bonus"]]);
                    double theme_bonus1 = columns[h["theme_bonus"]] == "" ? 0 : Double.Parse(columns[h["theme_bonus"]]);
                    // Create a new StatementEntry based on columns
                    var entry = new StatementEntry
                    {
                        participant = columns[h["participant"]],
                        episode_name = columns[h["episode_name"]],
                        use_code = columns[h["use_code"]],
                        timing_secs = timing_val,
                        percent = percent1,
                        perf_counts = perf_counts1,
                        bonus_level = columns[h["bonus_level"]],
                        royalty_amount = royalty_amt,
                        perf_period = perf_prd,
                        current_activity = current_activity1,
                        hits_bonus = hits_bonus1,
                        theme_bonus = theme_bonus1,
                        foreign_adj = columns[h["foreign_adj"]],
                        statement_period = columns[h["statement_period"]],
                        title_name = columns[h["title_name"]],
                        title_num = columns[h["title_num"]],
                        perf_source = columns[h["perf_source"]],
                        perf_country = columns[h["perf_country"]],
                        show_name = columns[h["show_name"]],
                        show_num = columns[h["show_num"]],
                        participant_num = columns[h["participant_num"]]
                    };

                    entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing CSV file {filePath}: {ex.Message}");
            }

            return entries;
        }
        
        public static async Task<string> ProcessStatementCsv(string filePath)
        {
            int records = 0;
            List<StatementEntry> entries = await CSVParsing.ParseCsv(filePath);
            if (entries != null && entries.Count > 0)
            {
                records = await EFData.SaveEntrySet(entries);
            }
            return $"CSV Processed for {records} records in {filePath}\n";
        }


        public static int ConvertTimeToSeconds(string value)
        {
            int amt = 0;
            string[] vals = value.Split(":");
            int minutes = int.Parse(vals[0]);
            int seconds = int.Parse(vals[1]);
            amt += (minutes * 60) + seconds;
            return amt;
        }

        public static Dictionary<string, int> GetHeaderColumns(Dictionary<string, List<string>> csvPairs,
            string[] headers)
        {
            Dictionary<string, int> pairs = new();
            for (int i = 0; i < headers.Length; i++)
            {
                foreach (string key in csvPairs.Keys)
                {
                    if (csvPairs[key].Contains(headers[i]))
                    {
                        pairs[key] = i; break;
                    }
                }
            }
            return pairs;
        }

        public static Dictionary<string, List<string>> CsvPairingSet()
        {
            Dictionary<string, List<string>> csvPairings = new();
            csvPairings["episode_name"] = new();
            csvPairings["use_code"] = new();
            csvPairings["timing_secs"] = new();
            csvPairings["percent"] = new();
            csvPairings["perf_counts"] = new();
            csvPairings["bonus_level"] = new();
            csvPairings["royalty_amount"] = new();
            csvPairings["perf_period"] = new();
            csvPairings["current_activity"] = new();
            csvPairings["hits_bonus"] = new();
            csvPairings["theme_bonus"] = new();
            csvPairings["foreign_adj"] = new();
            csvPairings["statement_period"] = new();
            csvPairings["title_name"] = new();
            csvPairings["title_num"] = new();
            csvPairings["perf_source"] = new();
            csvPairings["perf_country"] = new();
            csvPairings["show_name"] = new();
            csvPairings["show_num"] = new();
            csvPairings["participant"] = new();
            csvPairings["participant_num"] = new();
            csvPairings["episode_name"].Add("EPISODE NAME");
            csvPairings["use_code"].Add("USE CODE");
            csvPairings["timing_secs"].Add("TIMING");
            csvPairings["percent"].Add("PARTICIPANT %");
            csvPairings["perf_counts"].Add("PERF COUNT");
            csvPairings["bonus_level"].Add("BONUS LEVEL");
            csvPairings["royalty_amount"].Add("ROYALTY AMOUNT");
            csvPairings["perf_period"].Add("PERF PERIOD");
            csvPairings["current_activity"].Add("CURRENT ACTIVITY AMT");
            csvPairings["hits_bonus"].Add("HITS SONG OR TV NET SUPER USAGE BONUS");
            csvPairings["theme_bonus"].Add("STANDARDS OR TV NET THEME BONUS");
            csvPairings["foreign_adj"].Add("FOREIGN SOCIETY ADJUSTMENT");
            csvPairings["statement_period"].Add("PERIOD");
            csvPairings["title_name"].Add("TITLE NAME");
            csvPairings["title_num"].Add("TITLE #");
            csvPairings["perf_source"].Add("PERF SOURCE");
            csvPairings["perf_country"].Add("COUNTRY OF PERFORMANCE");
            csvPairings["show_name"].Add("SHOW NAME");
            csvPairings["show_num"].Add("SHOW #");
            csvPairings["participant"].Add("PARTICIPANT NAME");
            csvPairings["participant_num"].Add("PARTICIPANT #");
            return csvPairings;
        }
    }
}
