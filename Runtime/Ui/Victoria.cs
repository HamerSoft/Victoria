using System;
using System.IO;
using HamerSoft.Victoria.Core.Audio;
using HamerSoft.Victoria.Ui.Elements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui
{
    /// <summary>
    /// Static factory for creating Victoria importer instances at runtime.
    /// Provides convenience methods for embedding the importer UI into an existing
    /// <see cref="VisualElement"/> hierarchy without needing to manage a Unity Editor window.
    /// </summary>
    public class Victoria
    {
        private const string VICTORIA = "Victoria";
        private const string IMPORTS = "Imports";

        /// <summary>
        /// Creates a <see cref="VictoriaRuntimeImporter"/> using the default import destination
        /// (<c>Application.persistentDataPath/Victoria/Imports</c>).
        /// </summary>
        /// <param name="parent">The <see cref="VisualElement"/> to attach the importer UI to.</param>
        /// <param name="source">Absolute path to the <c>.unitypackage</c> file to import.</param>
        /// <returns>A fully initialised <see cref="VictoriaRuntimeImporter"/> added to <paramref name="parent"/>.</returns>
        public static VictoriaRuntimeImporter Create(VisualElement parent, string source)
        {
            var destination = Path.Combine(Application.persistentDataPath, VICTORIA, IMPORTS);
            return Create(parent, source, destination);
        }

        /// <summary>
        /// Creates a <see cref="VictoriaRuntimeImporter"/> targeting the specified destination directory,
        /// creating it on disk if it does not already exist.
        /// </summary>
        /// <param name="parent">The <see cref="VisualElement"/> to attach the importer UI to.</param>
        /// <param name="source">Absolute path to the <c>.unitypackage</c> file to import.</param>
        /// <param name="destination">Absolute path to the directory where assets will be written.</param>
        /// <returns>A fully initialised <see cref="VictoriaRuntimeImporter"/> added to <paramref name="parent"/>.</returns>
        public static VictoriaRuntimeImporter Create(VisualElement parent, string source, string destination)
        {
            var destinationInfo = new DirectoryInfo(destination);
            if (!destinationInfo.Exists)
                destinationInfo.Create();

            var unityPackage = UnityPackage.LoadFromPath(new FileInfo(source), new RuntimeAudioSource());
            return new VictoriaRuntimeImporter(unityPackage, parent, destinationInfo);
        }

        /// <summary>
        /// A self-contained <see cref="VisualElement"/> that renders the full Victoria import UI at runtime.
        /// Owns the lifecycle of the loaded <see cref="UnityPackage"/> and the underlying
        /// <see cref="VictoriaElement"/>; dispose to release all cached Unity objects.
        /// </summary>
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
                var styleSheet = Resources.Load<StyleSheet>("VictoriaEditorDark");
                if (styleSheet)
                    styleSheets.Add(styleSheet);

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
                    name = "close-button",
                    text = "X",
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleCenter,
                        unityFontStyleAndWeight = FontStyle.Bold
                    }
                });

                RegisterCallback<NavigationMoveEvent>(e =>
                {
                    e.StopPropagation();
                    e.PreventDefault();
                }, TrickleDown.TrickleDown);

                Add(header);
                _victoriaElement = new VictoriaElement(unityPackage, destination, Close);
                Add(_victoriaElement);
                parent.Add(this);
            }

            internal void Close()
            {
                Dispose();
                RemoveFromHierarchy();
            }

            /// <summary>
            /// Disposes the loaded <see cref="UnityPackage"/> — destroying all cached Unity objects —
            /// and tears down the <see cref="VictoriaElement"/> UI.
            /// </summary>
            public void Dispose()
            {
                _unityPackage.Dispose();
                _victoriaElement.Destroy();
            }
        }
    }
}