using System;
using System.Diagnostics;

namespace Kachuwa.Dash.Model
{
    public static class ProcessHelper
    {
        public static bool IsRunning(this Process process)
        {
            if (process == null)
                throw new ArgumentNullException("process");

            try
            {
                process.Refresh();  // Important
                Process.GetProcessById(process.Id);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public static Process GetProcessById(int id)
        {
            try
            {
                var process = System.Diagnostics.Process.GetProcessById(id);
                return process;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}