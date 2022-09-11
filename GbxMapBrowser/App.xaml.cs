using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GbxMapBrowser
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Console.WriteLine("Arguments: ");
            foreach(var item in e.Args)
            {
                if(item is not null)
                Console.WriteLine(item);
            }
        }
    }
}
