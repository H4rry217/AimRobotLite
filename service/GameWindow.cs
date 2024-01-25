using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.service {
    public class GameWindow {

        public static IntPtr GetBfvHandle() {
            IntPtr intPtr = IntPtr.Zero;
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes) {
                IntPtr mainWindowHandle = process.MainWindowHandle;

                if (mainWindowHandle != IntPtr.Zero) {
                    if (process.ProcessName.Equals("bfv")) return mainWindowHandle;
                }
            }

            return intPtr;
        }

    }
}
