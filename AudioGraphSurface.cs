// Copyright (C) 2021 Pavel Patrovský. Do not redistribute!

using System;
using System.Collections.Generic;
using FlaxEditor;
using FlaxEditor.Scripting;
using FlaxEditor.Surface;
using FlaxEngine;

namespace AudioSystemFlax
{
    /// <summary>
    /// AudioGraphSurface Script.
    /// </summary>
    public class AudioGraphSurface : VisjectSurface
    {
        public const int MainNodeGroupId = 1;
        public const int MainNodeTypeId = 1;

        // Our own node archetypes
        public static readonly NodeArchetype[] ExpressionGraphNodes =
        {
            // Main node
            new NodeArchetype
            {
                TypeID = MainNodeTypeId,
                Title = "Output",
                Description = "The Resulting audio",
                Flags = NodeFlags.AllGraphs | NodeFlags.NoRemove | NodeFlags.NoSpawnViaGUI | NodeFlags.NoCloseButton,
                Size = new Vector2(150, 100),
                Elements = new[]
                {
                    NodeElementArchetype.Factory.Input(0, "Audio", true, typeof(void), 0)
                }
            },
            new NodeArchetype
            {
                TypeID = 2,
                Title = "Sample Audio",
                Description = "Input Samples the Audio",
                Flags = NodeFlags.AllGraphs,
                Size = new Vector2(230, 160),
                DefaultValues = new object[]
                {
                    Guid.Empty,
                    1.0f,
                    true,
                    0.0f,
                },
                Elements = new[]
                {
                    NodeElementArchetype.Factory.Output(0, "", typeof(void), 0),
                    NodeElementArchetype.Factory.Output(1, "Normalized Time", typeof(float), 1),
                    NodeElementArchetype.Factory.Output(2, "Time", typeof(float), 2),
                    NodeElementArchetype.Factory.Output(3, "Length", typeof(float), 3),
                    NodeElementArchetype.Factory.Output(4, "Is Playing", typeof(bool), 4),
                    NodeElementArchetype.Factory.Input(0, "Speed", true, typeof(float), 5, 1),
                    NodeElementArchetype.Factory.Input(1, "Loop", true, typeof(bool), 6, 2),
                    NodeElementArchetype.Factory.Input(2, "Start Position", true, typeof(float), 7, 3),
                    NodeElementArchetype.Factory.Asset(0, FlaxEditor.Surface.Constants.LayoutOffsetY * 3, 0, typeof(AudioGraph)),
                }
            },
        };

        // List of group archetypes
        public static readonly List<GroupArchetype> ExpressionGraphGroups = new List<GroupArchetype>()
        {
            // Our own nodes, including the main node
            new GroupArchetype
            {
                GroupID = MainNodeGroupId,
                Name = "ExpressionGraph",
                Color = new Color(231, 231, 60),
                Archetypes = ExpressionGraphNodes
            },
            // All math nodes
            new GroupArchetype
            {
                GroupID = 3,
                Name = "Math",
                Color = new Color(52, 152, 219),
                Archetypes = FlaxEditor.Surface.Archetypes.Math.Nodes
            },
            // Just a single parameter node
            new GroupArchetype
            {
                GroupID = 5,
                Name = "Parameters",
                Color = new Color(52, 73, 94),
                Archetypes = new []{ FlaxEditor.Surface.Archetypes.Parameters.Nodes[0] }
            },
            new GroupArchetype
            {
                GroupID = 6,
                Name = "Bitwise",
                Color = new Color(52, 73, 94),
                Archetypes = FlaxEditor.Surface.Archetypes.Bitwise.Nodes
            },
            new GroupArchetype
            {
                GroupID = 7,
                Name = "Boolean",
                Color = new Color(52, 73, 94),
                Archetypes = FlaxEditor.Surface.Archetypes.Boolean.Nodes
            },
            new GroupArchetype
            {
                GroupID = 8,
                Name = "Comparisons",
                Color = new Color(52, 73, 94),
                Archetypes = FlaxEditor.Surface.Archetypes.Comparisons.Nodes
            },
            new GroupArchetype
            {
                GroupID = 9,
                Name = "Constants",
                Color = new Color(52, 73, 94),
                Archetypes = FlaxEditor.Surface.Archetypes.Constants.Nodes
            },
            new GroupArchetype
            {
                GroupID = 10,
                Name = "Flow",
                Color = new Color(52, 73, 94),
                Archetypes = FlaxEditor.Surface.Archetypes.Flow.Nodes
            },
            new GroupArchetype
            {
                GroupID = 12,
                Name = "Packing",
                Color = new Color(52, 73, 94),
                Archetypes = FlaxEditor.Surface.Archetypes.Packing.Nodes
            }
        };

        // Surface will be expanded later
        public AudioGraphSurface(IVisjectSurfaceOwner owner, Action onSave, FlaxEditor.Undo undo = null)
        : base(owner, onSave, undo, CreateStyle(), ExpressionGraphGroups)
        {
            ScriptsBuilder.ScriptsReloadBegin += ScriptsReload;
        }

        void ScriptsReload()
        {
            Owner.OnSurfaceClose();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            ScriptsBuilder.ScriptsReloadBegin -= ScriptsReload;
        }

        public override string GetTypeName(ScriptType type)
        {
            if (type.Type == typeof(void))
                return "Audio";
            return base.GetTypeName(type);
        }
        /// <summary>
        /// For saving and loading surfaces
        /// </summary>
        private class FakeSurfaceContext : ISurfaceContext
        {
            public string SurfaceName => throw new NotImplementedException();

            public byte[] SurfaceData { get; set; }

            public void OnContextCreated(VisjectSurfaceContext context)
            {

            }
        }

        /// <summary>
        /// Tries to load surface graph from the asset.
        /// </summary>
        /// <param name="createDefaultIfMissing">True if create default surface if missing, otherwise won't load anything.</param>
        /// <returns>Loaded surface bytes or null if cannot load it or it's missing.</returns>
        public static byte[] LoadSurface(JsonAsset asset, AudioGraph assetInstance, bool createDefaultIfMissing)
        {
            if (!asset) throw new ArgumentNullException(nameof(asset));
            if (assetInstance == null) throw new ArgumentNullException(nameof(assetInstance));

            // Return its data
            if (assetInstance.Surface?.Length > 0)
            {
                return assetInstance.Surface;
            }

            // Create it if it's missing
            if (createDefaultIfMissing)
            {
                // A bit of a hack
                // Create a Visject Graph with a main node and serialize it!
                var surfaceContext = new VisjectSurfaceContext(null, null, new FakeSurfaceContext());

                // Add the main node
                // TODO: Change NodeFactory.DefaultGroups to your list of group archetypes
                var node = NodeFactory.CreateNode(ExpressionGraphGroups, 1, surfaceContext, MainNodeGroupId, MainNodeTypeId);

                if (node == null)
                {
                    Debug.LogWarning("Failed to create main node.");
                    return null;
                }
                surfaceContext.Nodes.Add(node);
                node.Location = Vector2.Zero;
                surfaceContext.Save();
                return surfaceContext.Context.SurfaceData;
            }
            else
            {
                return null;
            }
        }

        private static SurfaceStyle CreateStyle()
        {
            var editor = Editor.Instance;
            var style = SurfaceStyle.CreateStyleHandler(editor);
            style.Colors.Impulse = Color.FromRGB(0xb32b96);
            return style;
        }

        /// <summary>
        /// Updates the surface graph asset (save new one, discard cached data, reload asset).
        /// </summary>
        /// <param name="data">Surface data.</param>
        /// <returns>True if cannot save it, otherwise false.</returns>
        public static bool SaveSurface(JsonAsset asset, AudioGraph assetInstance, byte[] surfaceData)
        {
            if (!asset) throw new ArgumentNullException(nameof(asset));

            assetInstance.Surface = surfaceData;

            bool success = FlaxEditor.Editor.SaveJsonAsset(asset.Path, assetInstance);
            asset.Reload();
            return success;
        }
    }
}
