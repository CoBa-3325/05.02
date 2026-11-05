using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace TourAgency
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => txtPassword.Focus();
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;
            if (login.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("Введите логин и пароль.");
                return;
            }
            try
            {
                DataTable table = MainWindow.Query("SELECT Kod_sotrudnika, FIO, Rol, Kod_klienta FROM KURS_Sotrudnik WHERE Login = @login AND Parol = @parol", new SqlParameter("@login", login), new SqlParameter("@parol", HashPassword(password)));
                if (table.Rows.Count == 0)
                {
                    MessageBox.Show("Неверный логин или пароль.");
                    return;
                }
                DataRow row = table.Rows[0];
                MainWindow.SotrudnikId = Convert.ToInt32(row["Kod_sotrudnika"]);
                MainWindow.SotrudnikFio = row["FIO"].ToString();
                MainWindow.Rol = row["Rol"].ToString();
                MainWindow.KlientId = row["Kod_klienta"] == DBNull.Value ? 0 : Convert.ToInt32(row["Kod_klienta"]);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Нет связи с базой данных: " + ex.Message);
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("X2"));
                }
                return builder.ToString();
            }
        }
    }
}
