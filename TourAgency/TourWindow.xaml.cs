using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TourAgency
{
    public partial class TourWindow : Window
    {
        private readonly int _id;
        private string _photoName = "";

        public TourWindow(int id)
        {
            InitializeComponent();
            _id = id;
            Title = id == 0 ? "Новый тур" : "Изменение тура";
            LoadCountries();
            if (id == 0)
            {
                dpStart.SelectedDate = DateTime.Today;
                dpEnd.SelectedDate = DateTime.Today.AddDays(7);
            }
            else
            {
                LoadTour();
            }
        }

        private void LoadCountries()
        {
            DataTable table = MainWindow.Query("SELECT Kod_strany, Strana FROM KURS_Strana ORDER BY Strana");
            cmbCountry.ItemsSource = table.DefaultView;
            cmbCountry.DisplayMemberPath = "Strana";
            cmbCountry.SelectedValuePath = "Kod_strany";
            cmbCountry.SelectedIndex = 0;
        }

        private void Country_Changed(object sender, SelectionChangedEventArgs e)
        {
            LoadCities(0);
        }

        private void LoadCities(int cityId)
        {
            if (cmbCountry.SelectedValue == null)
            {
                return;
            }
            DataTable table = MainWindow.Query("SELECT Kod_goroda, Gorod FROM KURS_Gorod WHERE Kod_strany = @s ORDER BY Gorod", new SqlParameter("@s", (int)cmbCountry.SelectedValue));
            cmbCity.ItemsSource = table.DefaultView;
            cmbCity.DisplayMemberPath = "Gorod";
            cmbCity.SelectedValuePath = "Kod_goroda";
            if (cityId > 0)
            {
                cmbCity.SelectedValue = cityId;
            }
            else if (table.Rows.Count > 0)
            {
                cmbCity.SelectedIndex = 0;
            }
        }

        private void LoadTour()
        {
            DataTable table = MainWindow.Query("SELECT t.*, g.Kod_strany FROM KURS_Tur t JOIN KURS_Gorod g ON g.Kod_goroda = t.Kod_goroda WHERE t.Kod_tura = @id", new SqlParameter("@id", _id));
            if (table.Rows.Count == 0)
            {
                return;
            }
            DataRow row = table.Rows[0];
            txtName.Text = row["Nazvanie"].ToString();
            cmbCountry.SelectedValue = Convert.ToInt32(row["Kod_strany"]);
            LoadCities(Convert.ToInt32(row["Kod_goroda"]));
            dpStart.SelectedDate = Convert.ToDateTime(row["Data_nachala"]);
            dpEnd.SelectedDate = Convert.ToDateTime(row["Data_okonchaniya"]);
            txtPrice.Text = Convert.ToDecimal(row["Stoimost"]).ToString("0.##");
            txtSeats.Text = row["Kolichestvo_mest"].ToString();

            _photoName = row["Foto"] == DBNull.Value ? "" : row["Foto"].ToString();
            ShowPhoto();
        }

        private void Photo_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    Directory.CreateDirectory(dir);
                    string ext = Path.GetExtension(dialog.FileName);
                    string name = "photo_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ext;
                    string dest = Path.Combine(dir, name);
                    File.Copy(dialog.FileName, dest, true);
                    _photoName = name;
                    ShowPhoto();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Не удалось загрузить фото: " + ex.Message);
                }
            }
        }

        private void PickPhoto_Click(object sender, RoutedEventArgs e)
        {
            var window = new PhotoPickerWindow { Owner = this };
            if (window.ShowDialog() == true && window.SelectedName.Length > 0)
            {
                _photoName = window.SelectedName;
                ShowPhoto();
            }
        }

        private void RemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            _photoName = "";
            imgPreview.Source = null;
            lblPhoto.Text = "";
        }

        private void ShowPhoto()
        {
            if (_photoName.Length == 0)
            {
                imgPreview.Source = null;
                lblPhoto.Text = "";
                return;
            }
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", _photoName);
            if (File.Exists(path))
            {
                imgPreview.Source = new BitmapImage(new Uri(path));
                lblPhoto.Text = _photoName;
            }
            else
            {
                lblPhoto.Text = _photoName + " (файл отсутствует)";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text.Trim().Length == 0)
            {
                MessageBox.Show("Введите название тура.");
                return;
            }
            if (cmbCity.SelectedValue == null)
            {
                MessageBox.Show("Выберите город.");
                return;
            }
            if (!dpStart.SelectedDate.HasValue || !dpEnd.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите даты поездки.");
                return;
            }
            if (dpEnd.SelectedDate.Value <= dpStart.SelectedDate.Value)
            {
                MessageBox.Show("Дата окончания должна быть позже даты начала.");
                return;
            }
            decimal price;
            if (!decimal.TryParse(txtPrice.Text, out price) || price <= 0)
            {
                MessageBox.Show("Стоимость должна быть положительным числом.");
                return;
            }
            int seats;
            if (!int.TryParse(txtSeats.Text, out seats) || seats < 0)
            {
                MessageBox.Show("Количество мест должно быть целым неотрицательным числом.");
                return;
            }

            object foto = _photoName.Length == 0 ? (object)DBNull.Value : _photoName;

            try
            {
                if (_id == 0)
                {
                    MainWindow.Execute("INSERT INTO KURS_Tur (Kod_goroda, Nazvanie, Data_nachala, Data_okonchaniya, Stoimost, Kolichestvo_mest, Foto) VALUES (@city, @name, @start, @end, @price, @seats, @foto)", new SqlParameter("@city", (int)cmbCity.SelectedValue), new SqlParameter("@name", txtName.Text.Trim()), new SqlParameter("@start", dpStart.SelectedDate.Value), new SqlParameter("@end", dpEnd.SelectedDate.Value), new SqlParameter("@price", price), new SqlParameter("@seats", seats), new SqlParameter("@foto", foto));
                }
                else
                {
                    MainWindow.Execute("UPDATE KURS_Tur SET Kod_goroda = @city, Nazvanie = @name, Data_nachala = @start, Data_okonchaniya = @end, Stoimost = @price, Kolichestvo_mest = @seats, Foto = @foto WHERE Kod_tura = @id", new SqlParameter("@city", (int)cmbCity.SelectedValue), new SqlParameter("@name", txtName.Text.Trim()), new SqlParameter("@start", dpStart.SelectedDate.Value), new SqlParameter("@end", dpEnd.SelectedDate.Value), new SqlParameter("@price", price), new SqlParameter("@seats", seats), new SqlParameter("@foto", foto), new SqlParameter("@id", _id));
                }
                DialogResult = true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Не удалось сохранить тур: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
