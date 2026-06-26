using LocationHeatMap.Services;
using LocationHeatMap.Views;

namespace LocationHeatMap
{
    public partial class App : Application
    {
        public App(DatabaseService db, MainView mainView)
        {
            InitializeComponent();

            MainPage = new NavigationPage(mainView)
            {
                BarBackgroundColor = Color.FromArgb("#6200EE"),
                BarTextColor = Colors.White
            };

            // Init database in background
            Task.Run(async () =>
            {
                try { await db.InitAsync(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"DB Init error: {ex.Message}");
                }
            });
        }
    }
}