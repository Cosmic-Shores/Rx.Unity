#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Rx.Unity.InternalUtil {
    public class SetIl2cppSettings : IPreprocessBuildWithReport {
        public int callbackOrder => 100;

        public void OnPreprocessBuild(BuildReport report) {
            PlayerSettings.SetAdditionalIl2CppArgs("--maximum-recursive-generic-depth=9 --generic-virtual-method-iterations=3");
        }
    }
}
#endif
