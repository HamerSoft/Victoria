using System;
using System.IO;
using HamerSoft.Victoria.Core.Audio;
using HamerSoft.Victoria.Ui.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui
{
    public class Victoria
    {
        private const string VICTORIA = "Victoria";
        private const string IMPORTS = "Imports";

        public static VictoriaRuntimeImporter Create(VisualElement parent, string source)
        {
            var destination = Path.Combine(Application.persistentDataPath, VICTORIA, IMPORTS);
            return Create(parent, source, destination);
        }

        public static VictoriaRuntimeImporter Create(VisualElement parent, string source, string destination)
        {
            var destinationInfo = new DirectoryInfo(destination);
            if (!destinationInfo.Exists)
                destinationInfo.Create();

            var unityPackage = UnityPackage.LoadFromPath(new FileInfo(source), new RuntimeAudioSource());
            return new VictoriaRuntimeImporter(unityPackage, parent, destinationInfo);
        }

        public class VictoriaRuntimeImporter : VisualElement, IDisposable
        {
            private readonly UnityPackage _unityPackage;
            private readonly VictoriaElement _victoriaElement;

            internal VictoriaRuntimeImporter(UnityPackage unityPackage, VisualElement parent,
                DirectoryInfo destination)
            {
                name = "victoria-importer";
                _unityPackage = unityPackage;
                style.flexGrow = 1;
                style.backgroundColor = new StyleColor(new Color(0.235f, 0.235f, 0.235f));
                styleSheets.Add(Resources.Load<StyleSheet>("VictoriaEditorDark"));
                var header = new VisualElement
                {
                    name = "header",
                    style =
                    {
                        height = 20,
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween,
                        alignItems = Align.Center,
                    }
                };
                header.Add(new Label("Victoria! Whatever, I do what I want!")
                {
                    style = { unityTextAlign = TextAnchor.MiddleCenter },
                    name = "header-label",
                });
                header.Add(new Button(Close)
                {
                    text = "X",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleCenter,
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                });

                Add(header);
                _victoriaElement = new VictoriaElement(unityPackage, destination, Close);
                Add(_victoriaElement);
                parent.Add(this);
            }

            private void Close()
            {
                Dispose();
                parent?.Remove(this);
            }

            public void Dispose()
            {
                _unityPackage.Dispose();
                _victoriaElement.Destroy();
            }
        }
    }
}