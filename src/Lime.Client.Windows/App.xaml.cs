using Lime.Client.Windows.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Lime.Protocol.Pcl.Compatibility;
using System.Reflection;

namespace Lime.Client.Windows
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomainWrapper.Instance = new AppDomainWrapperInstance(); 
        }
    }

    class AppDomainWrapperInstance : IAppDomain
    {
        public IList<Assembly> GetAssemblies()
        {
            return System.AppDomain.CurrentDomain.GetAssemblies();
        }
    }   

}
