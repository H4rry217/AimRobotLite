using AimRobot.Api;
using AimRobot.Api.command;
using AimRobot.Api.events;
using AimRobot.Api.events.ev;
using AimRobot.Api.plugin;
using AimRobotLite.service.robotplugin;
using System.Reflection;
using static AimRobot.Api.command.ICommandListener;
using System.Text;
using AimRobotLite.events;
using AimRobotLite.commands;
using AimRobot.Api.config;
using AimRobotLite.service;

namespace AimRobotLite.plugin{
    public class RobotPluginManager : PluginManager {

        protected Dictionary<string, PluginBase> Plugins = new Dictionary<string, PluginBase>();
        protected ISet<IEventListener> EventListeners = new HashSet<IEventListener>();

        protected Dictionary<PluginBase, CommandList> PluginCommands = new Dictionary<PluginBase, CommandList>();

        protected Dictionary<Type, MethodList> EventMethods = new Dictionary<Type, MethodList>();

        protected ISet<IDataset<string, string>> AutoSaveData = new HashSet<IDataset<string, string>>();

        private System.Threading.Timer _AutoSaveTimer;

        public RobotPluginManager(Robot robot) : base(robot) {
            this._AutoSaveTimer = new System.Threading.Timer(_AutoSave, null, 0, 120 * 1000);
        }

        public override void CallEvent(RobotEvent robotEvent) {
            if (robot.IsEnable()) {
                if (robotEvent is PlayerChatEvent chatEvent) {
                    CheckCommand(chatEvent.speaker, chatEvent.message);
                }

                if (EventMethods.TryGetValue(robotEvent.GetType(), out MethodList methodList)) {
                    methodList.InvokeAll(robotEvent);
                }
            }
        }

        public override void CheckCommand(string sender, string content) {
            if (content.StartsWith(ICommandListener.CMD_SIGN)) {
                string part2 = content.Substring(ICommandListener.CMD_SIGN.Length);

                StringBuilder keywordBuilder = new StringBuilder();
                CommandStyle commandStyle = CommandStyle.Unknown;

                int equalSignIndex = 0;
                int greaterThanIndex = 0;

                Dictionary<string, string> paramMap = new Dictionary<string, string>();

                for (int i = 0; i < part2.Length; i++) {
                    char chr = part2[i];

                    if (chr == '=') {
                        commandStyle = CommandStyle.Style1;
                        equalSignIndex = i;
                    } else if (chr == '>') {
                        greaterThanIndex = i;
                        commandStyle = CommandStyle.Style1;
                    } else if (chr == ' ') {
                        commandStyle = CommandStyle.Style2;
                    } else {
                        keywordBuilder.Append(chr);
                        continue;
                    }

                    break;
                }

                string keyword = keywordBuilder.ToString();

                switch (commandStyle) {
                    case CommandStyle.Style1:

                        if (greaterThanIndex == 0) {
                            paramMap["param"] = part2.Substring(equalSignIndex + 1);
                        } else {
                            StringBuilder toBuilder = new StringBuilder();

                            for (int i = greaterThanIndex + 1; i < part2.Length; i++) {
                                if (part2[i] == '=') {
                                    paramMap["param"] = part2.Substring(i + 1);
                                    paramMap["to"] = toBuilder.ToString();
                                } else {
                                    toBuilder.Append(part2[i]);
                                }
                            }

                        }

                        break;
                    case CommandStyle.Style2:
                        var argMap = Utils.getArgs(part2.Substring(part2.IndexOf(' ') == -1 ? part2.Length : part2.IndexOf(' ')));
                        foreach (var item in argMap) paramMap[item.Key] = item.Value;
                        break;
                }

                CommandData commandData = new CommandData(paramMap, sender);

                StringBuilder logBuilder = new StringBuilder($"Command ({keyword}) ARGS -> ");

                if (paramMap.Count > 0) {
                    foreach (var kv in paramMap) logBuilder.Append($"\"{kv.Key}\": \"{kv.Value}\"").Append("   ");
                }

                Robot.GetInstance().GetLogger().Debug(content);
                Robot.GetInstance().GetLogger().Debug(logBuilder.ToString());

                CallCommmand(keyword, commandData);

            }
        }

        public override bool DisablePlugin(PluginBase plugin) {
            if (this.Plugins.TryGetValue(plugin.GetPluginName(), out PluginBase loadPlugin)) {
                if (loadPlugin.IsEnable()) {
                    loadPlugin.SetEnable(false);

                    loadPlugin.OnDisable();
                    this.robot.GetLogger().Info($"插件 {plugin.GetPluginName()} 已卸载");

                    return true;
                }
            }

            return false;
        }

        public override bool EnablePlugin(PluginBase plugin) {
            if (this.Plugins.TryGetValue(plugin.GetPluginName(), out PluginBase loadPlugin)) {
                if (!loadPlugin.IsEnable()) {
                    loadPlugin.SetEnable(true);

                    loadPlugin.OnEnable();
                    this.robot.GetLogger().Info($"插件 {plugin.GetPluginName()} 已启用");

                    return true;
                }
            }

            return false;
        }

        public override PluginBase GetPlugin(string pluginName) {
            return Plugins[pluginName];
        }

        public override ISet<PluginBase> GetPlugins() {
            return new HashSet<PluginBase>(this.Plugins.Values);
        }

        public override void LoadPlugin(PluginBase plugin) {
            if (!this.Plugins.ContainsKey(plugin.GetPluginName())) {
                plugin.OnLoad();

                this.Plugins.Add(plugin.GetPluginName(), plugin);
                this.robot.GetLogger().Info($"插件 {plugin.GetPluginName()} 加载");

            } else if (Plugins.TryGetValue(plugin.GetPluginName(), out PluginBase loadedPlugin)) {
                this.robot.GetLogger().Warn($"重复加载同一插件 {loadedPlugin.GetPluginName()}! 已加载版本: {loadedPlugin.GetVersion()} 未加载版本: {plugin.GetVersion()}");
            }
        }

        public void LoadPlugins(){
            string[] dllFiles = Directory.GetFiles(Path.Combine(robot.GetDirectory(), "plugins"));

            IPluginLoader pluginLoader = new CSharpPluginLoader();
            foreach (string dllFile in dllFiles){
                IPlugin plugin = pluginLoader.LoadPlugin(dllFile);

                if (plugin != null && plugin is PluginBase pluginBase){
                    pluginBase.Init(robot, pluginLoader);
                    LoadPlugin(pluginBase);
                    EnablePlugin(pluginBase);
                }

            }

            /************/
            var arlDefaultPlugin = new AimRobotDefaultPlugin();
            arlDefaultPlugin.Init(robot, null);
            LoadPlugin(arlDefaultPlugin);
            EnablePlugin(arlDefaultPlugin);

            var arlLogPlugin = new FormLoggerPlugin();
            arlDefaultPlugin.Init(robot, null);
            LoadPlugin(arlLogPlugin);
            EnablePlugin(arlLogPlugin);
            /************/

            Program.Winform.refreshPluginListBox();
        }

        public override void CallCommmand(string keyword, CommandData commandData) {
            foreach (var item in PluginCommands.Values){
                item.InvokeCommand(keyword, commandData);
            }
        }

        public override void RegisterCommandListener(PluginBase plugin, ICommandListener commandListener) {
            if (plugin == null) return;

            CommandList commandList = PluginCommands.ContainsKey(plugin) ? PluginCommands[plugin] : new CommandList(plugin);
            commandList.RegisterCommand(commandListener);
            PluginCommands[plugin] = commandList;
        }


        public override void RegisterListener(PluginBase plugin, IEventListener eventListener) {
            if (plugin == null) return;
            var type = eventListener.GetType();

            MethodInfo[] methods = type.GetMethods();
            Type robotEventBase = typeof(RobotEvent);

            if (!EventListeners.Contains(eventListener)) {
                foreach (var method in methods) {
                    if (Attribute.IsDefined(method, typeof(AimRobot.Api.events.EventHandler))) {

                        ParameterInfo[] parameters = method.GetParameters();
                        if (parameters.Length == 1) {

                            var paramType = parameters[0].ParameterType;
                            if (robotEventBase.IsAssignableFrom(paramType)) {
                                EventMethod eventMethod = new EventMethod(method, eventListener, plugin);

                                MethodList methodList = EventMethods.ContainsKey(paramType)? EventMethods[paramType] : new MethodList();
                                methodList.RegisterMethod(eventMethod);

                                EventMethods[paramType] = methodList;
                            }

                        }
                    }
                }

                EventListeners.Add(eventListener);
            }
        }

        public override void UnregisterCommandListener(ICommandListener commandListener) {
            foreach (var item in PluginCommands){
                item.Value.UnregisterCommand(commandListener);
            }
        }

        public override void UnregisterListener(IEventListener eventListener) {
            EventListeners.Remove(eventListener);

            foreach (var item in EventMethods) {
                item.Value.UnregisterMethod(eventListener);
            }
        }

        public override AutoSaveConfig GetDefaultAutoSaveConfig(PluginBase plugin, string configName) {
            if(plugin == null) return null;
            return new DefaultPluginAutoSaveConfig(plugin, configName);
        }

        public override void ConfigAutoSave<K, V>(IDataset<K, V> config) {
            if(typeof(K) == typeof(string) && typeof(V) == typeof(string)) {
                AutoSaveData.Add((IDataset<string, string>)config);
            }
        }

        public void _AutoSave(object state) {
            foreach (var item in AutoSaveData) item.Save();

            this.robot.GetLogger().Debug($"{AutoSaveData.Count} files autosave");
        }
    }
}
