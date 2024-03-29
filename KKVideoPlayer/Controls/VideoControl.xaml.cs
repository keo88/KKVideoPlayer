﻿namespace KKVideoPlayer.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.SQLite;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Xaml;
    using KKVideoPlayer.Foundation;
    using KKVideoPlayer.ViewModels;

    /// <summary>
    /// VideoControl.xaml에 대한 상호 작용 논리.
    /// </summary>
    public partial class VideoControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoControl"/> class.
        /// </summary>
        public VideoControl()
        {
            InitializeComponent();
            App.ViewModel.VideoCtrl.ActorsControl = ActorsListControl;
            App.ViewModel.VideoCtrl.GenresControl = GenresListControl;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.VideoCtrl.ApplyVideoInfoChanges();

            App.ViewModel.VideoCtrl.ParentWindow.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.VideoCtrl.ParentWindow.Close();
        }

        private void DeformatButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.VideoCtrl.DeformatFilepath();
        }

        private void FormatButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.VideoCtrl.FormatFilepath();
        }

        private void WebCrawlButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.VideoCtrl.CrawlVideoInfoAsync();
        }

        private void AButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.VideoCtrl.OpenDvdIdWeb("https://www.avdbs.com/menu/search.php?kwd=SEARCH_TERM");
        }

        private void RButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.VideoCtrl.OpenDvdIdWeb("https://www.r18.com/common/search/searchword=SEARCH_TERM/");
        }

        private void ShowInExplorerButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.VideoCtrl.ShowItemInExplorer();
        }

        private async void CompleteDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string fp = UpdatedFilepath.Text;

            if (App.ViewModel.PlayingVideo != null && !App.ViewModel.PlayingVideo.Filepath.Equals(fp, StringComparison.Ordinal))
                await App.ViewModel.Commands.CloseCommand.ExecuteAsync(App.ViewModel.MediaElement);

            MessageBoxResult result = MessageBox.Show($"Are you sure?\nClick Ok to delete {fp}", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Cancel)
                return;

            List<string> ls = new List<string>();
            ls.Add(fp);
            App.ViewModel.Playlist.DeleteItemFromList(ls, true);
            App.ViewModel.VideoCtrl.ParentWindow.Close();
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return) return;
            App.ViewModel.VideoCtrl.ApplyActorsAndGenres();
        }
    }
}
