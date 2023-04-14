using HibernatingRhinos.Profiler.Appender.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Intelligent.OTC.WebApi
{
    public class DbProfilerConfig
    {
        public static void RegisterEntityFrameworkProfiler( )
        {
            try
            {
                EntityFrameworkProfiler.Initialize();
            }
            catch(Exception ex)
            {

            }
        }
    }
}