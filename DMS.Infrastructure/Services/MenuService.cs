using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;

namespace DMS.Infrastructure.Services
{
    public class MenuService : BaseService<MenuBean, MenuRepository>
    {
        public MenuService(MenuRepository repository) : base(repository)
        {
        }
    }
}
