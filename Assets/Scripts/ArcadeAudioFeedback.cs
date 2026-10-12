using UnityEngine;

namespace NeonReflexVR
{
    public enum ArcadeCue { Fire, Hit, Start, GameOver }

    [RequireComponent(typeof(AudioSource))]
    public sealed class ArcadeAudioFeedback : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] float fireVolume = 0.18f;
        [SerializeField, Range(0f, 1f)] float hitVolume = 0.22f;
        [SerializeField, Range(0f, 1f)] float startVolume = 0.2f;
        [SerializeField, Range(0f, 1f)] float gameOverVolume = 0.18f;
        const int SampleRate = 48000;
        AudioSource source;
        AudioClip[] clips;
        readonly int[] requests = new int[4];

        public int GetRequestCount(ArcadeCue cue) => requests[(int)cue];
        public AudioClip GetClip(ArcadeCue cue) { EnsureInitialized(); return clips[(int)cue]; }
        void Awake() => EnsureInitialized();

        void EnsureInitialized()
        {
            if (clips != null) return;
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f; // Centered stereo output through the existing XR AudioListener.
            source.panStereo = 0f;
            source.dopplerLevel = 0f;
            clips = new AudioClip[4];
            for (var i = 0; i < clips.Length; i++)
            {
                var data = GenerateSamples((ArcadeCue)i);
                clips[i] = AudioClip.Create("NeonReflexVR " + (ArcadeCue)i, data.Length, 1, SampleRate, false);
                clips[i].hideFlags = HideFlags.DontSave;
                clips[i].SetData(data, 0);
            }
        }

        public static float[] GenerateSamples(ArcadeCue cue)
        {
            var duration = cue == ArcadeCue.Fire ? 0.09f : cue == ArcadeCue.Hit ? 0.14f : cue == ArcadeCue.Start ? 0.32f : 0.38f;
            var data = new float[Mathf.RoundToInt(duration * SampleRate)];
            var phase = 0f;
            for (var i = 0; i < data.Length; i++)
            {
                var t = i / (float)SampleRate;
                var progress = t / duration;
                var frequency = cue == ArcadeCue.Fire ? Mathf.Lerp(620f, 280f, progress)
                    : cue == ArcadeCue.Hit ? 440f
                    : cue == ArcadeCue.Start ? 220f + 110f * Mathf.Min(2, Mathf.FloorToInt(progress * 3f))
                    : Mathf.Lerp(440f, 180f, progress);
                phase += 2f * Mathf.PI * frequency / SampleRate;
                var fade = Mathf.Min(1f, Mathf.Min(t / 0.008f, (data.Length - 1 - i) / (SampleRate * 0.012f)));
                fade = fade * fade * (3f - 2f * fade);
                var wave = Mathf.Sin(phase) + (cue == ArcadeCue.Hit ? 0.2f * Mathf.Sin(phase * 2f) : 0f);
                data[i] = 0.28f * wave * fade * Mathf.Exp(-2f * progress);
            }
            return data;
        }

        public void Play(ArcadeCue cue)
        {
            EnsureInitialized();
            requests[(int)cue]++;
            if (!Application.isPlaying) return; // EditMode tests verify requests, not audible playback.
            if (cue == ArcadeCue.Start || cue == ArcadeCue.GameOver) source.Stop();
            var volume = cue == ArcadeCue.Fire ? fireVolume : cue == ArcadeCue.Hit ? hitVolume : cue == ArcadeCue.Start ? startVolume : gameOverVolume;
            source.PlayOneShot(clips[(int)cue], Mathf.Clamp01(volume));
        }

        void OnDestroy()
        {
            if (clips == null) return;
            foreach (var clip in clips)
                if (clip != null) { if (Application.isPlaying) Destroy(clip); else DestroyImmediate(clip); }
            clips = null;
        }
    }
}