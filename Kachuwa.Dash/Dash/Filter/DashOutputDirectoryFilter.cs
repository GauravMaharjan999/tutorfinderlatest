using System;
using System.Collections.Generic;
using System.Text;
using Kachuwa.Dash.Filters;

namespace Kachuwa.Dash.Dash.Filter
{
    
    public class DashOutputDirectoryFilter : BaseDashFilter
    {
        public string Directory { get; private set; }

       
        public DashOutputDirectoryFilter(string directory)
        {
            Name = "DashOutputDirector";
            Rank = 8;
            Directory = directory;
        }

        public override string ToString()
        {

            return string.Format(" --o {0}", Directory);
        }
    }
    public class DashFileNameFilter : BaseDashFilter
    {
        public string Name { get; private set; }


        public DashFileNameFilter(string name)
        {
            Name = "DashFileName";
            Rank = 8;
            Name = name;
        }

        public override string ToString()
        {

            return string.Format(" --mpd-name {0} --force", Name);
        }
    }
}
