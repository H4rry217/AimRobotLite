using AimRobot.Api.plugin;
using System.Reflection;
using System.Resources;

namespace AimRobotLite.plugin {
    public class CSharpPluginLoader : IPluginLoader {

        public IPlugin LoadPlugin(string filePath) {
            IPlugin plugin = null;

            if (filePath.EndsWith(".dll")) {
                Assembly assembly = Assembly.LoadFile(filePath);
                Type[] types = assembly.GetTypes();

                string mainClass = string.Empty;

                foreach (var type in types) {
                    if (type.Name == "Resources") {
                        ResourceManager resourceManager = new ResourceManager(type.FullName, assembly);

                        mainClass = resourceManager.GetString("MainClass");
                    }
                }

                if (!string.Empty.Equals(mainClass)) {
                    Type pluginClass = assembly.GetType(mainClass);

                    plugin = (IPlugin)Activator.CreateInstance(pluginClass);
                }
            }

            return plugin;

        }

    }
}
