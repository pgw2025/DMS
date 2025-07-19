using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;

namespace DMS.Infrastructure.Services
{
    public class MenuService : BaseService<MenuBean, DbMenu, MenuRepository>
    {
        public MenuService(IMapper mapper, MenuRepository repository) : base(mapper, repository)
        {
        }
    }
}
