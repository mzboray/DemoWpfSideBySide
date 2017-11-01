using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlugInInterfaces
{
    public interface IPlugIn
    {
        string Name { get; }

        object GetControl();
    }
}
