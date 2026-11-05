using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TourAgency
{
    public partial class PhotoPickerWindow : Window
    {
        public string SelectedName { get; private set; } = "";

        public PhotoPickerWindow()
        {
            InitializeComponent();

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
            var items = new List<PhotoItem>();

            if (Directory.Exists(dir))
            {
                var extensions = new[] { ".png", ".jpg", ".jpeg" };
                foreach (string file in Directory.GetFiles(dir).Where(f => extensions.Contains(Path.GetExtension(f).ToLower())))
                {
                    try
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.DecodePixelWidth = 180;
                        image.UriSource = new Uri(file);
                        image.EndInit();
                        items.Add(new PhotoItem { Name = Path.GetFileName(file), Preview = image });
                    }
                    catch
                    {
                        // пропускаем повреждённые файлы
                    }
                }
            }

            items = items.OrderBy(p => p.Name).ToList();
            lst.ItemsSource = items;
            if (items.Count > 0)
            {
                lst.SelectedIndex = 0;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (lst.SelectedItem is PhotoItem item)
            {
                SelectedName = item.Name;
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void List_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lst.SelectedItem is PhotoItem item)
            {
                SelectedName = item.Name;
                DialogResult = true;
            }
        }
    }

    public class PhotoItem
    {
        public string Name { get; set; }
        public BitmapImage Preview { get; set; }
    }
}
