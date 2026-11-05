using System.Windows;

namespace TourAgency
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            while (true)
            {
                var login = new LoginWindow();
                if (login.ShowDialog() != true)
                {
                    Shutdown();
                    return;
                }

                var main = new MainWindow();
                MainWindow = main;
                main.ShowDialog();

                if (!main.LogoutRequested)
                {
                    Shutdown();
                    return;
                }
            }
        }
    }
}
