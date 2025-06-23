using Microsoft.Extensions.Hosting;

namespace PMSWPF.Services;

internal class DemoBackgroundService : BackgroundService
{
    private int count = 0;


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // while (!stoppingToken.IsCancellationRequested)
        // {
        //     await Task.Delay(1000);
        //     count += 1;
        //     var msg = new MyMessage(35) { Count = count };
        //     WeakReferenceMessenger.Default.Send<MyMessage>(msg);
        //     Console.WriteLine("Hello");
        // }
    }
}