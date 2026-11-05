using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace TourAgency
{
    public partial class EmployeeWindow : Window
    {
        private readonly int _id;

        public EmployeeWindow(int id)
        {
            InitializeComponent();
            _id = id;
            Title = id == 0 ? "Новый пользователь" : "Изменение пользователя";
            cmbRol.ItemsSource = new[] { "Клиент", "Менеджер", "Администратор" };
            cmbRol.SelectedIndex = 1;
            cmbRol.SelectionChanged += Rol_Changed;
            List<string> positions = new List<string> { "Руководитель отдела", "Менеджер по туризму", "Старший менеджер", "Туроператор", "Бухгалтер", "Клиент" };
            DataTable dbPositions = MainWindow.Query("SELECT DISTINCT Dolzhnost FROM KURS_Sotrudnik WHERE Dolzhnost IS NOT NULL AND LTRIM(RTRIM(Dolzhnost)) <> N''");
            foreach (DataRow row in dbPositions.Rows)
            {
                string position = row[0].ToString().Trim();
                if (position.Length > 0 && !positions.Contains(position))
                {
                    positions.Add(position);
                }
            }
            cmbDolzhnost.ItemsSource = positions;
            cmbDolzhnost.SelectedIndex = 1;
            DataTable clients = MainWindow.Query("SELECT Kod_klienta, FIO FROM KURS_Klient ORDER BY FIO");
            cmbKlient.ItemsSource = clients.DefaultView;
            cmbKlient.DisplayMemberPath = "FIO";
            cmbKlient.SelectedValuePath = "Kod_klienta";
            cmbKlient.SelectedIndex = clients.Rows.Count > 0 ? 0 : -1;
            if (id > 0)
            {
                LoadEmployee();
            }
        }

        private void Rol_Changed(object sender, SelectionChangedEventArgs e)
        {
            panelClient.Visibility = (string)cmbRol.SelectedItem == "Клиент" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadEmployee()
        {
            DataTable table = MainWindow.Query("SELECT * FROM KURS_Sotrudnik WHERE Kod_sotrudnika = @id", new SqlParameter("@id", _id));
            if (table.Rows.Count == 0)
            {
                return;
            }
            DataRow row = table.Rows[0];
            txtFio.Text = row["FIO"].ToString();
            cmbDolzhnost.SelectedItem = row["Dolzhnost"].ToString();
            txtPhone.Text = row["Telefon"].ToString();
            txtLogin.Text = row["Login"].ToString();
            cmbRol.SelectedItem = row["Rol"].ToString();
            if (row["Kod_klienta"] != DBNull.Value)
            {
                cmbKlient.SelectedValue = Convert.ToInt32(row["Kod_klienta"]);
            }
            lblPassword.Text = "Новый пароль (оставьте пустым, чтобы не менять)";
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (txtFio.Text.Trim().Length == 0 || txtLogin.Text.Trim().Length == 0)
            {
                MessageBox.Show("Введите ФИО и логин.");
                return;
            }
            string rol = cmbRol.SelectedItem as string;
            if (rol == null)
            {
                MessageBox.Show("Выберите роль.");
                return;
            }
            string dolzh = cmbDolzhnost.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(dolzh))
            {
                MessageBox.Show("Выберите должность.");
                return;
            }
            if (_id == 0 && txtPassword.Password.Length == 0)
            {
                MessageBox.Show("Введите пароль.");
                return;
            }
            object klient = DBNull.Value;
            if (rol == "Клиент")
            {
                if (cmbKlient.SelectedValue == null)
                {
                    MessageBox.Show("Для роли «Клиент» выберите клиента.");
                    return;
                }
                klient = (int)cmbKlient.SelectedValue;
            }
            try
            {
                if (_id == 0)
                {
                    MainWindow.Execute("INSERT INTO KURS_Sotrudnik (Login, Parol, FIO, Dolzhnost, Telefon, Rol, Kod_klienta) VALUES (@login, @parol, @fio, @dolzh, @phone, @rol, @klient)", new SqlParameter("@login", txtLogin.Text.Trim()), new SqlParameter("@parol", LoginWindow.HashPassword(txtPassword.Password)), new SqlParameter("@fio", txtFio.Text.Trim()), new SqlParameter("@dolzh", dolzh), new SqlParameter("@phone", txtPhone.Text.Trim()), new SqlParameter("@rol", rol), new SqlParameter("@klient", klient));
                }
                else if (txtPassword.Password.Length > 0)
                {
                    MainWindow.Execute("UPDATE KURS_Sotrudnik SET Login = @login, Parol = @parol, FIO = @fio, Dolzhnost = @dolzh, Telefon = @phone, Rol = @rol, Kod_klienta = @klient WHERE Kod_sotrudnika = @id", new SqlParameter("@login", txtLogin.Text.Trim()), new SqlParameter("@parol", LoginWindow.HashPassword(txtPassword.Password)), new SqlParameter("@fio", txtFio.Text.Trim()), new SqlParameter("@dolzh", dolzh), new SqlParameter("@phone", txtPhone.Text.Trim()), new SqlParameter("@rol", rol), new SqlParameter("@klient", klient), new SqlParameter("@id", _id));
                }
                else
                {
                    MainWindow.Execute("UPDATE KURS_Sotrudnik SET Login = @login, FIO = @fio, Dolzhnost = @dolzh, Telefon = @phone, Rol = @rol, Kod_klienta = @klient WHERE Kod_sotrudnika = @id", new SqlParameter("@login", txtLogin.Text.Trim()), new SqlParameter("@fio", txtFio.Text.Trim()), new SqlParameter("@dolzh", dolzh), new SqlParameter("@phone", txtPhone.Text.Trim()), new SqlParameter("@rol", rol), new SqlParameter("@klient", klient), new SqlParameter("@id", _id));
                }
                DialogResult = true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Не удалось сохранить: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
