using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace TourAgency
{
    public partial class MainWindow : Window
    {
        public static string ConnectionString = "Server=stud-mssql.sttec.yar.ru,38325;Database=user87_db;User Id=user87_db;Password=user87;";
        public static int SotrudnikId = 1;
        public static string SotrudnikFio = "";
        public static string Rol = "Менеджер";
        public static int KlientId = 0;
        public bool LogoutRequested = false;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Туристические агентства - " + SotrudnikFio + " (" + Rol + ")";
            txtUser.Text = SotrudnikFio + " (" + Rol + ")";
            dpFrom.SelectedDate = new DateTime(2026, 6, 1);
            dpTo.SelectedDate = new DateTime(2026, 8, 31);
            dpReportFrom.SelectedDate = new DateTime(2026, 1, 1);
            dpReportTo.SelectedDate = DateTime.Today;
            try
            {
                LoadCountries();
                LoadTours();
                if (Rol == "Клиент")
                {
                    LoadMyRequests();
                }
                else
                {
                    LoadClients();
                    LoadRequestLists();
                    LoadRequests();
                }
                if (Rol == "Администратор")
                {
                    LoadEmployees();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Нет связи с базой данных: " + ex.Message);
            }
            ApplyRole();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            LogoutRequested = true;
            Close();
        }

        private void ApplyRole()
        {
            bool isClient = Rol == "Клиент";
            bool isAdmin = Rol == "Администратор";
            tabClients.Visibility = isClient ? Visibility.Collapsed : Visibility.Visible;
            tabRequests.Visibility = isClient ? Visibility.Collapsed : Visibility.Visible;
            tabReports.Visibility = isClient ? Visibility.Collapsed : Visibility.Visible;
            tabEmployees.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            tabMyRequests.Visibility = isClient ? Visibility.Visible : Visibility.Collapsed;
            panelToursEdit.Visibility = isClient ? Visibility.Collapsed : Visibility.Visible;
            btnRequestTour.Visibility = isClient ? Visibility.Visible : Visibility.Collapsed;
        }

        public static DataTable Query(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddRange(parameters);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }

        public static void Execute(string sql, params SqlParameter[] parameters)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddRange(parameters);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadCountries()
        {
            DataTable table = Query("SELECT Kod_strany, Strana FROM KURS_Strana ORDER BY Strana");
            DataRow row = table.NewRow();
            row["Kod_strany"] = 0;
            row["Strana"] = "Все страны";
            table.Rows.InsertAt(row, 0);
            cmbCountry.ItemsSource = table.DefaultView;
            cmbCountry.DisplayMemberPath = "Strana";
            cmbCountry.SelectedValuePath = "Kod_strany";
            cmbCountry.SelectedIndex = 0;
        }

        private void LoadTours()
        {
            string sql = "SELECT t.Kod_tura AS [Код], t.Foto AS [Foto], t.Nazvanie AS [Название], s.Strana AS [Страна], g.Gorod AS [Город], CONVERT(varchar(10), t.Data_nachala, 104) AS [Начало], CONVERT(varchar(10), t.Data_okonchaniya, 104) AS [Окончание], FORMAT(t.Stoimost, N'N0') AS [Стоимость], t.Kolichestvo_mest AS [Мест], t.Kolichestvo_mest - ISNULL((SELECT COUNT(*) FROM KURS_Zayavka z WHERE z.Kod_tura = t.Kod_tura AND z.Kod_statusa <> 4), 0) AS [Свободно] FROM KURS_Tur t JOIN KURS_Gorod g ON g.Kod_goroda = t.Kod_goroda JOIN KURS_Strana s ON s.Kod_strany = g.Kod_strany WHERE 1 = 1";
            if (cmbCountry.SelectedValue != null)
            {
                int country = (int)cmbCountry.SelectedValue;
                if (country > 0)
                {
                    sql += " AND s.Kod_strany = " + country;
                }
            }
            if (dpFrom.SelectedDate.HasValue)
            {
                sql += " AND t.Data_nachala >= '" + dpFrom.SelectedDate.Value.ToString("yyyy-MM-dd") + "'";
            }
            if (dpTo.SelectedDate.HasValue)
            {
                sql += " AND t.Data_nachala <= '" + dpTo.SelectedDate.Value.ToString("yyyy-MM-dd") + "'";
            }
            decimal price;
            if (decimal.TryParse(txtPrice.Text, out price))
            {
                sql += " AND t.Stoimost <= " + price.ToString().Replace(',', '.');
            }
            sql += " ORDER BY t.Data_nachala";
            DataTable table = Query(sql);
            table.Columns.Add("FotoPath", typeof(string));
            foreach (DataRow row in table.Rows)
            {
                string foto = row["Foto"] == DBNull.Value ? "" : row["Foto"].ToString();
                row["FotoPath"] = foto.Length == 0 ? null : System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", foto);
            }
            dgTours.ItemsSource = table.DefaultView;
        }

        private void FindTours_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadTours();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выборки туров: " + ex.Message);
            }
        }

        private void AddTour_Click(object sender, RoutedEventArgs e)
        {
            TourWindow window = new TourWindow(0) { Owner = this };
            if (window.ShowDialog() == true)
            {
                LoadTours();
            }
        }

        private void EditTour_Click(object sender, RoutedEventArgs e)
        {
            int id = SelectedId(dgTours, "Код");
            if (id == 0)
            {
                MessageBox.Show("Выберите тур в таблице.");
                return;
            }
            TourWindow window = new TourWindow(id) { Owner = this };
            if (window.ShowDialog() == true)
            {
                LoadTours();
            }
        }

        private void DeleteTour_Click(object sender, RoutedEventArgs e)
        {
            int id = SelectedId(dgTours, "Код");
            if (id == 0)
            {
                MessageBox.Show("Выберите тур в таблице.");
                return;
            }
            int used = Convert.ToInt32(Query("SELECT COUNT(*) FROM KURS_Zayavka WHERE Kod_tura = @id", new SqlParameter("@id", id)).Rows[0][0]);
            if (used > 0)
            {
                MessageBox.Show("Нельзя удалить тур: по нему есть заявки.");
                return;
            }
            if (MessageBox.Show("Удалить выбранный тур?", "Туры", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            Execute("DELETE FROM KURS_Tur WHERE Kod_tura = @id", new SqlParameter("@id", id));
            LoadTours();
        }

        private void RequestTour_Click(object sender, RoutedEventArgs e)
        {
            int id = SelectedId(dgTours, "Код");
            if (id == 0)
            {
                MessageBox.Show("Выберите тур в таблице.");
                return;
            }
            if (CreateRequest(KlientId, id))
            {
                LoadMyRequests();
                LoadTours();
            }
        }

        private static int SelectedId(DataGrid grid, string column)
        {
            DataRowView row = grid.SelectedItem as DataRowView;
            if (row == null)
            {
                return 0;
            }
            return Convert.ToInt32(row[column]);
        }

        private void LoadClients()
        {
            string sql = "SELECT Kod_klienta AS [Код], FIO AS [ФИО], Pasport AS [Паспорт], Telefon AS [Телефон], Email AS [Почта] FROM KURS_Klient";
            string search = txtClientSearch.Text.Trim();
            if (search.Length > 0)
            {
                sql += " WHERE FIO LIKE @q OR Telefon LIKE @q";
            }
            sql += " ORDER BY FIO";
            dgClients.ItemsSource = Query(sql, new SqlParameter("@q", "%" + search + "%")).DefaultView;
        }

        private void FindClients_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadClients();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выборки клиентов: " + ex.Message);
            }
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            ClientWindow window = new ClientWindow(0) { Owner = this };
            if (window.ShowDialog() == true)
            {
                LoadClients();
                LoadRequestLists();
            }
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            int id = SelectedId(dgClients, "Код");
            if (id == 0)
            {
                MessageBox.Show("Выберите клиента в таблице.");
                return;
            }
            ClientWindow window = new ClientWindow(id) { Owner = this };
            if (window.ShowDialog() == true)
            {
                LoadClients();
                LoadRequestLists();
            }
        }

        private void LoadRequestLists()
        {
            DataTable clients = Query("SELECT Kod_klienta, FIO FROM KURS_Klient ORDER BY FIO");
            cmbClient.ItemsSource = clients.DefaultView;
            cmbClient.DisplayMemberPath = "FIO";
            cmbClient.SelectedValuePath = "Kod_klienta";
            cmbClient.SelectedIndex = clients.Rows.Count > 0 ? 0 : -1;
            DataTable tours = Query("SELECT t.Kod_tura, t.Nazvanie + N', ' + g.Gorod + N' (' + CAST(t.Kolichestvo_mest - ISNULL((SELECT COUNT(*) FROM KURS_Zayavka z WHERE z.Kod_tura = t.Kod_tura AND z.Kod_statusa <> 4), 0) AS nvarchar(10)) + N' мест)' AS Nazvanie FROM KURS_Tur t JOIN KURS_Gorod g ON g.Kod_goroda = t.Kod_goroda ORDER BY t.Data_nachala");
            cmbTour.ItemsSource = tours.DefaultView;
            cmbTour.DisplayMemberPath = "Nazvanie";
            cmbTour.SelectedValuePath = "Kod_tura";
            cmbTour.SelectedIndex = tours.Rows.Count > 0 ? 0 : -1;
        }

        private void LoadRequests()
        {
            dgRequests.ItemsSource = Query("SELECT z.Kod_zayavki AS [Номер], k.FIO AS [Клиент], t.Nazvanie AS [Тур], st.Status AS [Статус], CONVERT(varchar(10), z.Data_podachi, 104) AS [Дата], FORMAT(z.Summa, N'N0') AS [Сумма] FROM KURS_Zayavka z JOIN KURS_Klient k ON k.Kod_klienta = z.Kod_klienta JOIN KURS_Tur t ON t.Kod_tura = z.Kod_tura JOIN KURS_Status st ON st.Kod_statusa = z.Kod_statusa ORDER BY z.Kod_zayavki DESC").DefaultView;
        }

        private void LoadMyRequests()
        {
            dgMyRequests.ItemsSource = Query("SELECT z.Kod_zayavki AS [Номер], t.Nazvanie AS [Тур], s.Strana AS [Страна], CONVERT(varchar(10), t.Data_nachala, 104) AS [Начало], st.Status AS [Статус], CONVERT(varchar(10), z.Data_podachi, 104) AS [Дата заявки], FORMAT(z.Summa, N'N0') AS [Сумма] FROM KURS_Zayavka z JOIN KURS_Tur t ON t.Kod_tura = z.Kod_tura JOIN KURS_Gorod g ON g.Kod_goroda = t.Kod_goroda JOIN KURS_Strana s ON s.Kod_strany = g.Kod_strany JOIN KURS_Status st ON st.Kod_statusa = z.Kod_statusa WHERE z.Kod_klienta = @id ORDER BY z.Kod_zayavki DESC", new SqlParameter("@id", KlientId)).DefaultView;
        }

        private void RefreshRequests_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadRequestLists();
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выборки заявок: " + ex.Message);
            }
        }

        private void RefreshMyRequests_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadMyRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выборки заявок: " + ex.Message);
            }
        }

        private void CreateRequest_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClient.SelectedValue == null || cmbTour.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента и тур.");
                return;
            }
            if (CreateRequest((int)cmbClient.SelectedValue, (int)cmbTour.SelectedValue))
            {
                LoadRequestLists();
                LoadRequests();
                LoadTours();
            }
        }

        private bool CreateRequest(int clientId, int tourId)
        {
            DataTable tour = Query("SELECT Stoimost, Kolichestvo_mest - ISNULL((SELECT COUNT(*) FROM KURS_Zayavka z WHERE z.Kod_tura = @id AND z.Kod_statusa <> 4), 0) AS Svobodno FROM KURS_Tur WHERE Kod_tura = @id", new SqlParameter("@id", tourId));
            if (tour.Rows.Count == 0)
            {
                MessageBox.Show("Тур не найден.");
                return false;
            }
            if (Convert.ToInt32(tour.Rows[0]["Svobodno"]) <= 0)
            {
                MessageBox.Show("На выбранный тур нет свободных мест.");
                return false;
            }
            decimal price = Convert.ToDecimal(tour.Rows[0]["Stoimost"]);
            try
            {
                Execute("INSERT INTO KURS_Zayavka (Kod_klienta, Kod_tura, Kod_sotrudnika, Kod_statusa, Data_podachi, Summa) VALUES (@client, @tour, @sotrudnik, 1, CAST(GETDATE() AS date), @sum)", new SqlParameter("@client", clientId), new SqlParameter("@tour", tourId), new SqlParameter("@sotrudnik", SotrudnikId), new SqlParameter("@sum", price));
                MessageBox.Show("Заявка оформлена.");
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Не удалось оформить заявку: " + ex.Message);
                return false;
            }
        }

        private void LoadEmployees()
        {
            dgEmployees.ItemsSource = Query("SELECT Kod_sotrudnika AS [Код], Login AS [Логин], FIO AS [ФИО], Rol AS [Роль], Dolzhnost AS [Должность], Telefon AS [Телефон] FROM KURS_Sotrudnik ORDER BY Rol, FIO").DefaultView;
        }

        private void RefreshEmployees_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка выборки сотрудников: " + ex.Message);
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            EmployeeWindow window = new EmployeeWindow(0) { Owner = this };
            if (window.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            int id = SelectedId(dgEmployees, "Код");
            if (id == 0)
            {
                MessageBox.Show("Выберите пользователя в таблице.");
                return;
            }
            EmployeeWindow window = new EmployeeWindow(id) { Owner = this };
            if (window.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void ConfirmRequest_Click(object sender, RoutedEventArgs e) => ChangeStatus(2);
        private void CancelRequest_Click(object sender, RoutedEventArgs e) => ChangeStatus(4);

        private void PayMyRequest_Click(object sender, RoutedEventArgs e)
        {
            int id = SelectedId(dgMyRequests, "Номер");
            if (id == 0)
            {
                MessageBox.Show("Выберите заявку в таблице.");
                return;
            }
            DataTable request = Query("SELECT Kod_statusa, Summa FROM KURS_Zayavka WHERE Kod_zayavki = @id AND Kod_klienta = @client", new SqlParameter("@id", id), new SqlParameter("@client", KlientId));
            if (request.Rows.Count == 0)
            {
                MessageBox.Show("Заявка не найдена.");
                return;
            }
            int status = Convert.ToInt32(request.Rows[0]["Kod_statusa"]);
            if (status == 3)
            {
                MessageBox.Show("Заявка уже оплачена.");
                return;
            }
            if (status == 4)
            {
                MessageBox.Show("Заявка отменена, оплатить её нельзя.");
                return;
            }
            decimal sum = Convert.ToDecimal(request.Rows[0]["Summa"]);
            if (MessageBox.Show("Оплатить заявку №" + id + " на сумму " + sum.ToString("N0") + " ₽?", "Оплата", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            Execute("UPDATE KURS_Zayavka SET Kod_statusa = 3 WHERE Kod_zayavki = @id", new SqlParameter("@id", id));
            MessageBox.Show("Заявка оплачена.");
            LoadMyRequests();
            LoadTours();
        }

        private void ChangeStatus(int statusId)
        {
            int id = SelectedId(dgRequests, "Номер");
            if (id == 0)
            {
                MessageBox.Show("Выберите заявку в таблице.");
                return;
            }
            DataTable current = Query("SELECT Kod_statusa FROM KURS_Zayavka WHERE Kod_zayavki = @id", new SqlParameter("@id", id));
            if (current.Rows.Count == 0)
            {
                MessageBox.Show("Заявка не найдена.");
                return;
            }
            int currentStatus = Convert.ToInt32(current.Rows[0]["Kod_statusa"]);
            if (statusId == 2 && currentStatus == 3)
            {
                MessageBox.Show("Заявка уже оплачена клиентом.");
                return;
            }
            if (statusId == 2 && currentStatus == 4)
            {
                MessageBox.Show("Заявка отменена.");
                return;
            }
            if (statusId == 4 && currentStatus == 4)
            {
                MessageBox.Show("Заявка уже отменена.");
                return;
            }
            string action = statusId == 2 ? "Подтвердить" : statusId == 3 ? "Оплатить" : "Отменить";
            if (MessageBox.Show(action + " выбранную заявку?", "Заявки", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }
            Execute("UPDATE KURS_Zayavka SET Kod_statusa = @s WHERE Kod_zayavki = @id", new SqlParameter("@s", statusId), new SqlParameter("@id", id));
            LoadRequests();
            LoadTours();
        }

        private void BuildReport_Click(object sender, RoutedEventArgs e)
        {
            if (!dpReportFrom.SelectedDate.HasValue || !dpReportTo.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите период.");
                return;
            }
            try
            {
                DataTable table = Query("SELECT s.Strana AS [Страна], g.Gorod AS [Город], t.Nazvanie AS [Тур], COUNT(*) AS [Заявок], FORMAT(SUM(z.Summa), N'N0') AS [Сумма] FROM KURS_Zayavka z JOIN KURS_Tur t ON t.Kod_tura = z.Kod_tura JOIN KURS_Gorod g ON g.Kod_goroda = t.Kod_goroda JOIN KURS_Strana s ON s.Kod_strany = g.Kod_strany WHERE z.Data_podachi BETWEEN @from AND @to AND z.Kod_statusa <> 4 GROUP BY s.Strana, g.Gorod, t.Nazvanie ORDER BY SUM(z.Summa) DESC", new SqlParameter("@from", dpReportFrom.SelectedDate.Value), new SqlParameter("@to", dpReportTo.SelectedDate.Value));
                dgReport.ItemsSource = table.DefaultView;
                decimal total = 0;
                int count = 0;
                foreach (DataRow row in table.Rows)
                {
                    total += Convert.ToDecimal(row["Сумма"].ToString().Replace(" ", "").Replace("\u00A0", ""));
                    count += Convert.ToInt32(row["Заявок"]);
                }
                tbTotal.Text = table.Rows.Count == 0 ? "За выбранный период данных нет." : string.Format("Итого: заявок {0} на сумму {1:N0} ₽", count, total);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отчёта: " + ex.Message);
            }
        }
    }
}
