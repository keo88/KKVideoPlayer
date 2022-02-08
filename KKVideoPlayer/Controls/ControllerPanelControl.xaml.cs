namespace KKVideoPlayer.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.SQLite;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using KKVideoPlayer;
    using ViewModels;

    /// <summary>
    /// Interaction logic for ControllerPanelControl.xaml.
    /// </summary>
    public partial class ControllerPanelControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerPanelControl"/> class.
        /// </summary>
        public ControllerPanelControl()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                HighlightsScrollViewer.PageLeft();
            else
                HighlightsScrollViewer.PageRight();
            e.Handled = true;
        }

        private void Highlight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock highlightText = (TextBlock)((Grid)sender).FindName("HighlightPosition");
            App.ViewModel.MediaElement.Position = TimeSpan.Parse(highlightText.Text);
        }

        private void Highlight_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ObservableCollection<HighlightEntry> highlights = App.ViewModel.Controller.Highlights;
            TextBlock highlightUid = (TextBlock)((Grid)sender).FindName("HighlightUid");

            foreach (HighlightEntry highlight in highlights)
            {
                if (highlight.HighlightUid == long.Parse(highlightUid.Text))
                {
                    highlights.RemoveAt(highlights.IndexOf(highlight));

                    using (SQLiteConnection conn = new SQLiteConnection("Data Source=" + App.ViewModel.CurrentVideoDirectory + "/deepdark.db"))
                    {
                        conn.Open();

                        using (SQLiteTransaction tr = conn.BeginTransaction())
                        {
                            string sql = $"DELETE FROM Favorites WHERE uid = {highlight.HighlightUid}";
                            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                            cmd.ExecuteNonQuery();
                            tr.Commit();
                        }

                        conn.Close();
                    }

                    return;
                }
            }

            MessageBox.Show("Failed to remove the highlight.");
        }

        private void ThumbnailButton_Click(object sender, RoutedEventArgs e)
        {
            App.ViewModel.Controller.TakeThumbnail();
        }
    }
}
