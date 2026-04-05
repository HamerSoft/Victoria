using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HamerSoft.Victoria.Core.Extractor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HamerSoft.Victoria.Ui.Elements
{
    internal class PreviewElement : VisualElement
    {
        private const int MAXIMUM_UNITY_UI_TEXT_LENGTH = 11000;
        private readonly UnityPackage _unityPackage;
        private Extractor.Node _node;
        private CancellationTokenSource _cancellationTokenSource;
        private VisualElement _previewElement;

        public PreviewElement(UnityPackage unityPackage)
        {
            _unityPackage = unityPackage;
            SetNode(unityPackage.Assets);
        }

        internal void SetNode(Extractor.Node node)
        {
            if (_node == node)
                return;
            _node = node;
            _unityPackage.AudioSource.Stop();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _ = ShowPreview(node, _cancellationTokenSource.Token);
        }

        private async Task ShowPreview(Extractor.Node node, CancellationToken token)
        {
            ClearPreview();
            if (node == null)
                _previewElement = ShowNoPreview();
            else if (node.IsLeaf)
            {
                _previewElement = ShowLoadingPreview(node as Extractor.Asset);
                if (!token.IsCancellationRequested)
                    _previewElement = await ShowFilePreview(node as Extractor.Asset, token);
            }
            else
                _previewElement = ShowDirectoryPreview(node);

            if (!token.IsCancellationRequested)
            {
                _previewElement.name = "preview";
                Add(_previewElement);
            }
            else
                _previewElement = null;
        }

        private VisualElement ShowLoadingPreview(Extractor.Asset node)
        {
            return new Label
            {
                text = $"Loading Preview for: {node.DetailedName}",
                style =
                {
                    flexGrow = 1,
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };
        }

        private VisualElement ShowNoPreview()
        {
            return new Label
            {
                text = "No preview available.",
                style =
                {
                    flexGrow = 1,
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold)
                }
            };
        }

        private async Task<VisualElement> ShowFilePreview(Extractor.Asset asset, CancellationToken token)
        {
            return asset.GetPreviewType() switch
            {
                Extractor.Asset.Preview.PlainText => await ShowTextContent(),
                Extractor.Asset.Preview.Image => await ShowImageContent(asset),
                Extractor.Asset.Preview.Audio => await ShowAudioPreview(asset),
                _ => ShowNoPreviewerAvailable()
            };

            async Task<VisualElement> ShowTextContent()
            {
                try
                {
                    var scrollView = new ScrollView
                    {
                        style =
                        {
                            flexGrow = 1
                        }
                    };
                    var text = await _unityPackage.LoadObject<string>(asset.Name, asset.FileContent,
                        Extractor.Asset.Preview.PlainText, _cancellationTokenSource.Token);
                    text = text.Length > MAXIMUM_UNITY_UI_TEXT_LENGTH
                        ? $"{text.Substring(0, MAXIMUM_UNITY_UI_TEXT_LENGTH)}\r\n...\r\nText is truncated..."
                        : text;
                    scrollView.Add(new Label(text)
                    {
                        style =
                        {
                            marginBottom = 5,
                            marginLeft = 5,
                            marginTop = 5,
                            marginRight = 5,
                            whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.Normal)
                        }
                    });
                    return scrollView;
                }
                catch (TaskCanceledException)
                {
                    // ignore this since something else is highlighted now
                }

                return null;
            }

            VisualElement ShowNoPreviewerAvailable()
            {
                return new Label
                {
                    text = $"No preview available for {asset.DetailedName}",
                    style =
                    {
                        flexGrow = 1,
                        unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                        unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                        whiteSpace = new StyleEnum<WhiteSpace>(WhiteSpace.Normal)
                    }
                };
            }
        }

        private async Task<VisualElement> ShowAudioPreview(Extractor.Asset asset)
        {
            var container = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column)
                }
            };

            var header = new VisualElement
            {
                name = "audio-header",
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                    height = 30,
                    marginLeft = 5,
                    marginBottom = 5,
                    marginRight = 5,
                    marginTop = 5
                }
            };
            header.Add(new Label(asset.DetailedName)
            {
                name = "audio-file-name",
                style =
                {
                    flexGrow = 1,
                    height = 20,
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleLeft),
                }
            });

            async void Clicked()
            {
                if (_unityPackage.AudioSource.IsPlaying)
                    _unityPackage.AudioSource.Stop();

                Debug.Log("Clicked play");
                // key must be unique, audio will have added image and clip. so some other unique need be
                var audioClip = await _unityPackage.LoadObject<AudioClip>(asset.DetailedName, asset.FileContent,
                    Extractor.Asset.Preview.Audio, _cancellationTokenSource.Token);

                if (audioClip)
                    _unityPackage.AudioSource.Play(audioClip);
                else
                    Debug.LogWarning($"Failed to play preview audio{asset.DetailedName}");
            }

            header.Add(new Button(Clicked)
            {
                name = "audio-play-button",
                text = "►",
                style =
                {
                    unityFontStyleAndWeight = new StyleEnum<FontStyle>(FontStyle.Bold),
                    height = 20,
                    width = 20
                }
            });
            container.Add(header);
            container.Add(await ShowImageContent(asset));
            return container;
        }

        private async Task<VisualElement> ShowImageContent(Extractor.Asset asset)
        {
            if (asset.PreviewContent?.Length == 0)
                return ShowNoPreview();
            try
            {
                var texture = await _unityPackage.LoadObject<Texture2D>(asset.Name, asset.PreviewContent,
                    Extractor.Asset.Preview.Image, _cancellationTokenSource.Token);

                if (texture == null)
                    return ShowNoPreview();

                return new Image
                {
                    style =
                    {
                        flexGrow = 1
                    },
                    image = texture
                };
            }
            catch (TaskCanceledException)
            {
                // ignore since something else is highlighted
            }

            return null;
        }

        private VisualElement ShowDirectoryPreview(Extractor.Node node)
        {
            var numberOfSubDirectories = node.Children.Count(n => n is Extractor.Folder);

            return new Label
            {
                text =
                    $"<b>Directory:</b>\r\n {node.Name}\r\n\n<b>SubDirectories:</b> {numberOfSubDirectories}\r\n<b>Files:</b> {node.Children.Count - numberOfSubDirectories}",
                style =
                {
                    flexGrow = 1,
                    unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter),
                }
            };
        }

        private void ClearPreview()
        {
            if (_previewElement != null && _previewElement.parent == contentContainer)
                Remove(_previewElement);
            _previewElement = null;
        }
    }
}