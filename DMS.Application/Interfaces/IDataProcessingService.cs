using DMS.Core.Models;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces;

public interface IDataProcessingService
{
    Task EnqueueAsync(Variable variable);
}