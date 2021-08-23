// Copyright (C) 2021 Pavel Patrovský. Do not redistribute!

using System;
using System.Collections.Generic;
using FlaxEditor;
using FlaxEngine;

namespace AudioSystemFlax
{
    /// <summary>
    /// AudioGraphPlugin Script.
    /// </summary>
    public class AudioGraphPlugin : EditorPlugin
    {
        private AudioGraphProxy _expressionGraphProxy;

        /// <inheritdoc />
        public override void InitializeEditor()
        {
            base.InitializeEditor();

            _expressionGraphProxy = new AudioGraphProxy();

            // Register the proxy
            Editor.ContentDatabase.Proxy.Insert(0, _expressionGraphProxy);
        }

        /// <inheritdoc />
        public override void Deinitialize()
        {
            // Cleanup on plugin deinit
            Editor.ContentDatabase.Proxy.Remove(_expressionGraphProxy);

            base.Deinitialize();
        }
    }
}
