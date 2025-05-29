using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Hosting;
using PMSWPF.Message;

namespace PMSWPF.Services
{
    internal class DemoBackgroundService : BackgroundService
    {
        int count = 0;

        public DemoBackgroundService()
        {

        }



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000);
                count += 1;
                var msg = new MyMessage(35) { Count = count };
                WeakReferenceMessenger.Default.Send<MyMessage>(msg);
                Console.WriteLine("Hello");
            }
        }
    }
}
