using AimRobot.Api;
using AimRobotLite.Properties;
using log4net.Config;
using System.Xml;

namespace AimRobotLite {
    static class Program {

        private static bool DEBUG_ENABLE = false;
        public static Form1 Winform;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {

#if DEBUG
            DEBUG_ENABLE = true;
#endif

            LoadLogger();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Control.CheckForIllegalCrossThreadCalls = false;

            Winform = new Form1();

            AimRobotLite.run();

            Application.ThreadException += new ThreadExceptionEventHandler(ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);

            Application.Run(Winform);
        }

        public static void LoadLogger() {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(Resources.log4net);
            XmlConfigurator.Configure(xmlDocument.DocumentElement);
        }

        public static bool IsDebug() {
            return DEBUG_ENABLE;
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            MessageBox.Show(e.ExceptionObject.ToString());
        }

        private static void ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
            Robot.GetInstance().GetLogger().Error(e.Exception.ToString());
        }

    }
}