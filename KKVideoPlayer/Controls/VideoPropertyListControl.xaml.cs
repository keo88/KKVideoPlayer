namespace KKVideoPlayer.Controls
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using KKVideoPlayer.Foundation;
    using KKVideoPlayer.Models;

    public partial class VideoPropertyListControl : UserControl
    {
        public ObservableDictionary<string, VideoPropertyItem> PropItemsDict { get; set; }

        public IEnumerable<string> VideoProperties => PropItemsDict.Keys;

        public VideoPropertyListControl()
        {
            PropItemsDict = new ObservableDictionary<string, VideoPropertyItem>();
            InitializeComponent();
        }

        public void AddItem(string itemName)
        {
            if (!PropItemsDict.ContainsKey(itemName))
            {
                PropItemsDict.Add(itemName, new VideoPropertyItem(itemName, () =>
                {
                    DeleteItem(itemName);
                }));
            }
        }

        public void AddItems(IEnumerable<string> items)
        {
            foreach (string item in items)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;
                AddItem(item);
            }
        }

        public void ResetItems(IEnumerable<string> items)
        {
            ClearItems();
            AddItems(items);
        }

        public void DeleteItem(string itemName)
        {
            if (PropItemsDict.Keys.Contains(itemName))
            {
                PropItemsDict.Remove(itemName);
            }
        }

        public void ClearItems()
        {
            PropItemsDict.Clear();
        }
    }
}