using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        [STAThread]
        static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            host.Start();
            App app = new App();
            app.InitializeComponent();
            app.MainWindow = host.Services.GetRequiredService<MainView>();
            app.MainWindow.Visibility = Visibility.Visible;
            app.Run();

        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureServices(services =>
            {

                services.AddHostedService<DemoBackgroundService>();
                services.AddSingleton<MainView>();
                services.AddSingleton<MainViewModel>();
            });
        }
    }

}
