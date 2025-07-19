using System.Diagnostics;
using AutoMapper;
using DMS.Infrastructure.Entities;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using SqlSugar;
using DMS.Infrastructure.Interfaces;

namespace DMS.Infrastructure.Repositories;

public class DeviceRepository : BaseRepository<DbDevice>
{

    public DeviceRepository(ITransaction transaction)
        : base(transaction)
    {
        
    }


    public override async Task<List<DbDevice>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var dlist = await Db.Queryable<DbDevice>()
                            .Includes(d => d.VariableTables, dv => dv.Device)
                            .Includes(d => d.VariableTables, dvd => dvd.Variables, data => data.VariableTable)
                            .Includes(d => d.VariableTables, vt => vt.Variables, v => v.VariableMqtts)
                            .ToListAsync();

        stopwatch.Stop();
        NlogHelper.Info($"加载设备列表总耗时：{stopwatch.ElapsedMilliseconds}ms");
        return dlist;
    }

    

    
}