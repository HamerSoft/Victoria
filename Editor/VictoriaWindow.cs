using System.IO;
using HamerSoft.Victoria.EditorAudio;
using HamerSoft.Victoria.Ui.Elements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Editor
{
    /// <summary>
    /// Victoria editor window which renders the importer
    /// </summary>
    public class VictoriaWindow : EditorWindow
    {
        private const string UNITY_PACKAGE = "unitypackage";
        private static VictoriaWindow _instance;
        private VisualElement _root;
        private UnityPackage _unityPackage;
        private VictoriaElement _victoria;

        /// <summary>
        /// Entry point for showing the import window in the editor.
        /// </summary>
        [MenuItem("Tools/HamerSoft/Victoria/Import Package")]
        public static void SpawnWindow()
        {
            var selectedPackage = EditorUtility.OpenFilePanel($"Select .{UNITY_PACKAGE}", "", UNITY_PACKAGE);
            if (string.IsNullOrWhiteSpace(selectedPackage))
            {
                Debug.LogError("[Victoria] Failed to select package.");
            }
            else
            {
                if (_instance != null)
                    CloseWindow();
                var unityPackage = UnityPackage.LoadFromPath(new FileInfo(selectedPackage), new EditorAudioSource());
                _instance = ShowVictoriaWindow(unityPackage);
            }
        }

        private static void CloseWindow()
        {
            if (_instance == null)
                return;

            _instance.OnDestroy();
            _instance.Close();
            _instance = null;
        }

        private static VictoriaWindow ShowVictoriaWindow(UnityPackage unityPackage)
        {
            var wnd = GetWindow<VictoriaWindow>();
            wnd.titleContent = new GUIContent("Whatever, I do what I want!");
            wnd.Initialize(unityPackage);
            return wnd;
        }

        private void OnDestroy()
        {
            _victoria?.Destroy();
            _unityPackage = null;
        }

        private void Initialize(UnityPackage unityPackage)
        {
            _unityPackage = unityPackage;
            _root = rootVisualElement;
            _root.Add(_victoria = new VictoriaElement(_unityPackage,
                new DirectoryInfo(Path.Combine(Application.dataPath, "..")), () =>
                {
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif
                    CloseWindow();
                }) { style = { flexGrow = 1 } });
        }
    }
}