// Copyright (C) 2021 Pavel Patrovský. Do not redistribute!

using System;
using System.Collections.Generic;
using FlaxEditor.Content;
using FlaxEditor.Windows;
using FlaxEngine;

namespace AudioSystemFlax
{
    /// <summary>
    /// AudioGraphProxy Script.
    /// </summary>
    public class AudioGraphProxy : JsonAssetProxy
    {
        /// <inheritdoc />
        public override string Name => "Audio Graph";

        /// <inheritdoc />
        // This will be implemented in the next step...
        public override EditorWindow Open(FlaxEditor.Editor editor, ContentItem item)
        {
            return new AudioGraphWindow(editor, (JsonAssetItem)item);
        }

        /// <inheritdoc />
        public override Color AccentColor => Color.FromRGB(0xb32b96);

        /// <inheritdoc />
        public override string TypeName { get; } = typeof(AudioGraph).FullName;

        /// <inheritdoc />
        public override bool CanCreate(ContentFolder targetLocation)
        {
            return targetLocation.CanHaveAssets;
        }

        /// <inheritdoc />
        public override void Create(string outputPath, object arg)
        {
            FlaxEditor.Editor.SaveJsonAsset(outputPath, new AudioGraph());
        }
    }
}
