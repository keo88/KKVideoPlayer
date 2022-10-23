namespace KKVideoPlayer.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using Controls;
    using Foundation;
    using static System.Windows.MessageBoxImage;

    /// <summary>
    ///  Represents video editing control.
    /// </summary>
    public sealed class VideoControlViewModel : AttachedViewModel
    {
        private static readonly string[] ResolutionTypes = { "SD", "HD", "FHD", "QHD", "UHD" };
        private string m_PathText;
        private string m_DvdIdText;
        private string m_TitleText;
        private string m_ActorsText;
        private string m_GenresText;
        private string m_ReleaseDateText;
        private string m_SeriesText;
        private string m_DirectorsText;
        private string m_CompaniesText;
        private VideoEntry m_EditedVideo;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoControlViewModel"/> class.
        /// </summary>
        /// <param name="root">root viewmodel.</param>
        public VideoControlViewModel(RootViewModel root)
            : base(root)
        {
            ActorsText = string.Empty;
            GenresText = string.Empty;
        }

        /// <summary>
        /// Parent Window of this control.
        /// </summary>
        public Window ParentWindow { get; set; }

        /// <summary>
        /// Control for actors.
        /// </summary>
        public VideoPropertyListControl ActorsControl { get; set; }

        /// <summary>
        /// Control for genres.
        /// </summary>
        public VideoPropertyListControl GenresControl { get; set; }

        /// <summary>
        ///  File path in string, input from user.
        /// </summary>
        public string PathText
        {
            get => m_PathText;
            set => SetProperty(ref m_PathText, value);
        }

        /// <summary>
        ///  File path in string, input from user.
        /// </summary>
        public string DvdIdText
        {
            get => m_DvdIdText;
            set => SetProperty(ref m_DvdIdText, value);
        }

        /// <summary>
        ///  DvdId in string, input from user.
        /// </summary>
        public string TitleText
        {
            get => m_TitleText;
            set => SetProperty(ref m_TitleText, value);
        }

        /// <summary>
        ///  Actors in string, input from user.
        /// </summary>
        public string ActorsText
        {
            get => m_ActorsText;
            set => SetProperty(ref m_ActorsText, value);
        }

        /// <summary>
        ///  Genres in string, input from user.
        /// </summary>
        public string GenresText
        {
            get => m_GenresText;
            set => SetProperty(ref m_GenresText, value);
        }

        /// <summary>
        ///  Release date in string, input from user.
        /// </summary>
        public string ReleaseDateText
        {
            get => m_ReleaseDateText;
            set => SetProperty(ref m_ReleaseDateText, value);
        }

        /// <summary>
        ///  Series in string, input from user.
        /// </summary>
        public string SeriesText
        {
            get => m_SeriesText;
            set => SetProperty(ref m_SeriesText, value);
        }

        /// <summary>
        ///  Directors in string, input from user.
        /// </summary>
        public string DirectorsText
        {
            get => m_DirectorsText;
            set => SetProperty(ref m_DirectorsText, value);
        }

        /// <summary>
        ///  Companies in string, input from user.
        /// </summary>
        public string CompaniesText
        {
            get => m_CompaniesText;
            set => SetProperty(ref m_CompaniesText, value);
        }

        /// <summary>
        ///  Info about video maintained through editing process. Used to compare changes.
        /// </summary>
        private static List<string> MaintainedVideoInfo { get; set; }

        /// <summary>
        ///  A video entry being edited.
        /// </summary>
        private VideoEntry EditedVideo
        {
            get => m_EditedVideo;
            set => SetProperty(ref m_EditedVideo, value);
        }

        /// <summary>
        ///  Open Update window.
        /// </summary>
        public void OpenEditControl()
        {
            EditedVideo = Root.Playlist.SelectedVideo;
            VideoEntry selectedVideo = EditedVideo;
            if (selectedVideo == null ||
                string.IsNullOrEmpty(selectedVideo.Filepath) ||
                string.IsNullOrWhiteSpace(Root.CurrentVideoDirectory))
                return;

            string oldDvdId = selectedVideo.DvdId;
            string sourcePath = selectedVideo.Filepath;
            string sourceFullPath = Path.Combine(Root.CurrentVideoDirectory, sourcePath);
            bool videoIsOpen = selectedVideo.Filepath.Equals(Root.PlayingVideo?.Filepath, StringComparison.Ordinal);

            ActorsControl.ResetItems(selectedVideo.Actors);

            // ActorsText = string.Join(", ", selectedVideo.Actors);
            GenresControl.ResetItems(selectedVideo.Genres);

            // GenresText = string.Join(", ", selectedVideo.Genres);
            DvdIdText = selectedVideo.DvdId;
            TitleText = selectedVideo.Title;
            PathText = selectedVideo.Filepath;
            SeriesText = selectedVideo.Series;
            DirectorsText = string.Join(", ", selectedVideo.Directors);
            CompaniesText = string.Join(", ", selectedVideo.Companies);

            ReleaseDateText = selectedVideo.ReleaseDate.Equals(DateTime.MinValue) ? string.Empty : selectedVideo.ReleaseDate.ToString("yyyy-MM-dd");

            MaintainedVideoInfo = new List<string>
            {
                oldDvdId,
                sourcePath,
                sourceFullPath,
                videoIsOpen.ToString(),
            };
        }

        /// <summary>
        /// Apply changes made to video and genres text.
        /// </summary>
        public void ApplyActorsAndGenres()
        {
            if (!string.IsNullOrWhiteSpace(ActorsText))
                ActorsControl.AddItems(ActorsText.Split(',').Select(p => p.Trim()).ToList());
            if (!string.IsNullOrWhiteSpace(GenresText))
                GenresControl.AddItems(GenresText.Split(',').Select(p => p.Trim()).ToList());

            ActorsText = string.Empty;
            GenresText = string.Empty;
        }

        /// <summary>
        /// Apply changes made in controls to Database.
        /// </summary>
        public async void ApplyVideoInfoChanges()
        {
            string targetPath = PathText;

            if (string.IsNullOrWhiteSpace(targetPath) || MaintainedVideoInfo == null)
                return;

            VideoEntry selectedVideo = EditedVideo;

            string oldDvdId = MaintainedVideoInfo[0];
            string sourcePath = MaintainedVideoInfo[1];
            string sourceFullPath = MaintainedVideoInfo[2];
            bool videoIsOpen = bool.Parse(MaintainedVideoInfo[3]);

            string targetFullPath = Path.Combine(Root.CurrentVideoDirectory, targetPath);
            bool targetExists = File.Exists(targetFullPath);
            bool sourceExists = File.Exists(sourceFullPath);

            // DvdId가 P.Key인데 dvd id가 빈칸이고 다른 곳은 차있으면 DB에 insert가 안되므로 return.
            if (string.IsNullOrWhiteSpace(DvdIdText) && (
                    ActorsControl.VideoProperties.Any() ||
                    GenresControl.VideoProperties.Any() ||
                    !string.IsNullOrWhiteSpace(TitleText)))
            {
                MessageBox.Show("Cannot edit descriptions while the dvdid is empty.");
                return;
            }

            selectedVideo.Actors = ActorsControl.VideoProperties.ToList();
            selectedVideo.Genres = GenresControl.VideoProperties.ToList();
            selectedVideo.DvdId = DvdIdText;
            selectedVideo.Title = TitleText;
            selectedVideo.Filepath = PathText;
            selectedVideo.Series = SeriesText;
            selectedVideo.Directors = DirectorsText.Split(',').Select(p => p.Trim()).ToList();
            selectedVideo.Companies = CompaniesText.Split(',').Select(p => p.Trim()).ToList();

            try
            {
                selectedVideo.ReleaseDate = DateTime.ParseExact(ReleaseDateText, "yyyy-MM-dd", null);
            }
            catch (FormatException)
            {
                selectedVideo.ReleaseDate = DateTime.MinValue;
            }

            using (var conn =
                   new SQLiteConnection("Data Source=" + Root.CurrentVideoDirectory + "/deepdark.db"))
            {
                conn.Open();

                using (SQLiteTransaction tr = conn.BeginTransaction())
                {
                    SQLiteCommand cmd = new SQLiteCommand(string.Empty, conn);
                    cmd.Transaction = tr;

                    string sql = $"UPDATE Files SET dvdId='{selectedVideo.DvdId}' WHERE filepath = '{sourcePath}'";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    // XOR(sourceExists, targetExists)의 결과가 true이면 File, Favorites에 대해 Path를 Update.
                    if (sourceExists ^ targetExists)
                    {
                        sql = $"UPDATE Files SET filepath = '{selectedVideo.Filepath}' WHERE filepath = '{sourcePath}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        sql =
                            $"UPDATE Favorites SET filepath = '{selectedVideo.Filepath}' WHERE filepath = '{sourcePath}'";
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        // target은 없고 source만 존재하면 OS에서 rename 필요.
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (sourceExists && !targetExists)
                        {
                            try
                            {
                                if (videoIsOpen)
                                {
                                    if (await App.ViewModel.MediaElement.Close())
                                        File.Move(sourceFullPath, targetFullPath);
                                }
                                else
                                {
                                    File.Move(sourceFullPath, targetFullPath);
                                }
                            }
                            catch (IOException ex)
                            {
                                _ = MessageBox.Show(
                                    $"Unable to rename file.\n{ex.Message}",
                                    "Error",
                                    MessageBoxButton.OK,
                                    Error);
                                tr.Rollback();
                                conn.Close();
                                return;
                            }

                            PlaylistViewModel.MigrateSubtitle(sourceFullPath, targetFullPath);
                        }
                    }
                    else if (!sourceFullPath.Equals(targetFullPath))
                    {
                        if (targetExists)
                            MessageBox.Show($"Another file already exists at '{targetFullPath}'");
                        else
                            MessageBox.Show($"Cannot bind new file name, '{targetFullPath}'\nThere is no such file.");
                    }

                    sql = $"DELETE FROM Dvd WHERE dvdId = '{oldDvdId}' or dvdId = '{selectedVideo.DvdId}'";
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();

                    sql =
                        "INSERT INTO Dvd(dvdId, dvdTitle, actors, genres, releaseDate, director, company, series, metaSource) VALUES (@dvdId, @dvdTitle, @actors, @genres, @releaseDate, @director, @company, @series, @metaSource)";
                    cmd.CommandText = sql;

                    cmd.Parameters.AddWithValue("@dvdId", selectedVideo.DvdId);
                    cmd.Parameters.AddWithValue("@dvdTitle", selectedVideo.Title);
                    cmd.Parameters.AddWithValue("@actors", string.Join(", ", selectedVideo.Actors));
                    cmd.Parameters.AddWithValue("@genres", string.Join(", ", selectedVideo.Genres));
                    cmd.Parameters.AddWithValue(
                        "@releaseDate",
                        selectedVideo.ReleaseDate != DateTime.MinValue ? selectedVideo.ReleaseDate.ToString("yyyy-MM-dd") : null);
                    cmd.Parameters.AddWithValue("@director", string.Join(", ", selectedVideo.Directors));
                    cmd.Parameters.AddWithValue("@company", string.Join(", ", selectedVideo.Companies));
                    cmd.Parameters.AddWithValue("@series", selectedVideo.Series);
                    cmd.Parameters.AddWithValue("@metaSource", null);
                    cmd.ExecuteNonQuery();

                    tr.Commit();
                }

                conn.Close();
            }

            // Root.Playlist.RefreshPlaylist();
        }

        /// <summary>
        /// Deformat file path into video info.
        /// </summary>
        public void DeformatFilepath()
        {
            string fp = PathText;

            string[] fpSplitByDot = fp.Split('.');

            try
            {
                string[] fpSplitComa = string.Join('.', fpSplitByDot.SkipLast(1)).Split(',');
                string[] fpSplitSharp = fpSplitComa.Length > 1
                    ? string.Join(',', fpSplitComa.SkipLast(1)).Split('#')
                    : fpSplitComa[0].Split('#');
                string fpHeader, fpBody;

                if (fpSplitSharp.Length > 1)
                {
                    fpHeader = fpSplitSharp[0].Trim();
                    fpBody = fpSplitSharp[1].Trim();
                }
                else
                {
                    fpHeader = fpSplitSharp[0];
                    fpBody = string.Empty;
                }

                string fpFooter = fpSplitComa[^1].Trim();
                string[] fpFooterSplit = fpFooter.Split(' ');

                string dvdId = fpFooterSplit.Length > 1
                    ? string.Join(' ', fpFooterSplit.SkipWhile(x => x.Contains('['))).Trim()
                    : fpFooterSplit[0].Trim();
                if (dvdId.Contains('['))
                    dvdId = string.Empty;

                var actors = new List<string>();

                string[] titleSplit = fpHeader.Split(new char[] { ')', ']' });
                string title = titleSplit[^1].Trim();

                if (fpSplitComa.Length == 1)
                {
                    dvdId = AssumeVideoCode(fp);
                    if (string.IsNullOrWhiteSpace(dvdId))
                        dvdId = title;
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    title = dvdId;
                }

                string[] frontTags = BracketSplit(fpHeader, '[', ']');
                var genres = frontTags.Select(item => item.ToUpper().Trim()).ToList();

                if (!string.IsNullOrWhiteSpace(fpBody))
                {
                    string[] bodyTags = fpBody.Split(new char[] { ' ', '#' });
                    genres.AddRange(bodyTags.Select(item => item.ToUpper().Trim()));
                }

                string[] footerTags = BracketSplit(fpFooter, '[', ']');
                genres.AddRange(footerTags.Select(item => item.ToUpper().Trim()));

                string[] actorsStr = BracketSplit(fpHeader, '(', ')');
                if (actorsStr.Length > 0)
                {
                    actors.AddRange(actorsStr[0].Split(',').Select(actor => actor.Trim()));
                }

                actors = actors.Distinct().ToList();
                genres = genres.Distinct().ToList();

                // If there is no resolution typed genre in the genres list, try make one.
                if (!genres.Any((item) => ResolutionTypes.Contains(item)))
                {
                    bool gotResolution = TryGetVideoResolution(out string resolution);
                    if (gotResolution)
                        genres.Add(resolution);
                }

                DvdIdText = dvdId;
                ActorsControl.AddItems(actors);
                GenresControl.AddItems(genres);
                TitleText = title;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to process {fp}\n" + ex.Message, "Error", MessageBoxButton.OK, Error);
            }
        }

        /// <summary>
        ///  Format file. Don't use it if you don't know it.
        /// </summary>
        public void FormatFilepath()
        {
            string[] fpSplitByDot = PathText.Split('.');
            string extender = fpSplitByDot[^1];

            var genresList = GenresControl.VideoProperties
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim()).ToList();
            string actors = string.Empty;
            string title = TitleText;
            string dvdId = DvdIdText;
            bool isDuplicateTitle = false;

            if (string.IsNullOrWhiteSpace(dvdId) && !string.IsNullOrWhiteSpace(title))
            {
                dvdId = title;
                isDuplicateTitle = true;
            }
            else if (title != null && title.Equals(dvdId))
            {
                isDuplicateTitle = true;
            }

            string actorText = string.Join(", ", ActorsControl.VideoProperties);
            if (!string.IsNullOrWhiteSpace(actorText))
            {
                actors = "(" + actorText + ")";
            }

            List<string> resolutions = genresList.Where(item => ResolutionTypes.Contains(item)).ToList();
            string resolution = string.Empty;

            if (resolutions.Count > 0)
            {
                resolution = resolutions[0];
                genresList.Remove(resolution);
                resolution = " [" + resolution + "]";
            }
            else
            {
                if (Root.PlayingVideo == EditedVideo)
                {
                    bool gotResolution = TryGetVideoResolution(out resolution);

                    if (gotResolution)
                        resolution = " [" + resolution + "]";
                }
            }

            string firstGenre = string.Empty;
            if (genresList.Count > 0)
            {
                firstGenre = genresList[0];
                genresList.Remove(firstGenre);
                firstGenre = "[" + firstGenre + "]";
            }

            string genres = string.Empty;
            if (genresList.Count > 0)
            {
                genres = " #" + string.Join(" ", genresList);
            }

            string filePath;

            // Remove duplicate title, if dvdId and the title is the same.
            if (isDuplicateTitle)
            {
                filePath = $"{firstGenre}{actors}{genres},{resolution} {dvdId}.{extender}";
            }
            else
            {
                filePath = $"{firstGenre}{actors}{title}{genres},{resolution} {dvdId}.{extender}";
            }

            PathText = filePath;
        }

        /// <summary>
        ///  Crawling from the web to get video info.
        /// </summary>
        public async void CrawlVideoInfoAsync()
        {
            if (string.IsNullOrWhiteSpace(DvdIdText))
            {
                string assumedVideoCode = AssumeVideoCode(PathText);

                if (!string.IsNullOrWhiteSpace(assumedVideoCode))
                    DvdIdText = assumedVideoCode;
            }

            VideoEntry v = new ();

            Task<bool> navigateTask = Root.Crawler.NavigateJavDb(DvdIdText, v);
            bool ret = await navigateTask;

            if (ret)
            {
                if (!string.IsNullOrWhiteSpace(v.Title))
                    TitleText = v.Title;

                if (!v.ReleaseDate.Equals(DateTime.MinValue))
                    ReleaseDateText = v.ReleaseDate.ToString("yyyy-MM-dd");

                if (v.Directors.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(DirectorsText))
                        DirectorsText += ", " + string.Join(", ", v.Directors);
                    else
                        DirectorsText = string.Join(", ", v.Directors);
                }

                if (v.Companies.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(CompaniesText))
                        CompaniesText += ", " + string.Join(", ", v.Companies);
                    else
                        CompaniesText = string.Join(", ", v.Companies);
                }

                if (v.Actors.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(ActorsText))
                        ActorsText += ", " + string.Join(", ", v.Actors);
                    else
                        ActorsText = string.Join(", ", v.Actors);
                }

                if (v.Genres.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(GenresText))
                        GenresText += ", " + string.Join(", ", v.Genres);
                    else
                        GenresText = string.Join(", ", v.Genres);
                }
            }
            else
            {
                _ = MessageBox.Show(
                    "Something went wrong.\nCannot connect to requested url.\nCheck your DNS/SNI Settings.\nIt may also mean your search id is not present in the Web DB.",
                    "Error",
                    MessageBoxButton.OK,
                    Error);
            }
        }

        /// <summary>
        /// Simple web browser open to search dvd id.
        /// </summary>
        /// <param name="url">Target url. query url must contain SEARCH_TERM in order to execute appropriate dvd id search query.</param>
        public void OpenDvdIdWeb(string url)
        {
            if (string.IsNullOrWhiteSpace(DvdIdText))
            {
                string assumedVideoCode = AssumeVideoCode(PathText);

                if (!string.IsNullOrWhiteSpace(assumedVideoCode))
                    DvdIdText = assumedVideoCode;
            }

            if (!url.Contains("SEARCH_TERM"))
                throw new ArgumentException("Target url is invalid. Must contain SEARCH_TERM.");

            string completeUrl = url.Replace("SEARCH_TERM", DvdIdText);
            WebCrawlDriver.OpenWebBrowser(completeUrl);
        }

        /// <summary>
        /// Display and select item in explorer.
        /// </summary>
        /// <param name="path">Path to the selected item.</param>
        public void ShowItemInExplorer(string path = null)
        {
            string pathCmd = path ?? Path.Combine(Root.CurrentVideoDirectory, PathText);

            if (!File.Exists(pathCmd))
            {
                MessageBox.Show($"File not found\n{pathCmd}", "Error", MessageBoxButton.OK, Error);
                return;
            }

            string cmdText = $"/n,/select,\"{pathCmd}\"";

            var showProcInfo = new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = cmdText,
            };
            Process.Start(showProcInfo);
        }

        private static string[] BracketSplit(string str, char sep1, char sep2)
        {
            List<string> output = new List<string>();

            try
            {
                var splitStr = str.Split(sep1);
                var strPrev = splitStr.Skip(1);

                foreach (string piece in strPrev)
                {
                    output.Add(piece.Split(sep2)[0]);
                }
            }
            catch (NullReferenceException)
            {
                return null;
            }

            return output.ToArray();
        }

        private bool TryGetVideoResolution(out string resolution)
        {
            resolution = string.Empty;
            bool gotValues = Root.TryGetVideoResolution(out int width, out int _);

            if (!gotValues)
                return false;

            if (width > 0 && width < 1280)
            {
                resolution = ResolutionTypes[0];
            }
            else if (width >= 1280 && width < 1920)
            {
                resolution = ResolutionTypes[1];
            }
            else if (width >= 1920 && width < 2560)
            {
                resolution = ResolutionTypes[2];
            }
            else if (width >= 2560 && width < 3840)
            {
                resolution = ResolutionTypes[3];
            }
            else if (width >= 3840)
            {
                resolution = ResolutionTypes[4];
            }

            return !string.IsNullOrWhiteSpace(resolution);
        }

        private string AssumeVideoCode(string raw)
        {
            string dvdId = string.Empty;
            foreach (Match match in Regex.Matches(raw, @"\w{3,}-\d{3,}"))
            {
                dvdId = match.ToString();
                DvdIdText = dvdId;
            }

            return dvdId;
        }
    }
}