using System.Diagnostics;
using AutoMapper;
using DMS.Core.Helper;
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

    public async Task<List<Core.Models.Device>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<Core.Models.Device>>(dbList);
    }

    public async Task<Core.Models.Device> GetByIdAsync(int id)
    {
        var dbDevice = await base.GetByIdAsync(id);
        return _mapper.Map<Core.Models.Device>(dbDevice);
    }

    public async Task<Core.Models.Device> AddAsync(Core.Models.Device model)
    {
        var dbDevice = await base.AddAsync(_mapper.Map<DbDevice>(model));
        return _mapper.Map(dbDevice, model);
    }

    public async Task<int> UpdateAsync(Core.Models.Device model) => await base.UpdateAsync(_mapper.Map<DbDevice>(model));


    public async Task<int> DeleteAsync(Core.Models.Device model) => await base.DeleteAsync(_mapper.Map<DbDevice>(model));

    public async Task<int> DeleteAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new DbDevice() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbDevice)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public new async Task<List<Core.Models.Device>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<Core.Models.Device>>(dbList);

    }
}