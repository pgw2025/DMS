using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ViewModels;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DMS.WPF.UnitTests.ViewModelTest
{
    public class DevicesViewModelTests:BaseServiceTest
    {
        private readonly DevicesViewModel _devicesViewModel;


        public DevicesViewModelTests()
        {
          _devicesViewModel=  ServiceProvider.GetRequiredService<DevicesViewModel>();
        }

        [Fact]
        public async Task AddDevice_Test()
        {
            // Arrange
            
            // Act
            await _devicesViewModel.AddDevice();

         
        }

        
    }
}