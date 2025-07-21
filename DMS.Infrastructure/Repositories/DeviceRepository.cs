using AutoMapper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

public class DeviceRepository : BaseRepository<DbDevice>, IDeviceRepository
{
    private readonly IMapper _mapper;

    public DeviceRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<List<Device>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<Device>>(dbList);
    }

    public async Task<Device> GetByIdAsync(int id)
    {
        var dbDevice = await base.GetByIdAsync(id);
        return _mapper.Map<Device>(dbDevice);
    }

    public async Task<Device> AddAsync(Device model)
    {
        var dbDevice = await base.AddAsync(_mapper.Map<DbDevice>(model));
        return _mapper.Map(dbDevice, model);
    }

    public async Task<int> UpdateAsync(Device model) => await base.UpdateAsync(_mapper.Map<DbDevice>(model));


    public async Task<int> DeleteAsync(Device model) => await base.DeleteAsync(_mapper.Map<DbDevice>(model));
}