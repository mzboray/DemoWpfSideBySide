using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlugInInterfaces;

namespace MyLib
{
    public class LibPlugIn : IPlugIn
    {
        public string Name => "MyPlugIn 1.0";

        public object GetControl()
        {
            return new UserControl1();
        }
    }
}
