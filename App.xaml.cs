using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PMSWPF.Data.Repositories;
using PMSWPF.Services;
using PMSWPF.ViewModels;
using PMSWPF.Views;

namespace PMSWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App()
        {
            var container = new ServiceCollection();
            container.AddSingleton<NavgatorServices>();
            container.AddSingleton<DevicesRepositories>();
            container.AddSingleton<IDeviceDialogService, DeviceDialogService>();
            container.AddSingleton<INotificationService, GrowlNotificationService>();
            container.AddSingleton<MainViewModel>();
            container.AddSingleton<HomeViewModel>();
            container.AddSingleton<DevicesViewModel>();
            container.AddSingleton<DataTransformViewModel>();
            container.AddSingleton<MainView>(dp => new MainView()
                { DataContext = dp.GetRequiredService<MainViewModel>() });
            container.AddSingleton<HomeView>();
            container.AddSingleton<DevicesView>();
            container.AddSingleton<DataTransformViewModel>();

            Services = container.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            CheckDb();
            
            MainWindow = Services.GetRequiredService<MainView>();
            MainWindow.Show();
        }

        private void CheckDb()
        {
            
        }

        // [STAThread]
        // static void Main(string[] args)
        // {
        //     using IHost host = CreateHostBuilder(args).Build();
        //     host.Start();
        //     App app = new App();
        //     app.InitializeComponent();
        //     app.MainWindow = host.Services.GetRequiredService<MainView>();
        //     app.MainWindow.Visibility = Visibility.Visible;
        //     app.Run();
        //
        // }
        //
        // private static IHostBuilder CreateHostBuilder(string[] args)
        // {
        //     return Host.CreateDefaultBuilder(args).ConfigureServices(services =>
        //     {
        //
        //         services.AddHostedService<DemoBackgroundService>();
        //         services.AddSingleton<MainView>();
        //         services.AddSingleton<MainViewModel>();
        //     });
        // }
    }
}