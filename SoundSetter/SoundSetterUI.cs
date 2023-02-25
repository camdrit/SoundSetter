using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using SoundSetter.OptionInternals;

namespace SoundSetter
{
    public class SoundSetterUI
    {
        private static readonly Vector4 HintColor = new(0.7f, 0.7f, 0.7f, 1.0f);

        private readonly Configuration config;
        private readonly VolumeControls vc;

        public bool IsVisible { get; set; }

        public SoundSetterUI(VolumeControls vc, Configuration config)
        {
            this.vc = vc;
            this.config = config;
        }

        /**
         * isInitialized seems to be true after reloading the plugin
         * for a single frame. The first time twice is incremented is
         * the plugin load frame, and the second time twice is incremented
         * is after initialization.
         */
        private int twice = 0;

        /**
         * Returns the appropriate window flags to allow the user to resize
         * the window, but only after the plugin has been initialized, and
         * only after giving ImGui one frame to set the default window size.
         *
         * Technically, I could just set appropriate window sizes explicitly,
         * but then those would need to be maintained, and they would need to
         * account for global font scaling.
         */
        private ImGuiWindowFlags GetWindowFlags()
        {
            var isInitialized = this.vc.BaseAddress != nint.Zero;
            if (!isInitialized)
            {
                return ImGuiWindowFlags.AlwaysAutoResize;
            }

            if (this.twice != 2)
            {
                this.twice++;
                return ImGuiWindowFlags.AlwaysAutoResize;
            }

            return ImGuiWindowFlags.None;
        }

        public void Draw()
        {
            if (!IsVisible)
                return;

            var pVisible = IsVisible;
            ImGui.Begin("SoundSetter Configuration", ref pVisible, GetWindowFlags());
            IsVisible = pVisible;

            var isInitialized = this.vc.BaseAddress != nint.Zero;
            if (isInitialized)
            {
                Settings();
            }
            else
            {
                Fail();
            }

            ImGui.End();
        }

        private void Settings()
        {
            var buttonSize = new Vector2(23, 23) * ImGui.GetIO().FontGlobalScale;

            ImGui.Text("Plugin Settings");

            ImGui.PushItemWidth(100f);
            var kItem1 = VirtualKey.EnumToIndex(this.config.ModifierKey);
            if (ImGui.Combo("##SoundSetterKeybind1", ref kItem1, VirtualKey.Names.Take(3).ToArray(), 3))
            {
                this.config.ModifierKey = VirtualKey.IndexToEnum(kItem1);
                this.config.Save();
            }

            ImGui.SameLine();
            var kItem2 = VirtualKey.EnumToIndex(this.config.MajorKey) - 3;
            if (ImGui.Combo("Keybind##SoundSetterKeybind2", ref kItem2, VirtualKey.Names.Skip(3).ToArray(),
                    VirtualKey.Names.Length - 3))
            {
                this.config.MajorKey = VirtualKey.IndexToEnum(kItem2) + 3;
                this.config.Save();
            }

            ImGui.PopItemWidth();

            var onlyCutscenes = this.config.OnlyShowInCutscenes;
            if (ImGui.Checkbox("Only enable keybind during cutscenes.##SoundSetterCutsceneOption", ref onlyCutscenes))
            {
                this.config.OnlyShowInCutscenes = onlyCutscenes;
                this.config.Save();
            }

            ImGui.TextColored(HintColor, "Use /ssconfig to reopen this window.");

            ImGui.Spacing();
            ImGui.Text("Sound Settings");

            var playSoundsWhileWindowIsNotActive = this.vc.PlaySoundsWhileWindowIsNotActive.GetValue();
            if (ImGui.Checkbox("Play sounds while window is not active.", ref playSoundsWhileWindowIsNotActive))
            {
                this.vc.PlaySoundsWhileWindowIsNotActive.SetValue(playSoundsWhileWindowIsNotActive);
            }

            ImGui.Indent();
            ImGui.BeginDisabled(!playSoundsWhileWindowIsNotActive);
            {
                if (ImGui.BeginTable("SoundSetterWhileInactiveOptions", 2, ImGuiTableFlags.None))
                {
                    ImGui.TableNextColumn();
                    var playSoundsWhileWindowIsNotActiveBgm = this.vc.PlaySoundsWhileWindowIsNotActiveBGM.GetValue();
                    if (ImGui.Checkbox("BGM", ref playSoundsWhileWindowIsNotActiveBgm))
                    {
                        this.vc.PlaySoundsWhileWindowIsNotActiveBGM.SetValue(playSoundsWhileWindowIsNotActiveBgm);
                    }

                    ImGui.TableNextColumn();
                    var playSoundsWhileWindowIsNotActiveSoundEffects =
                        this.vc.PlaySoundsWhileWindowIsNotActiveSoundEffects.GetValue();
                    if (ImGui.Checkbox("Sound Effects", ref playSoundsWhileWindowIsNotActiveSoundEffects))
                    {
                        this.vc.PlaySoundsWhileWindowIsNotActiveSoundEffects.SetValue(
                            playSoundsWhileWindowIsNotActiveSoundEffects);
                    }

                    ImGui.TableNextColumn();
                    var playSoundsWhileWindowIsNotActiveVoice =
                        this.vc.PlaySoundsWhileWindowIsNotActiveVoice.GetValue();
                    if (ImGui.Checkbox("Voice", ref playSoundsWhileWindowIsNotActiveVoice))
                    {
                        this.vc.PlaySoundsWhileWindowIsNotActiveVoice.SetValue(playSoundsWhileWindowIsNotActiveVoice);
                    }

                    ImGui.TableNextColumn();
                    var playSoundsWhileWindowIsNotActiveSystemSounds =
                        this.vc.PlaySoundsWhileWindowIsNotActiveSystemSounds.GetValue();
                    if (ImGui.Checkbox("System Sounds", ref playSoundsWhileWindowIsNotActiveSystemSounds))
                    {
                        this.vc.PlaySoundsWhileWindowIsNotActiveSystemSounds.SetValue(
                            playSoundsWhileWindowIsNotActiveSystemSounds);
                    }

                    ImGui.TableNextColumn();
                    var playSoundsWhileWindowIsNotActiveAmbientSounds =
                        this.vc.PlaySoundsWhileWindowIsNotActiveAmbientSounds.GetValue();
                    if (ImGui.Checkbox("Ambient Sounds", ref playSoundsWhileWindowIsNotActiveAmbientSounds))
                    {
                        this.vc.PlaySoundsWhileWindowIsNotActiveAmbientSounds.SetValue(
                            playSoundsWhileWindowIsNotActiveAmbientSounds);
                    }

                    ImGui.TableNextColumn();
                    var playSoundsWhileWindowIsNotActivePerformance =
                        this.vc.PlaySoundsWhileWindowIsNotActivePerformance.GetValue();
                    if (ImGui.Checkbox("Performance", ref playSoundsWhileWindowIsNotActivePerformance))
                    {
                        this.vc.PlaySoundsWhileWindowIsNotActivePerformance.SetValue(
                            playSoundsWhileWindowIsNotActivePerformance);
                    }

                    ImGui.EndTable();
                }
            }
            ImGui.EndDisabled();
            ImGui.Unindent();

            var playMusicWhenMounted = this.vc.PlayMusicWhenMounted.GetValue();
            if (ImGui.Checkbox("Play music when mounted.", ref playMusicWhenMounted))
            {
                this.vc.PlayMusicWhenMounted.SetValue(playMusicWhenMounted);
            }

            var enableNormalBattleMusic = this.vc.EnableNormalBattleMusic.GetValue();
            if (ImGui.Checkbox("Enable normal battle music.", ref enableNormalBattleMusic))
            {
                this.vc.EnableNormalBattleMusic.SetValue(enableNormalBattleMusic);
            }

            var enableCityStateBGM = this.vc.EnableCityStateBGM.GetValue();
            if (ImGui.Checkbox("Enable city-state BGM in residential areas.", ref enableCityStateBGM))
            {
                this.vc.EnableCityStateBGM.SetValue(enableCityStateBGM);
            }

            var playSystemSounds = this.vc.PlaySystemSounds.GetValue();
            if (ImGui.Checkbox("Play system sounds while waiting for Duty Finder.", ref playSystemSounds))
            {
                this.vc.PlaySystemSounds.SetValue(playSystemSounds);
            }

            ImGui.Text("Volume Settings");

            ImGui.PushFont(UiBuilder.IconFont);
            var masterVolumeMuted = this.vc.MasterVolumeMuted.GetValue();
            if (ImGui.Button(VolumeButtonName(masterVolumeMuted, nameof(masterVolumeMuted)), buttonSize))
            {
                this.vc.MasterVolumeMuted.SetValue(!masterVolumeMuted);
            }

            ImGui.PopFont();
            ImGui.SameLine();
            var masterVolume = (int)this.vc.MasterVolume.GetValue();
            if (ImGui.SliderInt("Master Volume", ref masterVolume, 0, 100))
            {
                this.vc.MasterVolume.SetValue((byte)masterVolume);
            }

            ImGui.PushFont(UiBuilder.IconFont);
            var bgmMuted = this.vc.BgmMuted.GetValue();
            if (ImGui.Button(VolumeButtonName(bgmMuted, nameof(bgmMuted)), buttonSize))
            {
                this.vc.BgmMuted.SetValue(!bgmMuted);
            }

            ImGui.PopFont();
            ImGui.SameLine();
            var bgm = (int)this.vc.Bgm.GetValue();
            if (ImGui.SliderInt("BGM", ref bgm, 0, 100))
            {
                this.vc.Bgm.SetValue((byte)bgm);
            }

            ImGui.PushFont(UiBuilder.IconFont);
            var soundEffectsMuted = this.vc.SoundEffectsMuted.GetValue();
            if (ImGui.Button(VolumeButtonName(soundEffectsMuted, nameof(soundEffectsMuted)), buttonSize))
            {
                this.vc.SoundEffectsMuted.SetValue(!soundEffectsMuted);
            }

            ImGui.PopFont();
            ImGui.SameLine();
            var soundEffects = (int)this.vc.SoundEffects.GetValue();
            if (ImGui.SliderInt("Sound Effects", ref soundEffects, 0, 100))
            {
                this.vc.SoundEffects.SetValue((byte)soundEffects);
            }

            ImGui.PushFont(UiBuilder.IconFont);
            var voiceMuted = this.vc.VoiceMuted.GetValue();
            if (ImGui.Button(VolumeButtonName(voiceMuted, nameof(voiceMuted)), buttonSize))
            {
                this.vc.VoiceMuted.SetValue(!voiceMuted);
            }

            ImGui.PopFont();
            ImGui.SameLine();
            var voice = (int)this.vc.Voice.GetValue();
            if (ImGui.SliderInt("Voice", ref voice, 0, 100))
            {
                this.vc.Voice.SetValue((byte)voice);
            }

            ImGui.PushFont(UiBuilder.IconFont);
            var systemSoundsMuted = this.vc.SystemSoundsMuted.GetValue();
            if (ImGui.Button(VolumeButtonName(systemSoundsMuted, nameof(systemSoundsMuted)), buttonSize))
            {
                this.vc.SystemSoundsMuted.SetValue(!systemSoundsMuted);
            }

            ImGui.PopFont();
            ImGui.SameLine();
            var systemSounds = (int)this.vc.SystemSounds.GetValue();
            if (ImGui.SliderInt("System Sounds", ref systemSounds, 0, 100))
            {
                this.vc.SystemSounds.SetValue((byte)systemSounds);
            }

            ImGui.PushFont(UiBuilder.IconFont);
            var ambientSoundsMuted = this.vc.AmbientSoundsMuted.GetValue();
            if (ImGui.Button(VolumeButtonName(ambientSoundsMuted, nameof(ambientSoundsMuted)), buttonSize))
            {
                this.vc.AmbientSoundsMuted.SetValue(!ambientSoundsMuted);
            }

            ImGui.PopFont();
            ImGui.SameLine();
            var ambientSounds = (int)this.vc.AmbientSounds.GetValue();
            if (ImGui.SliderInt("Ambient Sounds", ref ambientSounds, 0, 100))
            {
                this.vc.AmbientSounds.SetValue((byte)ambientSounds);
            }

            ImGui.PushFont(UiBuilder.IconFont);
            var performanceMuted = this.vc.PerformanceMuted.GetValue();
            if (ImGui.Button(VolumeButtonName(performanceMuted, nameof(performanceMuted)), buttonSize))
            {
                this.vc.PerformanceMuted.SetValue(!performanceMuted);
            }

            ImGui.PopFont();
            ImGui.SameLine();
            var performance = (int)this.vc.Performance.GetValue();
            if (ImGui.SliderInt("Performance", ref performance, 0, 100))
            {
                this.vc.Performance.SetValue((byte)performance);
            }

            ImGui.Text("Cutscene Volume");
            var autoAdjustCutsceneVolume = this.config.AutoAdjustCutsceneVolume;
            if (ImGui.Checkbox("Automatically adjust volume during cutscenes.", ref autoAdjustCutsceneVolume))
            {
                this.config.AutoAdjustCutsceneVolume = autoAdjustCutsceneVolume;
                this.config.Save();
            }

            ImGui.Indent();
            ImGui.BeginDisabled(!autoAdjustCutsceneVolume);
            {
                if (ImGui.Button("Set from Current Volume Settings"))
                {
                    this.config.MasterVolumeCutsceneMuted = this.vc.MasterVolumeMuted.GetValue();
                    this.config.MasterVolumeCutscene = this.vc.MasterVolume.GetValue();
                    this.config.BgmCutsceneMuted = this.vc.BgmMuted.GetValue();
                    this.config.BgmCutscene = this.vc.Bgm.GetValue();
                    this.config.SoundEffectsCutsceneMuted = this.vc.SoundEffectsMuted.GetValue();
                    this.config.SoundEffectsCutscene = this.vc.SoundEffects.GetValue();
                    this.config.VoiceCutsceneMuted = this.vc.SoundEffectsMuted.GetValue();
                    this.config.VoiceCutscene = this.vc.Voice.GetValue();
                    this.config.SystemSoundsCutsceneMuted = this.vc.SystemSoundsMuted.GetValue();
                    this.config.SystemSoundsCutscene = this.vc.SystemSounds.GetValue();
                    this.config.AmbientSoundsCutsceneMuted = this.vc.AmbientSoundsMuted.GetValue();
                    this.config.AmbientSoundsCutscene = this.vc.AmbientSounds.GetValue();
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var masterVolumeCutsceneMuted = this.config.MasterVolumeCutsceneMuted;
                if (ImGui.Button(VolumeButtonName(masterVolumeCutsceneMuted, nameof(masterVolumeCutsceneMuted)), buttonSize))
                {
                    this.config.MasterVolumeCutsceneMuted = !masterVolumeCutsceneMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var masterVolumeCutscene = this.config.MasterVolumeCutscene;
                if (ImGui.SliderInt("Master Volume##Cutscene", ref masterVolumeCutscene, 0, 100))
                {
                    this.config.MasterVolumeCutscene = masterVolumeCutscene;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var bgmCutsceneMuted = this.config.BgmCutsceneMuted;
                if (ImGui.Button(VolumeButtonName(bgmCutsceneMuted, nameof(bgmCutsceneMuted)), buttonSize))
                {
                    this.config.BgmCutsceneMuted = !bgmCutsceneMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var bgmCutscene = this.config.BgmCutscene;
                if (ImGui.SliderInt("BGM##Cutscene", ref bgmCutscene, 0, 100))
                {
                    this.config.BgmCutscene = bgmCutscene;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var soundEffectsCutsceneMuted = this.config.SoundEffectsCutsceneMuted;
                if (ImGui.Button(VolumeButtonName(soundEffectsCutsceneMuted, nameof(soundEffectsCutsceneMuted)), buttonSize))
                {
                    this.config.SoundEffectsCutsceneMuted = !soundEffectsCutsceneMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var soundEffectsCutscene = this.config.SoundEffectsCutscene;
                if (ImGui.SliderInt("Sound Effects##Cutscene", ref soundEffectsCutscene, 0, 100))
                {
                    this.config.SoundEffectsCutscene = soundEffectsCutscene;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var voiceCutsceneMuted = this.config.VoiceCutsceneMuted;
                if (ImGui.Button(VolumeButtonName(voiceCutsceneMuted, nameof(voiceCutsceneMuted)), buttonSize))
                {
                    this.config.VoiceCutsceneMuted = !voiceCutsceneMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var voiceCutscene = this.config.VoiceCutscene;
                if (ImGui.SliderInt("Voice##Cutscene", ref voiceCutscene, 0, 100))
                {
                    this.config.VoiceCutscene = voiceCutscene;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var systemSoundsCutsceneMuted = this.config.SystemSoundsCutsceneMuted;
                if (ImGui.Button(VolumeButtonName(systemSoundsCutsceneMuted, nameof(systemSoundsCutsceneMuted)), buttonSize))
                {
                    this.config.SystemSoundsCutsceneMuted = !systemSoundsCutsceneMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var systemSoundsCutscene = this.config.SystemSoundsCutscene;
                if (ImGui.SliderInt("System Sounds##Cutscene", ref systemSoundsCutscene, 0, 100))
                {
                    this.config.SystemSoundsCutscene = systemSoundsCutscene;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var ambientSoundsCutsceneMuted = this.config.AmbientSoundsCutsceneMuted;
                if (ImGui.Button(VolumeButtonName(ambientSoundsCutsceneMuted, nameof(ambientSoundsCutsceneMuted)), buttonSize))
                {
                    this.config.AmbientSoundsCutsceneMuted = !ambientSoundsCutsceneMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var ambientSoundsCutscene = this.config.AmbientSoundsCutscene;
                if (ImGui.SliderInt("Ambient Sounds##Cutscene", ref ambientSoundsCutscene, 0, 100))
                {
                    this.config.AmbientSoundsCutscene = ambientSoundsCutscene;
                    this.config.Save();
                }
            }
            ImGui.EndDisabled();
            ImGui.Unindent();

            ImGui.Text("Combat Volume");
            var autoAdjustCombatVolume = this.config.AutoAdjustCombatVolume;
            if (ImGui.Checkbox("Automatically adjust volume during Combat.", ref autoAdjustCombatVolume))
            {
                this.config.AutoAdjustCombatVolume = autoAdjustCombatVolume;
                this.config.Save();
            }

            ImGui.Indent();
            ImGui.BeginDisabled(!autoAdjustCombatVolume);
            {
                if (ImGui.Button("Set from Current Volume Settings"))
                {
                    this.config.MasterVolumeCombatMuted = this.vc.MasterVolumeMuted.GetValue();
                    this.config.MasterVolumeCombat = this.vc.MasterVolume.GetValue();
                    this.config.BgmCombatMuted = this.vc.BgmMuted.GetValue();
                    this.config.BgmCombat = this.vc.Bgm.GetValue();
                    this.config.SoundEffectsCombatMuted = this.vc.SoundEffectsMuted.GetValue();
                    this.config.SoundEffectsCombat = this.vc.SoundEffects.GetValue();
                    this.config.VoiceCombatMuted = this.vc.SoundEffectsMuted.GetValue();
                    this.config.VoiceCombat = this.vc.Voice.GetValue();
                    this.config.SystemSoundsCombatMuted = this.vc.SystemSoundsMuted.GetValue();
                    this.config.SystemSoundsCombat = this.vc.SystemSounds.GetValue();
                    this.config.AmbientSoundsCombatMuted = this.vc.AmbientSoundsMuted.GetValue();
                    this.config.AmbientSoundsCombat = this.vc.AmbientSounds.GetValue();
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var masterVolumeCombatMuted = this.config.MasterVolumeCombatMuted;
                if (ImGui.Button(VolumeButtonName(masterVolumeCombatMuted, nameof(masterVolumeCombatMuted)), buttonSize))
                {
                    this.config.MasterVolumeCombatMuted = !masterVolumeCombatMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var masterVolumeCombat = this.config.MasterVolumeCombat;
                if (ImGui.SliderInt("Master Volume##Combat", ref masterVolumeCombat, 0, 100))
                {
                    this.config.MasterVolumeCombat = masterVolumeCombat;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var bgmCombatMuted = this.config.BgmCombatMuted;
                if (ImGui.Button(VolumeButtonName(bgmCombatMuted, nameof(bgmCombatMuted)), buttonSize))
                {
                    this.config.BgmCombatMuted = !bgmCombatMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var bgmCombat = this.config.BgmCombat;
                if (ImGui.SliderInt("BGM##Combat", ref bgmCombat, 0, 100))
                {
                    this.config.BgmCombat = bgmCombat;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var soundEffectsCombatMuted = this.config.SoundEffectsCombatMuted;
                if (ImGui.Button(VolumeButtonName(soundEffectsCombatMuted, nameof(soundEffectsCombatMuted)), buttonSize))
                {
                    this.config.SoundEffectsCombatMuted = !soundEffectsCombatMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var soundEffectsCombat = this.config.SoundEffectsCombat;
                if (ImGui.SliderInt("Sound Effects##Combat", ref soundEffectsCombat, 0, 100))
                {
                    this.config.SoundEffectsCombat = soundEffectsCombat;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var voiceCombatMuted = this.config.VoiceCombatMuted;
                if (ImGui.Button(VolumeButtonName(voiceCombatMuted, nameof(voiceCombatMuted)), buttonSize))
                {
                    this.config.VoiceCombatMuted = !voiceCombatMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var voiceCombat = this.config.VoiceCombat;
                if (ImGui.SliderInt("Voice##Combat", ref voiceCombat, 0, 100))
                {
                    this.config.VoiceCombat = voiceCombat;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var systemSoundsCombatMuted = this.config.SystemSoundsCombatMuted;
                if (ImGui.Button(VolumeButtonName(systemSoundsCombatMuted, nameof(systemSoundsCombatMuted)), buttonSize))
                {
                    this.config.SystemSoundsCombatMuted = !systemSoundsCombatMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var systemSoundsCombat = this.config.SystemSoundsCombat;
                if (ImGui.SliderInt("System Sounds##Combat", ref systemSoundsCombat, 0, 100))
                {
                    this.config.SystemSoundsCombat = systemSoundsCombat;
                    this.config.Save();
                }

                ImGui.PushFont(UiBuilder.IconFont);
                var ambientSoundsCombatMuted = this.config.AmbientSoundsCombatMuted;
                if (ImGui.Button(VolumeButtonName(ambientSoundsCombatMuted, nameof(ambientSoundsCombatMuted)), buttonSize))
                {
                    this.config.AmbientSoundsCombatMuted = !ambientSoundsCombatMuted;
                    this.config.Save();
                }

                ImGui.PopFont();
                ImGui.SameLine();
                var ambientSoundsCombat = this.config.AmbientSoundsCombat;
                if (ImGui.SliderInt("Ambient Sounds##Combat", ref ambientSoundsCombat, 0, 100))
                {
                    this.config.AmbientSoundsCombat = ambientSoundsCombat;
                    this.config.Save();
                }
            }
            ImGui.EndDisabled();
            ImGui.Unindent();

            ImGui.Text("Player Effects Volume");

            var self = (int)this.vc.Self.GetValue();
            if (ImGui.SliderInt("Self", ref self, 0, 100))
            {
                this.vc.Self.SetValue((byte)self);
            }

            var party = (int)this.vc.Party.GetValue();
            if (ImGui.SliderInt("Party", ref party, 0, 100))
            {
                this.vc.Party.SetValue((byte)party);
            }

            var others = (int)this.vc.OtherPCs.GetValue();
            if (ImGui.SliderInt("Other PCs", ref others, 0, 100))
            {
                this.vc.OtherPCs.SetValue((byte)others);
            }

            ImGui.Text("Equalizer");

            var eqMode = (int)this.vc.EqualizerMode.GetValue();
            if (ImGui.Combo("Mode", ref eqMode, EqualizerMode.Names, EqualizerMode.Names.Length))
            {
                this.vc.EqualizerMode.SetValue((EqualizerMode.Enum)eqMode);
            }
        }

        private static void Fail()
        {
            ImGui.Text(
                "This appears to be your first installation of this plugin (or you reloaded all of your plugins).\nPlease manually change a volume setting once in order to initialize the plugin.");
        }

        private static string VolumeButtonName(bool state, string internalName)
        {
            return
                $"{(state ? FontAwesomeIcon.VolumeOff.ToIconString() : FontAwesomeIcon.VolumeUp.ToIconString())}##SoundSetter{internalName}";
        }
    }
}