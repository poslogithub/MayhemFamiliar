using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayhemFamiliar
{
    internal static class Util
    {
        public static Boolean IsProcessRunning(string processName)
        {
            // プロセス名を小文字に変換して比較
            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                // プロセス名が一致するか確認
                if (String.Compare(process.ProcessName, processName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
