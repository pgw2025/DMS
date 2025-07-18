using DMS.Models;

namespace DMS.Helper;

public class MenuHelper
{
    public static void MenuAddParent(MenuBean menu)
    {
        if (menu.Items==null || menu.Items.Count==0)
            return;
        foreach (MenuBean menuItem in menu.Items)
        {
            menuItem.Parent=menu;
            if (menuItem.Items!= null && menuItem.Items.Count>0)
            {
                MenuAddParent(menuItem);
            }
        }
    }
}