﻿using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.IoC;
using Dalamud.Plugin;
using SoundSetter.Attributes;
using SoundSetter.OptionInternals;
using System;
using System.Linq;
using System.Text;
using Dalamud.Logging;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.CompilerServices;

// ReSharper disable ConvertIfStatementToSwitchStatement

namespace SoundSetter
{
    public class SoundSetter : IDalamudPlugin
    {
        private readonly DalamudPluginInterface pluginInterface;
        private readonly ChatGui chatGui;
        private readonly Condition condition;
        private readonly KeyState keyState;
        private readonly ClientState clientState;

        private readonly PluginCommandManager<SoundSetter> commandManager;

        private readonly Configuration config;
        private readonly SoundSetterUI ui;
        private readonly VolumeControls vc;
        private readonly PreviousSettings prev;

        public string Name => "SoundSetter";

        public SoundSetter(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ChatGui chatGui,
            [RequiredVersion("1.0")] SigScanner sigScanner,
            [RequiredVersion("1.0")] CommandManager commands,
            [RequiredVersion("1.0")] Condition condition,
            [RequiredVersion("1.0")] ClientState clientState,
            [RequiredVersion("1.0")] KeyState keyState)
        {
            this.pluginInterface = pluginInterface;
            this.chatGui = chatGui;
            this.condition = condition;
            this.clientState = clientState;
            this.keyState = keyState;

            this.config = (Configuration)this.pluginInterface.GetPluginConfig() ?? new Configuration();
            this.config.Initialize(this.pluginInterface);

            this.vc = new VolumeControls(sigScanner, null); // TODO: restore IPC

            this.prev = new PreviousSettings();

            this.pluginInterface.UiBuilder.DisableAutomaticUiHide = true;

            this.ui = new SoundSetterUI(this.vc, this.config);
            this.pluginInterface.UiBuilder.Draw += this.ui.Draw;
            this.pluginInterface.UiBuilder.Draw += OnTick;

            this.pluginInterface.UiBuilder.OpenConfigUi += OpenConfig;

            this.condition.ConditionChange += OnConditionChange;

            this.commandManager = new PluginCommandManager<SoundSetter>(this, commands);
        }

        private bool keysDown;


        private void OnConditionChange(ConditionFlag flag, bool value)
        {
            if ((flag == ConditionFlag.OccupiedInCutSceneEvent || flag == ConditionFlag.WatchingCutscene || flag == ConditionFlag.WatchingCutscene78) && this.config.AutoAdjustCutsceneVolume)
            {
                if (value)
                {
                    // Entering cutscene
                    this.prev.SetFromVolumeControl(this.vc);
                    this.vc.MasterVolumeMuted.SetValue(this.config.MasterVolumeCutsceneMuted);
                    this.vc.BgmMuted.SetValue(this.config.BgmCutsceneMuted);
                    this.vc.SoundEffectsMuted.SetValue(this.config.SoundEffectsCutsceneMuted);
                    this.vc.VoiceMuted.SetValue(this.config.VoiceCutsceneMuted);
                    this.vc.SystemSoundsMuted.SetValue(this.config.SystemSoundsCutsceneMuted);
                    this.vc.AmbientSoundsMuted.SetValue(this.config.AmbientSoundsCutsceneMuted);

                    var masterTask = SmoothValue(this.vc.MasterVolume, (byte)this.config.MasterVolumeCutscene);
                    var bgmTask = SmoothValue(this.vc.Bgm, (byte)this.config.BgmCutscene);
                    var sfxTask = SmoothValue(this.vc.SoundEffects, (byte)this.config.SoundEffectsCutscene);
                    var voiceTask = SmoothValue(this.vc.Voice, (byte)this.config.VoiceCutscene);
                    var systemTask = SmoothValue(this.vc.SystemSounds, (byte)this.config.SystemSoundsCutscene);
                    var ambientTask = SmoothValue(this.vc.AmbientSounds, (byte)this.config.AmbientSoundsCutscene);
                    var allTasks = Task.WhenAll(masterTask, bgmTask, sfxTask, voiceTask, systemTask, ambientTask);
                }
                else
                {
                    this.vc.MasterVolumeMuted.SetValue(this.prev.MasterVolumeMuted);
                    this.vc.BgmMuted.SetValue(this.prev.BgmMuted);
                    this.vc.SoundEffectsMuted.SetValue(this.prev.SoundEffectsMuted);
                    this.vc.VoiceMuted.SetValue(this.prev.VoiceMuted);
                    this.vc.SystemSoundsMuted.SetValue(this.prev.SystemSoundsMuted);
                    this.vc.AmbientSoundsMuted.SetValue(this.prev.AmbientSoundsMuted);

                    var masterTask = SmoothValue(this.vc.MasterVolume, prev.MasterVolume);
                    var bgmTask = SmoothValue(this.vc.Bgm, prev.Bgm);
                    var sfxTask = SmoothValue(this.vc.SoundEffects, prev.SoundEffects);
                    var voiceTask = SmoothValue(this.vc.Voice, prev.Voice);
                    var systemTask = SmoothValue(this.vc.SystemSounds, prev.SystemSounds);
                    var ambientTask = SmoothValue(this.vc.AmbientSounds, prev.AmbientSounds);
                    Task.WhenAll(masterTask, bgmTask, sfxTask, voiceTask, systemTask, ambientTask);
                }
            }
            else if (flag == ConditionFlag.InCombat && this.config.AutoAdjustCombatVolume)
            {
                if (value)
                {
                    // Entering combat
                    this.prev.SetFromVolumeControl(this.vc);
                    this.vc.MasterVolumeMuted.SetValue(this.config.MasterVolumeCombatMuted);
                    this.vc.BgmMuted.SetValue(this.config.BgmCombatMuted);
                    this.vc.SoundEffectsMuted.SetValue(this.config.SoundEffectsCombatMuted);
                    this.vc.VoiceMuted.SetValue(this.config.VoiceCombatMuted);
                    this.vc.SystemSoundsMuted.SetValue(this.config.SystemSoundsCombatMuted);
                    this.vc.AmbientSoundsMuted.SetValue(this.config.AmbientSoundsCombatMuted);

                    var masterTask = SmoothValue(this.vc.MasterVolume, (byte)this.config.MasterVolumeCombat);
                    var bgmTask = SmoothValue(this.vc.Bgm, (byte)this.config.BgmCombat);
                    var sfxTask = SmoothValue(this.vc.SoundEffects, (byte)this.config.SoundEffectsCombat);
                    var voiceTask = SmoothValue(this.vc.Voice, (byte)this.config.VoiceCombat);
                    var systemTask = SmoothValue(this.vc.SystemSounds, (byte)this.config.SystemSoundsCombat);
                    var ambientTask = SmoothValue(this.vc.AmbientSounds, (byte)this.config.AmbientSoundsCombat);
                    var allTasks = Task.WhenAll(masterTask, bgmTask, sfxTask, voiceTask, systemTask, ambientTask);
                }
                else
                {
                    this.vc.MasterVolumeMuted.SetValue(this.prev.MasterVolumeMuted);
                    this.vc.BgmMuted.SetValue(this.prev.BgmMuted);
                    this.vc.SoundEffectsMuted.SetValue(this.prev.SoundEffectsMuted);
                    this.vc.VoiceMuted.SetValue(this.prev.VoiceMuted);
                    this.vc.SystemSoundsMuted.SetValue(this.prev.SystemSoundsMuted);
                    this.vc.AmbientSoundsMuted.SetValue(this.prev.AmbientSoundsMuted);

                    var masterTask = SmoothValue(this.vc.MasterVolume, prev.MasterVolume, 15);
                    var bgmTask = SmoothValue(this.vc.Bgm, prev.Bgm, 15);
                    var sfxTask = SmoothValue(this.vc.SoundEffects, prev.SoundEffects, 15);
                    var voiceTask = SmoothValue(this.vc.Voice, prev.Voice, 15);
                    var systemTask = SmoothValue(this.vc.SystemSounds, prev.SystemSounds, 15);
                    var ambientTask = SmoothValue(this.vc.AmbientSounds, prev.AmbientSounds, 15);
                    Task.WhenAll(masterTask, bgmTask, sfxTask, voiceTask, systemTask, ambientTask);
                }
            }
        }

        static async Task SmoothValue(ByteOption target, byte to, int delay = 35)
        {
            if (target.GetValue() > to)
            {
                while (to < target.GetValue())
                {
                    target.SetValueSafe((byte)(target.GetValue() - 1));
                    await Task.Delay(delay);
                }
            }
            else if (target.GetValue() < to)
            {
                while (to > target.GetValue())
                {
                    target.SetValueSafe((byte)(target.GetValue() + 1));
                    await Task.Delay(delay);
                }
            }
            target.SetValueSafe(to);
        }

        private void OnTick()
        {
            // We don't want to open the UI before the player loads, that leaves the options uninitialized.
            if (this.clientState.LocalContentId == 0) return;

            var cutsceneActive = this.condition[ConditionFlag.OccupiedInCutSceneEvent] ||
                                 this.condition[ConditionFlag.WatchingCutscene] ||
                                 this.condition[ConditionFlag.WatchingCutscene78];

            if (this.config.OnlyShowInCutscenes && !cutsceneActive) return;

            if (this.keyState[(byte)this.config.ModifierKey] &&
                this.keyState[(byte)this.config.MajorKey])
            {
                if (this.keysDown) return;

                this.keysDown = true;
                ToggleConfig();
                return;
            }

            this.keysDown = false;
        }

        [Command("/soundsetterconfig")]
        [Aliases("/ssconfig")]
        [HelpMessage("Open the SoundSetter configuration.")]
        public void SoundSetterConfigCommand(string command, string args)
            => ToggleConfig();

        private void ToggleConfig()
            => this.ui.IsVisible = !this.ui.IsVisible;

        private void OpenConfig()
            => this.ui.IsVisible = true;

        private const string MasterVolumeAdjustCommand = "/ssmv";

        [Command(MasterVolumeAdjustCommand)]
        [HelpMessage("Adjust the game's master volume by the specified quantity.")]
        public void MasterVolumeAdjust(string command, string args)
        {
            DoCommand(
                MasterVolumeAdjustCommand,
                "Master",
                ErrorMessages.AdjustCommand,
                this.vc.MasterVolumeMuted,
                this.vc.MasterVolume,
                args);
        }

        private const string BgmAdjustCommand = "/ssbgm";

        [Command(BgmAdjustCommand)]
        [HelpMessage("Adjust the game's BGM volume by the specified quantity.")]
        public void BgmAdjust(string command, string args)
        {
            DoCommand(
                BgmAdjustCommand,
                "BGM",
                ErrorMessages.AdjustCommand,
                this.vc.BgmMuted,
                this.vc.Bgm,
                args);
        }

        private const string SoundEffectsAdjustCommand = "/sssfx";

        [Command(SoundEffectsAdjustCommand)]
        [HelpMessage("Adjust the game's SFX volume by the specified quantity.")]
        public void SoundEffectsAdjust(string command, string args)
        {
            DoCommand(
                SoundEffectsAdjustCommand,
                "SFX",
                ErrorMessages.AdjustCommand,
                this.vc.SoundEffectsMuted,
                this.vc.SoundEffects,
                args);
        }

        private const string VoiceAdjustCommand = "/ssv";

        [Command(VoiceAdjustCommand)]
        [HelpMessage("Adjust the game's voice volume by the specified quantity.")]
        public void VoiceAdjust(string command, string args)
        {
            DoCommand(
                VoiceAdjustCommand,
                "Voice",
                ErrorMessages.AdjustCommand,
                this.vc.VoiceMuted,
                this.vc.Voice,
                args);
        }

        private const string SystemSoundsAdjustCommand = "/sssys";

        [Command(SystemSoundsAdjustCommand)]
        [HelpMessage("Adjust the game's system sound volume by the specified quantity.")]
        public void SystemSoundsAdjust(string command, string args)
        {
            DoCommand(
                SystemSoundsAdjustCommand,
                "System sound",
                ErrorMessages.AdjustCommand,
                this.vc.SystemSoundsMuted,
                this.vc.SystemSounds,
                args);
        }

        private const string AmbientSoundsAdjustCommand = "/ssas";

        [Command(AmbientSoundsAdjustCommand)]
        [HelpMessage("Adjust the game's ambient sound volume by the specified quantity.")]
        public void AmbientSoundsAdjust(string command, string args)
        {
            DoCommand(
                AmbientSoundsAdjustCommand,
                "Ambient sound",
                ErrorMessages.AdjustCommand,
                this.vc.AmbientSoundsMuted,
                this.vc.AmbientSounds,
                args);
        }

        private const string PerformanceAdjustCommand = "/ssp";

        [Command(PerformanceAdjustCommand)]
        [HelpMessage("Adjust the game's performance volume by the specified quantity.")]
        public void PerformanceAdjust(string command, string args)
        {
            DoCommand(
                PerformanceAdjustCommand,
                "Performance",
                ErrorMessages.AdjustCommand,
                this.vc.PerformanceMuted,
                this.vc.Performance,
                args);
        }

        private void DoCommand(string command, string optName, string errorMessage, BooleanOption boolOpt,
            ByteOption varOpt, string args)
        {
            ParseAdjustArgs(args, out var op, out var targetStr);

            try
            {
                if (op == OperationKind.Toggle)
                {
                    var muted = boolOpt?.GetValue();
                    op = muted == true ? OperationKind.Unmute : OperationKind.Mute;
                }

                if (op == OperationKind.Mute)
                {
                    VolumeControls.ToggleVolume(boolOpt, op);
                    this.chatGui.Print($"{optName} volume muted.");
                    return;
                }

                if (op == OperationKind.Unmute)
                {
                    VolumeControls.ToggleVolume(boolOpt, op);
                    this.chatGui.Print($"{optName} volume unmuted.");
                    return;
                }

                if (!int.TryParse(targetStr, out var volumeTarget))
                {
                    PrintChatError(this.chatGui, string.Format(errorMessage, command));
                    return;
                }

                VolumeControls.AdjustVolume(varOpt, volumeTarget, op);
                this.chatGui.Print($"{optName} volume set to {varOpt.GetValue()}.");
            }
            catch (InvalidOperationException e)
            {
                PluginLog.LogError(e, "Command failed.");
                this.chatGui.Print("SoundSetter is uninitialized.");
                this.chatGui.Print("Please manually change a volume setting once in order to initialize the plugin.");
            }
        }

        private static void ParseAdjustArgs(string args, out OperationKind op, out string volumeTargetStr)
        {
            volumeTargetStr = "";

            if (string.IsNullOrEmpty(args))
            {
                op = OperationKind.Toggle;
                return;
            }

            var argsList = args.Split(' ').Select(a => a.ToLower()).ToList();

            var arg0 = argsList.FirstOrDefault();
            if (string.IsNullOrEmpty(arg0))
            {
                op = OperationKind.Set;
                return;
            }

            if (arg0 == "toggle")
            {
                op = OperationKind.Toggle;
                return;
            }

            if (arg0 == "mute")
            {
                op = OperationKind.Mute;
                return;
            }

            if (arg0 == "unmute")
            {
                op = OperationKind.Unmute;
                return;
            }

            volumeTargetStr = arg0;
            op = volumeTargetStr[0] switch
            {
                '+' => OperationKind.Add,
                '-' => OperationKind.Subtract,
                _ => OperationKind.Set,
            };

            if (op != OperationKind.Set)
                volumeTargetStr = volumeTargetStr[1..];
        }

        private static void PrintChatError(ChatGui chat, string message)
        {
            chat.PrintChat(new XivChatEntry
            {
                Message = SeString.Parse(Encoding.UTF8.GetBytes(message)),
                Type = XivChatType.ErrorMessage,
            });
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this.commandManager.Dispose();

            this.pluginInterface.UiBuilder.OpenConfigUi -= OpenConfig;

            this.pluginInterface.UiBuilder.Draw -= OnTick;
            this.pluginInterface.UiBuilder.Draw -= this.ui.Draw;

            this.vc.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}