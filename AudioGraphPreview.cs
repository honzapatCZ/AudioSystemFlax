// Copyright (C) 2021 Pavel Patrovský. Do not redistribute!

using System;
using System.Collections.Generic;
using FlaxEditor.Viewport.Previews;
using FlaxEngine;

namespace AudioSystemFlax
{
    /// <summary>
    /// AudioGraphPreview Script.
    /// </summary>
    public class AudioGraphPreview : AssetPreview
    {
        // Preview will be expanded later
        public AudioGraphPreview(bool useWidgets) : base(useWidgets)
        {
        }

        public AudioGraph ExpressionGraph { get; set; }
    }
}
