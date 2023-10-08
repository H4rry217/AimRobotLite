using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.common.logger {
    class FormOutputAppender : AppenderSkeleton {
        protected override void Append(LoggingEvent loggingEvent) { 
            Program.Winform.ConsoleTextBoxAppend(loggingEvent);
        }
    }
}
