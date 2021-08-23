// Copyright (C) 2021 Pavel Patrovský. Do not redistribute!

using System;
using System.Collections.Generic;
using FlaxEditor.Content;
using FlaxEditor.Scripting;
using FlaxEditor.Surface;
using FlaxEngine;

namespace AudioSystemFlax
{
    /// <summary>
    /// AudioGraphWindow Script.
    /// </summary>
    public class AudioGraphWindow : VisjectSurfaceWindow<JsonAsset, AudioGraphSurface, AudioGraphPreview>
    {
        /// <summary>
        /// The allowed parameter types.
        /// </summary>
        private readonly ScriptType[] _newParameterTypes =
        {
        new ScriptType(typeof(float)),
        new ScriptType(typeof(Vector2)),
        new ScriptType(typeof(Vector3)),
        new ScriptType(typeof(Vector4)),
    };

        /// <summary>
        /// The properties proxy object.
        /// </summary>
        private sealed class PropertiesProxy
        {
            [EditorOrder(1000), EditorDisplay("Parameters"), CustomEditor(typeof(ParametersEditor)), NoSerialize]
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public AudioGraphWindow Window { get; set; }

            [EditorOrder(20), EditorDisplay("General"), Tooltip("It's for demo purposes")]
            public int DemoInteger { get; set; }

            [HideInEditor, Serialize]
            public List<SurfaceParameter> Parameters
            {
                get => Window.Surface.Parameters;
                set => throw new Exception("No setter.");
            }

            /// <summary>
            /// Gathers parameters from the specified window.
            /// </summary>
            /// <param name="window">The window.</param>
            public void OnLoad(AudioGraphWindow window)
            {
                // Link
                Window = window;
            }

            /// <summary>
            /// Clears temporary data.
            /// </summary>
            public void OnClean()
            {
                // Unlink
                Window = null;
            }
        }

        private readonly PropertiesProxy _properties;

        private AudioGraph _assetInstance;

        /// <inheritdoc />
        public AudioGraphWindow(FlaxEditor.Editor editor, AssetItem item)
        : base(editor, item)
        {
            // Asset preview
            _preview = new AudioGraphPreview(true)
            {
                Parent = _split2.Panel1
            };

            // Asset properties proxy
            _properties = new PropertiesProxy();
            _propertiesEditor.Select(_properties);

            // Surface
            _surface = new AudioGraphSurface(this, Save, _undo)
            {
                Parent = _split1.Panel1,
                Enabled = false
            };

            // Toolstrip
            _toolstrip.AddSeparator();
            _toolstrip.AddButton(editor.Icons.Code64, () => ShowJson(_asset)).LinkTooltip("Show asset contents");
        }

        /// <summary>
        /// Shows the JSON contents window.
        /// </summary>
        /// <param name="asset">The JSON asset.</param>
        public static void ShowJson(JsonAsset asset)
        {
            FlaxEditor.Utilities.Utils.ShowSourceCodeWindow(asset.Data, "Asset JSON");
        }

        /// <inheritdoc />
        public override IEnumerable<ScriptType> NewParameterTypes => _newParameterTypes;

        /// <inheritdoc />
        protected override void UnlinkItem()
        {
            // Cleanup
            _properties.OnClean();
            _preview.ExpressionGraph = null;

            base.UnlinkItem();
        }

        /// <inheritdoc />
        protected override void OnAssetLinked()
        {
            // Setup
            _assetInstance = _asset.CreateInstance<AudioGraph>();
            
            _preview.ExpressionGraph = _assetInstance;

            base.OnAssetLinked();
        }

        /// <inheritdoc />
        public override string SurfaceName => "Expression Graph";

        /// <inheritdoc />
        public override byte[] SurfaceData
        {
            get => AudioGraphSurface.LoadSurface(_asset, _assetInstance, true);
            set
            {
                // Save data to the temporary asset
                if (AudioGraphSurface.SaveSurface(_asset, _assetInstance, value))
                {
                    // Error
                    _surface.MarkAsEdited();
                    Debug.LogError("Failed to save surface data");
                }
                // Optionally reset the preview
            }
        }

        /// <inheritdoc />
        protected override bool LoadSurface()
        {
            // Init asset properties and parameters proxy
            _properties.OnLoad(this);

            // Load surface graph
            if (_surface.Load())
            {
                // Error
                Debug.LogError("Failed to load expression graph surface.");
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        protected override bool SaveSurface()
        {
            // TODO: Graph compilation
            _surface.Save();
            return false;
        }

        /// <inheritdoc />
        public override void SetParameter(int index, object value)
        {
            // TODO: Update the asset value to have nice live preview
            //_assetInstance.Parameters[index].Value = value;

            base.SetParameter(index, value);
        }
    }
}
