namespace KKVideoPlayer.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Xaml;
    using KKVideoPlayer.Foundation;
    using KKVideoPlayer.ViewModels;

    /// <summary>
    /// Interaction logic for PlaylistPanelControl.xaml.
    /// </summary>
    public partial class PlaylistPanelControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlaylistPanelControl"/> class.
        /// </summary>
        public PlaylistPanelControl()
        {
            InitializeComponent();

            // Prevent binding to the events
            if (App.IsInDesignMode)
                return;

            SearchTextBox.IsEnabledChanged += (s, e) =>
            {
                if ((bool)e.OldValue == false && (bool)e.NewValue)
                    FocusSearchBox();
            };

            IsVisibleChanged += (s, e) =>
            {
                if (SearchTextBox.IsEnabled)
                    FocusSearchBox();
            };
        }

        #region Properties

        /// <summary>
        /// A proxy, strongly-typed property to the underlying DataContext.
        /// </summary>
#pragma warning disable SA1201 // Elements should appear in the correct order
        public RootViewModel ViewModel => DataContext as RootViewModel;
#pragma warning restore SA1201 // Elements should appear in the correct order

        #endregion

        private static void FocusTextBox(TextBoxBase textBox)
        {
            DeferredAction.Create(context =>
            {
                if (textBox == null || Application.Current == null || Application.Current.MainWindow == null)
                    return;

                textBox.Focus();
                textBox.SelectAll();
                FocusManager.SetFocusedElement(Application.Current.MainWindow, textBox);
                Keyboard.Focus(textBox);

                if (textBox.IsVisible == false || textBox.IsKeyboardFocused)
                    context?.Dispose();
                else
                    context?.Defer(TimeSpan.FromSeconds(0.25));
            }).Defer(TimeSpan.FromSeconds(0.25));
        }

        /// <summary>
        /// Focuses the search box.
        /// </summary>
        private void FocusSearchBox()
        {
            FocusTextBox(SearchTextBox);
        }

        private async void FileList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock filePathElement = (TextBlock)((Grid)sender).FindName("ItemFilePath");

            if (App.ViewModel.MediaElement.IsClosing || App.ViewModel.MediaElement.IsOpening)
                return;
            else
                await App.ViewModel.Commands.OpenCommand.ExecuteAsync(Path.Combine(ViewModel.CurrentVideoDirectory, filePathElement.Text));
        }

        private async void Directory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (App.IsInDesignMode)
                return;
            if (App.ViewModel.MediaElement.IsOpen)
                await App.ViewModel.Commands.CloseCommand.ExecuteAsync(ViewModel.MediaElement);

            ViewModel.Playlist.ValidateDirectory(ViewModel.Playlist.SelectedDirectory);

            ViewModel.CurrentVideoDirectory = ViewModel.Playlist.SelectedDirectory;
            bool dbExists = SqlManager.CheckOrCreateDb(ViewModel.CurrentVideoDirectory);

            if (dbExists)
            {
                ViewModel.Controller.InitializeHighlight();
                ViewModel.Playlist.RefreshPlaylist();
            }
            else
            {
                ViewModel.Controller.MaxHighlightUid = 0;
                ViewModel.Playlist.RefreshPlaylist();
            }
        }

        private void AddDirButton_Click(object sender, RoutedEventArgs e)
        {
            AddDirectoryWindow addDirWin = new AddDirectoryWindow();
            addDirWin.DataContext = addDirWin;

            string returnPath = string.Empty;
            var result = addDirWin.ShowDialog();

            if (result == true)
            {
                string directoryName = addDirWin.DirectoryTextBox.Text;
                ViewModel.Playlist.AddDirectory(directoryName);
            }
        }

        private void FileList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)SelectAllVideoCheckBox.IsChecked)
            {
                if (FileList.SelectedItems.Count > 1)
                {
                    // Do nothing.
                }
                else
                {
                    SelectAllVideoCheckBox.IsChecked = false;
                }
            }
        }

        private void DelDirButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Playlist.RemoveDirectory(ViewModel.Playlist.SelectedDirectory);
        }

        private async void FileList_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (App.ViewModel.MediaElement.IsClosing ||
                App.ViewModel.MediaElement.IsOpening ||
                ViewModel.Playlist.SelectedVideo == null ||
                string.IsNullOrEmpty(ViewModel.Playlist.SelectedVideo.Filepath) ||
                string.IsNullOrWhiteSpace(ViewModel.CurrentVideoDirectory))
                return;

            string targetPath = Path.Combine(ViewModel.CurrentVideoDirectory, ViewModel.Playlist.SelectedVideo.Filepath);
            if (File.Exists(targetPath))
            {
                await App.ViewModel.Commands.OpenCommand.ExecuteAsync(targetPath);
            }
            else
            {
                MessageBox.Show("File does not exist.");
            }
        }

        #region Video Info Edit

        // Update Video Info.
        private void FileList_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.Playlist.VideoCtrlWindow.Show();
            ViewModel.VideoCtrl.ParentWindow = ViewModel.Playlist.VideoCtrlWindow;

            // PlaylistRightClickPopup.IsOpen = true;
            ViewModel.VideoCtrl.OpenEditControl();

            // ViewModel.Playlist.OpenUpdateWindow();
        }
        #endregion

        private void ActorListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ViewModel.Playlist.SelectedActor))
            {
                string p_str = ViewModel.Playlist.PlaylistSearchString;
                ViewModel.Playlist.PlaylistSearchString = $"{p_str}actor:{ViewModel.Playlist.SelectedActor.Trim()}, ";
            }

            ((ListView)sender).UnselectAll();
        }

        private void GenreListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ViewModel.Playlist.SelectedGenre))
            {
                string p_str = ViewModel.Playlist.PlaylistSearchString;
                ViewModel.Playlist.PlaylistSearchString = $"{p_str}genre:{ViewModel.Playlist.SelectedGenre.Trim()}, ";
            }

            ((ListView)sender).UnselectAll();
        }

        private void RefreshDirButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Playlist.UpdatePlaylist(ViewModel.CurrentVideoDirectory);
        }

        private void ScrollToSelection()
        {
            ViewModel.Playlist.EntriesView.Refresh();

            if (ViewModel.Playlist.SelectedVideo != null)
            {
                try
                {
                    ListBox lb = (ListBox)this.FindName("FileList");
                    lb.ScrollIntoView(ViewModel.Playlist.SelectedVideo);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        private void ContentTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Playlist.PlaylistTemplate = TemplateType.ContentTemplate;

            ViewModel.Playlist.EntriesView.Refresh();
            ScrollToSelection();
        }

        private void DetailTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Playlist.PlaylistTemplate = TemplateType.DetailTemplate;

            ViewModel.Playlist.EntriesView.Refresh();
            ScrollToSelection();
        }

        private void BinButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock tb = (TextBlock)((sender as FrameworkElement).Parent as FrameworkElement).FindName("ItemFilePath");

            MessageBoxResult mr = MessageBox.Show("Are you sure you want to remove this file from the list?", "Remove", MessageBoxButton.OKCancel);
            if (mr == MessageBoxResult.OK)
            {
                List<string> s = new List<string>();
                s.Add(tb.Text);
                ViewModel.Playlist.DeleteItemFromList(s);
            }

            return;
        }

        private void FlushButton_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.IList items = FileList.SelectedItems;
            List<VideoEntry> selectedVideos = items.Cast<VideoEntry>().ToList();
            List<string> deleteTargetVideo = new List<string>();

            foreach (VideoEntry v in selectedVideos)
            {
                if (!File.Exists(Path.Combine(App.ViewModel.CurrentVideoDirectory, v.Filepath)))
                {
                    deleteTargetVideo.Add(v.Filepath);
                }
            }

            if (deleteTargetVideo.Count > 0)
            {
                MessageBoxResult mr = MessageBox.Show($"Deleting entries with missing video file:\n{string.Join('\n', deleteTargetVideo)}", "Remove", MessageBoxButton.OKCancel);
                if (mr == MessageBoxResult.OK)
                {
                    ViewModel.Playlist.DeleteItemFromList(deleteTargetVideo);
                }

                return;
            }
            else
            {
                MessageBox.Show("Selected files are valid.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MigrateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(MigrateDirectoryComboBox.Text))
            {
                _ = MessageBox.Show($"Directory '{MigrateDirectoryComboBox.Text}' does not Exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Collections.IList items = FileList.SelectedItems;
            List<VideoEntry> selectedVideos = items.Cast<VideoEntry>().ToList();
            List<VideoEntry> targetVideo = new List<VideoEntry>();
            List<List<HighlightEntry>> targetHighlights = new List<List<HighlightEntry>>();

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + ViewModel.CurrentVideoDirectory + "/deepdark.db"))
            {
                conn.Open();

                foreach (VideoEntry v in selectedVideos)
                {
                    if (File.Exists(Path.Combine(App.ViewModel.CurrentVideoDirectory, v.Filepath)))
                    {
                        if (ViewModel.PlayingVideo != null && (bool)ViewModel.PlayingVideo?.Filepath.Equals(v.Filepath, StringComparison.Ordinal))
                        {
                            _ = MessageBox.Show($"File {v.Filepath} is perhaps being played?\n Stop the video before migrating.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }

                        targetVideo.Add(v);

                        List<HighlightEntry> highlights = new List<HighlightEntry>();

                        string sql = $"SELECT uid, thumb, start FROM Favorites WHERE filepath='{v.Filepath}'";
                        SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                        SQLiteDataReader rdr = cmd.ExecuteReader();

                        while (rdr.Read())
                        {
                            highlights.Add(new HighlightEntry((long)rdr["uid"], (double)rdr["start"], rdr["thumb"]));
                        }

                        targetHighlights.Add(highlights);
                    }
                }

                conn.Close();
            }

            if (targetVideo.Count > 0)
            {
                MessageBoxResult mr = MessageBox.Show($"Migrating video files to {MigrateDirectoryComboBox.SelectedItem}:\n{string.Join('\n', targetVideo.Select(str => str.Filepath))}", "Migrate", MessageBoxButton.OKCancel);
                if (mr == MessageBoxResult.OK)
                {
                    ViewModel.Playlist.MigrateFile(targetVideo, targetHighlights, ViewModel.CurrentVideoDirectory, MigrateDirectoryComboBox.Text);
                    ViewModel.Playlist.DeleteItemFromList(targetVideo.Select(str => str.Filepath));
                }

                return;
            }
            else
            {
                _ = MessageBox.Show("No file selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Sortby_Changed(object sender, RoutedEventArgs e)
        {
            ListSortDirection dir = ViewModel.AppConfig.PlaylistSettings.SortDirection ? ListSortDirection.Descending : ListSortDirection.Ascending;
            int selectedIndex = ViewModel.AppConfig.PlaylistSettings.SortComboIndex;

            switch (selectedIndex)
            {
                case 0:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("DvdId", dir);
                    break;
                case 1:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("Title", dir);
                    break;
                case 2:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("Filepath", dir);
                    break;
                case 3:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("DvdId", dir);

                    // ViewModel.Playlist.PlaylistSortDescriptionUpdate("Actors", dir);
                    break;
                case 4:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("DvdId", dir);

                    // ViewModel.Playlist.PlaylistSortDescriptionUpdate("Genres", dir);
                    break;
                case 5:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("ReleaseDate", dir);
                    break;
                case 6:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("FileDate", dir);
                    break;
                case 7:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("Rating", dir);
                    break;
                case 8:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("RandomInt", dir);
                    break;
                default:
                    ViewModel.Playlist.PlaylistSortDescriptionUpdate("DvdId", dir);
                    break;
            }
        }

        private void SelectAllVideoCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)SelectAllVideoCheckBox?.IsChecked)
            {
                FileList.SelectAll();
            }
            else
            {
                FileList.UnselectAll();
            }
        }
    }
}