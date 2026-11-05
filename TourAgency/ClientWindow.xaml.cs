using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace TourAgency
{
    public partial class ClientWindow : Window
    {
        private readonly int _id;

        public ClientWindow(int id)
        {
            InitializeComponent();
            _id = id;
            Title = id == 0 ? "Новый клиент" : "Изменение клиента";
            if (id > 0)
            {
                LoadClient();
            }
        }

        private void LoadClient()
        {
            DataTable table = MainWindow.Query("SELECT * FROM KURS_Klient WHERE Kod_klienta = @id", new SqlParameter("@id", _id));
            if (table.Rows.Count == 0)
            {
                return;
            }
            DataRow row = table.Rows[0];
            txtFio.Text = row["FIO"].ToString();
            txtPassport.Text = row["Pasport"].ToString();
            txtPhone.Text = row["Telefon"].ToString();
            txtEmail.Text = row["Email"].ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (txtFio.Text.Trim().Length == 0 || txtPassport.Text.Trim().Length == 0)
            {
                MessageBox.Show("Введите ФИО и паспортные данные.");
                return;
            }
            try
            {
                if (_id == 0)
                {
                    MainWindow.Execute("INSERT INTO KURS_Klient (FIO, Pasport, Telefon, Email) VALUES (@fio, @passport, @phone, @email)", new SqlParameter("@fio", txtFio.Text.Trim()), new SqlParameter("@passport", txtPassport.Text.Trim()), new SqlParameter("@phone", txtPhone.Text.Trim()), new SqlParameter("@email", txtEmail.Text.Trim()));
                }
                else
                {
                    MainWindow.Execute("UPDATE KURS_Klient SET FIO = @fio, Pasport = @passport, Telefon = @phone, Email = @email WHERE Kod_klienta = @id", new SqlParameter("@fio", txtFio.Text.Trim()), new SqlParameter("@passport", txtPassport.Text.Trim()), new SqlParameter("@phone", txtPhone.Text.Trim()), new SqlParameter("@email", txtEmail.Text.Trim()), new SqlParameter("@id", _id));
                }
                DialogResult = true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Не удалось сохранить клиента: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
