using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using Unity.Profiling;

namespace NeonReflexVR
{
    /// <summary>Bounded player-frame samples. Percentiles are computed only when reporting.</summary>
    public sealed class PerformanceWindow
    {
        readonly double[] frames;
        readonly double[] sorted;
        public readonly int[] States = new int[3];
        public readonly double[] SpikeMs = new double[8];
        public readonly double[] SpikeAt = new double[8];
        public readonly GameState[] SpikeState = new GameState[8];
        public int Count { get; private set; }
        public int SpikeCount { get; private set; }
        public int CpuCount { get; private set; }
        public int GpuCount { get; private set; }
        int mainCount, allocationCount;
        double total, cpuTotal, gpuTotal, mainTotal, allocationTotal;
        public double WorstMs { get; private set; }
        public long PeakAllocationBytes { get; private set; }
        public bool Full => Count == frames.Length;
        public double AverageFps => Count == 0 ? double.NaN : Count * 1000d / total;
        public double CpuMeanMs => CpuCount == 0 ? double.NaN : cpuTotal / CpuCount;
        public double GpuMeanMs => GpuCount == 0 ? double.NaN : gpuTotal / GpuCount;
        public double MainMeanMs => mainCount == 0 ? double.NaN : mainTotal / mainCount;
        public double AllocationMeanBytes => allocationCount == 0 ? double.NaN : allocationTotal / allocationCount;
        public PerformanceWindow(int capacity = 8192)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            frames = new double[capacity]; sorted = new double[capacity];
        }
        public static bool ValidTime(double value) => value > 0 && !double.IsNaN(value) && !double.IsInfinity(value);
        public static string Metric(double value) => double.IsNaN(value) || double.IsInfinity(value)
            ? "unavailable" : value.ToString("F3", CultureInfo.InvariantCulture);
        public bool Record(double ms, GameState state, double elapsed, double cpu = double.NaN,
            double gpu = double.NaN, double main = double.NaN, long allocation = -1)
        {
            if (!ValidTime(ms) || Full) return false;
            frames[Count++] = ms; total += ms; States[(int)state]++;
            WorstMs = Math.Max(WorstMs, ms);
            if (ValidTime(cpu)) { cpuTotal += cpu; CpuCount++; }
            if (ValidTime(gpu)) { gpuTotal += gpu; GpuCount++; }
            if (ValidTime(main)) { mainTotal += main; mainCount++; }
            if (allocation >= 0) { allocationTotal += allocation; allocationCount++; PeakAllocationBytes = Math.Max(PeakAllocationBytes, allocation); }
            if (ms > 33.3)
            {
                SpikeCount++;
                for (var i = 0; i < SpikeMs.Length; i++)
                    if (ms > SpikeMs[i])
                    {
                        for (var j = SpikeMs.Length - 1; j > i; j--)
                        { SpikeMs[j] = SpikeMs[j - 1]; SpikeAt[j] = SpikeAt[j - 1]; SpikeState[j] = SpikeState[j - 1]; }
                        SpikeMs[i] = ms; SpikeAt[i] = elapsed; SpikeState[i] = state; break;
                    }
            }
            return true;
        }
        public double P99Ms()
        {
            if (Count == 0) return double.NaN;
            Array.Copy(frames, sorted, Count); Array.Sort(sorted, 0, Count);
            return sorted[Math.Max(0, (int)Math.Ceiling(Count * 0.99) - 1)];
        }
        public void Clear()
        {
            Count = SpikeCount = CpuCount = GpuCount = mainCount = allocationCount = 0;
            total = cpuTotal = gpuTotal = mainTotal = allocationTotal = WorstMs = 0;
            PeakAllocationBytes = 0; Array.Clear(States, 0, 3); Array.Clear(SpikeMs, 0, 8);
        }
    }

    /// <summary>Development Player only; no HUD, per-frame logs, deep profiler or unbounded sample history.</summary>
    public sealed class PerformanceDiagnostics : MonoBehaviour
    {
        PerformanceWindow window;
        readonly FrameTiming[] timings = new FrameTiming[1];
        readonly List<XRDisplaySubsystem> displays = new List<XRDisplaySubsystem>(2);
        ProfilerRecorder main, used, reserved, allocated, draws, triangles;
        GameManager manager;
        GameState lastState = GameState.Ready;
        XRHMD hmd;
        XRDisplaySubsystem display;
        double started, previous, nextReport, nextLookup, nextTrackingLog;
        ulong lastTiming;
        bool focused = true, paused, tracked, gpuClock, baselinePending = true;
        int focusLosses, trackingLosses, gcCollections;
        string reportPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Application.isEditor || !Debug.isDebugBuild || FindFirstObjectByType<PerformanceDiagnostics>() != null) return;
            var obj = new GameObject("Performance Diagnostics"); DontDestroyOnLoad(obj);
            obj.AddComponent<PerformanceDiagnostics>();
        }
        void OnEnable()
        {
            if (Application.isEditor || !Debug.isDebugBuild) { enabled = false; return; }
            window = new PerformanceWindow();
            started = Time.realtimeSinceStartupAsDouble; previous = started; nextReport = started + 30;
            focused = Application.isFocused;
            gcCollections = GC.CollectionCount(0);
            main = Counter(ProfilerCategory.Internal, "Main Thread");
            used = Counter(ProfilerCategory.Memory, "GC Used Memory");
            reserved = Counter(ProfilerCategory.Memory, "GC Reserved Memory");
            allocated = Counter(ProfilerCategory.Memory, "GC Allocated In Frame");
            draws = Counter(ProfilerCategory.Render, "Draw Calls Count");
            triangles = Counter(ProfilerCategory.Render, "Triangles Count");
            gpuClock = FrameTimingManager.IsFeatureEnabled() && FrameTimingManager.GetGpuTimerFrequency() > 0;
            try
            {
                var folder = Path.Combine(Application.persistentDataPath, "Diagnostics"); Directory.CreateDirectory(folder);
                reportPath = Path.Combine(folder, "perf-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".log");
            }
            catch (IOException) { reportPath = null; }
            catch (UnauthorizedAccessException) { reportPath = null; }
            Emit("start utc=" + DateTime.UtcNow.ToString("O") + " unity=" + Application.unityVersion + " backend=" + SystemInfo.graphicsDeviceType
                + " frameTiming=" + FrameTimingManager.IsFeatureEnabled() + " file=" + (reportPath ?? "unavailable; Player.log only")
                + "; fps=active Player loop, not headset/compositor FPS; p99-equivalent is not worst-1%-average; recorder counters may lag a frame");
        }
        static ProfilerRecorder Counter(ProfilerCategory category, string name)
        {
            try
            {
                var recorder = ProfilerRecorder.StartNew(category, name, 1);
                if (recorder.Valid) return recorder;
                recorder.Dispose();
            }
            catch (ArgumentException) { }
            catch (InvalidOperationException) { }
            return default;
        }
        static long Read(ProfilerRecorder recorder) => recorder.Valid && recorder.Count > 0 ? recorder.LastValue : -1;
        void LateUpdate()
        {
            if (window == null) return;
            var now = Time.realtimeSinceStartupAsDouble;
            if (now >= nextLookup)
            {
                if (manager == null) manager = FindFirstObjectByType<GameManager>();
                hmd = InputSystem.GetDevice<XRHMD>();
                displays.Clear(); SubsystemManager.GetSubsystems(displays); display = null;
                foreach (var item in displays) if (item.running) { display = item; break; }
                gpuClock = FrameTimingManager.IsFeatureEnabled() && FrameTimingManager.GetGpuTimerFrequency() > 0;
                nextLookup = now + 1;
            }
            var currentlyTracked = hmd != null && hmd.added && hmd.enabled && hmd.isTracked.isPressed && (hmd.trackingState.ReadValue() & 3) == 3;
            if (tracked != currentlyTracked)
            {
                if (tracked) trackingLosses++;
                if (now >= nextTrackingLog)
                {
                    Emit("event tracking=" + currentlyTracked + " t=" + PerformanceWindow.Metric(now - started));
                    nextTrackingLog = now + 1; // Count every loss, rate-limit chatter from an unstable provider.
                }
            }
            tracked = currentlyTracked;
            if (!focused || paused) { previous = now; baselinePending = true; return; }
            if (baselinePending) { previous = now; baselinePending = false; return; }
            var state = manager != null ? manager.State : GameState.Ready;
            if (state != lastState) { WriteReport(now); lastState = state; }
            var ms = (now - previous) * 1000; previous = now;
            double cpu = double.NaN, gpu = double.NaN;
            if (FrameTimingManager.IsFeatureEnabled())
            {
                FrameTimingManager.CaptureFrameTimings();
                if (FrameTimingManager.GetLatestTimings(1, timings) > 0 && timings[0].frameStartTimestamp != 0
                    && timings[0].frameStartTimestamp != lastTiming)
                {
                    lastTiming = timings[0].frameStartTimestamp;
                    cpu = timings[0].cpuFrameTime;
                    if (gpuClock) gpu = timings[0].gpuFrameTime;
                }
            }
            if (window.Full) WriteReport(now);
            var mainNs = Read(main);
            window.Record(ms, state, now - started,
                cpu, gpu, mainNs > 0 ? mainNs / 1000000d : double.NaN, Read(allocated));
            if (now >= nextReport) WriteReport(now);
        }
        void OnApplicationFocus(bool value)
        {
            if (focused && !value) focusLosses++;
            focused = value;
            if (window == null) return;
            var now = Time.realtimeSinceStartupAsDouble;
            WriteReport(now); previous = now; baselinePending = true;
            Emit("event focus=" + value + " t=" + PerformanceWindow.Metric(now - started));
        }
        void OnApplicationPause(bool value)
        {
            paused = value;
            if (window == null) return;
            var now = Time.realtimeSinceStartupAsDouble;
            WriteReport(now); previous = now; baselinePending = true;
            Emit("event pause=" + value + " t=" + PerformanceWindow.Metric(now - started));
        }
        void WriteReport(double now)
        {
            if (window == null || window.Count == 0) { nextReport = now + 30; return; }
            var p99 = window.P99Ms();
            var text = new StringBuilder(1000);
            text.Append("window t=").Append(PerformanceWindow.Metric(now - started)).Append(" samples=").Append(window.Count)
                .Append(" ready/playing/over=").Append(window.States[0]).Append('/').Append(window.States[1]).Append('/').Append(window.States[2]);
            Metric(text, "avgPlayerFps", window.AverageFps); Metric(text, "p99Ms", p99); Metric(text, "p99EquivalentFps", 1000 / p99);
            Metric(text, "worstMs", window.WorstMs); text.Append(" spikesOver33ms=").Append(window.SpikeCount);
            Metric(text, "cpuFrameFTMms", window.CpuMeanMs); Metric(text, "gpuFrameFTMms", window.GpuMeanMs);
            text.Append(" cpu/gpuSamples=").Append(window.CpuCount).Append('/').Append(window.GpuCount);
            Metric(text, "mainThreadInclusiveRecorderMs", window.MainMeanMs);
            Metric(text, "gcAllocMeanBytes", window.AllocationMeanBytes);
            Metric(text, "gcAllocPeakBytes", double.IsNaN(window.AllocationMeanBytes) ? double.NaN : window.PeakAllocationBytes);
            Metric(text, "gcUsedMB", BytesToMb(Read(used))); Metric(text, "gcReservedMB", BytesToMb(Read(reserved)));
            var collections = GC.CollectionCount(0); text.Append(" gc0Collections=").Append(collections - gcCollections); gcCollections = collections;
            Metric(text, "drawCallsLatest", Available(Read(draws))); Metric(text, "trianglesLatest", Available(Read(triangles)));
            float hz; int dropped;
            Metric(text, "xrRefreshHz", display != null && display.running && display.TryGetDisplayRefreshRate(out hz) && hz > 0 ? hz : double.NaN);
            Metric(text, "xrDroppedCumulative", display != null && display.running && display.TryGetDroppedFrameCount(out dropped) ? dropped : double.NaN);
            text.Append(" focusLosses=").Append(focusLosses).Append(" trackingLosses=").Append(trackingLosses).Append(" topSpikes=");
            for (var i = 0; i < 8 && window.SpikeMs[i] > 0; i++)
                text.Append(PerformanceWindow.Metric(window.SpikeAt[i])).Append('s').Append(':').Append(PerformanceWindow.Metric(window.SpikeMs[i])).Append("ms/").Append(window.SpikeState[i]).Append(';');
            Emit(text.ToString()); window.Clear(); nextReport = now + 30;
            // Report formatting/I/O occurs occasionally; its next frame is measured, not silently discarded.
        }
        static double Available(long value) => value < 0 ? double.NaN : value;
        static double BytesToMb(long value) => value < 0 ? double.NaN : value / 1048576d;
        static void Metric(StringBuilder text, string name, double value) => text.Append(' ').Append(name).Append('=').Append(PerformanceWindow.Metric(value));
        void Emit(string message)
        {
            Debug.Log("[PERF] " + message);
            if (reportPath == null) return;
            try { File.AppendAllText(reportPath, message + Environment.NewLine); }
            catch (IOException) { reportPath = null; }
            catch (UnauthorizedAccessException) { reportPath = null; }
        }
        void OnApplicationQuit() => WriteReport(Time.realtimeSinceStartupAsDouble);
        void OnDisable()
        {
            WriteReport(Time.realtimeSinceStartupAsDouble);
            if (main.Valid) main.Dispose(); if (used.Valid) used.Dispose(); if (reserved.Valid) reserved.Dispose();
            if (allocated.Valid) allocated.Dispose(); if (draws.Valid) draws.Dispose(); if (triangles.Valid) triangles.Dispose();
            window = null;
        }
    }
}