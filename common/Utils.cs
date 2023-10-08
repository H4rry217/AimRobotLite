using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimRobotLite.common {
    class Utils {

        public static int CompareVersions(string version1, string version2) {
            string[] version1Parts = version1.Split('.');
            string[] version2Parts = version2.Split('.');
            int maxLength = Math.Max(version1Parts.Length, version2Parts.Length);

            for (int i = 0; i < maxLength; i++) {
                int version1Part = i < version1Parts.Length ? int.Parse(version1Parts[i]) : 0;
                int version2Part = i < version2Parts.Length ? int.Parse(version2Parts[i]) : 0;

                if (version1Part < version2Part) {
                    return -1;
                } else if (version1Part > version2Part) {
                    return 1;
                }
            }

            return 0;
        }

    }
}
