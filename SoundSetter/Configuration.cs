﻿using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace SoundSetter
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }

        [Obsolete]
        public VirtualKey.Enum[] Keybind { get; }

        public VirtualKey.Enum ModifierKey { get; set; }
        public VirtualKey.Enum MajorKey { get; set; }

        public bool OnlyShowInCutscenes { get; set; }
        public bool AutoAdjustCutsceneVolume { get; set; }

        // CUTSCENE OPTIONS
        public bool MasterVolumeCutsceneMuted { get; set; }
        public bool BgmCutsceneMuted { get; set; }
        public bool SoundEffectsCutsceneMuted { get; set; }
        public bool VoiceCutsceneMuted { get; set; }
        public bool SystemSoundsCutsceneMuted { get; set; }
        public bool AmbientSoundsCutsceneMuted { get; set; }

        public int MasterVolumeCutscene { get; set; }
        public int BgmCutscene { get; set; }
        public int SoundEffectsCutscene { get; set; }
        public int VoiceCutscene { get; set; }
        public int SystemSoundsCutscene { get; set; }
        public int AmbientSoundsCutscene { get; set; }

        public Configuration()
        {
            ModifierKey = VirtualKey.Enum.VkControl;
            MajorKey = VirtualKey.Enum.VkK;
        }

        [JsonIgnore] private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            // v1.0.2 compat
            if (ModifierKey != default) return;
#pragma warning disable 612
            ModifierKey = Keybind[0];
            MajorKey = Keybind[1];
#pragma warning restore 612
        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}