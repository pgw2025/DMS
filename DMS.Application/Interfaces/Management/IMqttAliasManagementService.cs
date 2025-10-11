using DMS.Core.Models;

namespace DMS.Application.Interfaces.Management;

public interface IMqttAliasManagementService
{
    Task<MqttAlias> AssignAliasAsync(MqttAlias alias);
    Task<int> UpdateAsync(MqttAlias alias);
    Task<bool> DeleteAsync(int id);
    Task<List<MqttAlias>> LoadAllMqttAliasAsync();
}
