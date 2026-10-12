using System;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace NeonReflexVR.Tests
{
    public class PerformanceDiagnosticsTests
    {
        [Test]
        public void KnownIntervalsProduceCorrectPlayerFpsAndNearestRankP99()
        {
            var window = new PerformanceWindow(1000);
            for (var i = 0; i < 1000; i++) window.Record(i < 989 ? 10 : 40, GameState.Playing, i * 0.01, 8, 9);
            Assert.That(window.AverageFps, Is.EqualTo(1000000d / 10330d).Within(0.001));
            Assert.That(window.P99Ms(), Is.EqualTo(40));
            Assert.That(1000 / window.P99Ms(), Is.EqualTo(25));
            Assert.That(window.SpikeCount, Is.EqualTo(11));
            Assert.That(window.SpikeMs[0], Is.EqualTo(40));
            Assert.That(window.CpuMeanMs, Is.EqualTo(8));
            Assert.That(window.GpuMeanMs, Is.EqualTo(9));
        }
        [Test]
        public void UnsupportedZeroOrInvalidMetricsRemainUnavailableRatherThanInvented()
        {
            var window = new PerformanceWindow();
            Assert.That(PerformanceWindow.Metric(window.AverageFps), Is.EqualTo("unavailable"));
            Assert.That(window.Record(0, GameState.Ready, 0), Is.False);
            Assert.That(window.Record(double.NaN, GameState.Ready, 0), Is.False);
            Assert.That(window.Record(double.PositiveInfinity, GameState.Ready, 0), Is.False);
            window.Record(12, GameState.Ready, 1, 0, -1);
            Assert.That(window.Count, Is.EqualTo(1));
            Assert.That(PerformanceWindow.Metric(window.GpuMeanMs), Is.EqualTo("unavailable"));
            Assert.That(PerformanceWindow.Metric(window.CpuMeanMs), Is.EqualTo("unavailable"));
            Assert.That(PerformanceWindow.Metric(window.AllocationMeanBytes), Is.EqualTo("unavailable"));
        }
        [Test]
        public void CapacityAndSpikeHistoryAreBoundedAndResetWithoutRetainingPreviousWindow()
        {
            var window = new PerformanceWindow(12);
            for (var i = 0; i < 12; i++) Assert.That(window.Record(40 + i, GameState.GameOver, i, allocation: i), Is.True);
            Assert.That(window.Full, Is.True);
            Assert.That(window.Record(1000, GameState.Playing, 20), Is.False);
            Assert.That(window.SpikeMs.Length, Is.EqualTo(8));
            Assert.That(window.SpikeMs[0], Is.EqualTo(51));
            Assert.That(window.WorstMs, Is.EqualTo(51));
            Assert.That(window.AllocationMeanBytes, Is.EqualTo(5.5));
            window.Clear(); window.Record(10, GameState.Ready, 30, allocation: 0);
            Assert.That(window.SpikeCount, Is.Zero);
            Assert.That(window.SpikeMs[0], Is.Zero);
            Assert.That(window.States, Is.EqualTo(new[] { 1, 0, 0 }));
            Assert.That(window.P99Ms(), Is.EqualTo(10));
            Assert.That(window.PeakAllocationBytes, Is.Zero);
        }
        [Test]
        public void EditorDoesNotAutomaticallyCollectOrReportPerformanceAsPlayerData()
        {
            var host = new GameObject("Diagnostics editor guard test");
            try
            {
                var diagnostics = host.AddComponent<PerformanceDiagnostics>();
                // Plain MonoBehaviour callbacks are not automatically invoked by EditMode AddComponent.
                typeof(PerformanceDiagnostics).GetMethod("OnEnable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(diagnostics, null);
                Assert.That(diagnostics.enabled, Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(host); }
            Assert.That(PlayerSettings.enableFrameTimingStats, Is.True);
        }
    }
}