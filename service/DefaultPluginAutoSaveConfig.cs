using AimRobot.Api;
using AimRobot.Api.config;
using AimRobot.Api.plugin;
using IniParser;
using IniParser.Model;
using System.Text;

namespace AimRobotLite.service {
    public class DefaultPluginAutoSaveConfig : AutoSaveConfig {

        private PluginBase pluginBase;

        private FileIniDataParser parser = new FileIniDataParser();
        private IniData iniData;

        private FileInfo fileInfo;

        private object lockObject = new object();
        private int writeCount = 0;

        private const int WRITE_LIMIT = 15;

        public DefaultPluginAutoSaveConfig(PluginBase pluginBase, string configName) : base(configName) {
            this.pluginBase = pluginBase;

            string dir = Path.Combine(Robot.GetInstance().GetDirectory(), "configs", pluginBase.GetPluginName());
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            FileInfo file = new FileInfo(Path.Combine(dir, configName));
            if (!file.Exists) File.Create(file.FullName).Close();

            this.iniData = parser.ReadFile(file.FullName, Encoding.Default);
            this.fileInfo = file;
        }

        private void IncreaseWriteCount(Action writeAction) {
            lock (this.lockObject) {
                writeAction();
                this.writeCount++;

                if (this.writeCount >= WRITE_LIMIT) {
                    Save();
                    this.writeCount = 0;
                }
            }
        }

        public override void DelData(string key) {
            IncreaseWriteCount(() => this.iniData[pluginBase.GetPluginName()].RemoveKey(key));
        }

        public override string GetData(string key) {
            return this.iniData[pluginBase.GetPluginName()][key];
        }

        public override FileInfo GetFile() {
            return fileInfo;
        }

        public override void Save() {
            this.parser.WriteFile(this.fileInfo.FullName, this.iniData, Encoding.Default);
        }

        public override void SetData(string key, string value) {
            IncreaseWriteCount(() => this.iniData[pluginBase.GetPluginName()][key] = value);
        }

        public override int GetSize(string key) {
            return this.iniData[pluginBase.GetPluginName()].Count;
        }

        public override bool hasData(string key) {
            return this.iniData[pluginBase.GetPluginName()].ContainsKey(key);
        }
    }
}
