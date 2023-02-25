using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundSetter
{
    public class PreviousSettings
    {
        public bool MasterVolumeMuted { get; set; }
        public bool BgmMuted { get; set; }
        public bool SoundEffectsMuted { get; set; }
        public bool VoiceMuted { get; set; }
        public bool SystemSoundsMuted { get; set; }
        public bool AmbientSoundsMuted { get; set; }

        public byte MasterVolume { get; set; }
        public byte Bgm { get; set; }
        public byte SoundEffects { get; set; }
        public byte Voice { get; set; }
        public byte SystemSounds { get; set; }
        public byte AmbientSounds { get; set; }

        public void SetFromVolumeControl(VolumeControls vc)
        {
            this.MasterVolumeMuted = vc.MasterVolumeMuted.GetValue();
            this.BgmMuted = vc.BgmMuted.GetValue();
            this.SoundEffectsMuted = vc.SoundEffectsMuted.GetValue();
            this.VoiceMuted = vc.VoiceMuted.GetValue();
            this.SystemSoundsMuted = vc.SystemSoundsMuted.GetValue();
            this.AmbientSoundsMuted = vc.AmbientSoundsMuted.GetValue();
            this.MasterVolume = vc.MasterVolume.GetValue();
            this.Bgm = vc.Bgm.GetValue();
            this.SoundEffects = vc.SoundEffects.GetValue();
            this.Voice = vc.Voice.GetValue();
            this.SystemSounds = vc.SystemSounds.GetValue();
            this.AmbientSounds = vc.AmbientSounds.GetValue();
        }

        public void SetFromConfig(Configuration config)
        {
            this.MasterVolumeMuted = config.MasterVolumeCutsceneMuted;
            this.BgmMuted = config.BgmCutsceneMuted;
            this.SoundEffectsMuted = config.SoundEffectsCutsceneMuted;
            this.VoiceMuted = config.VoiceCutsceneMuted;
            this.SystemSoundsMuted = config.SystemSoundsCutsceneMuted;
            this.AmbientSoundsMuted = config.AmbientSoundsCutsceneMuted;
            this.MasterVolume = (byte)config.MasterVolumeCutscene;
            this.Bgm = (byte)config.BgmCutscene;
            this.SoundEffects = (byte)config.SoundEffectsCutscene;
            this.Voice = (byte)config.VoiceCutscene;
            this.SystemSounds = (byte)config.SystemSoundsCutscene;
            this.AmbientSounds = (byte)config.AmbientSoundsCutscene;
        }
    }
}
