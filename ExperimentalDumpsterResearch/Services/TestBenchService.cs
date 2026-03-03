using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExperimentalDumpsterResearch.Services;

public class TestBenchService : IDisposable
{
    private readonly Configuration config;
    private readonly VideoPlaybackService videoService;
    private readonly IPluginLog log;
    private readonly IChatGui chat;

    private List<TestResult> testResults = new();
    private bool isRunningTests = false;

    public TestBenchService(Configuration config, VideoPlaybackService videoService, IPluginLog log, IChatGui chat)
    {
        this.config = config;
        this.videoService = videoService;
        this.log = log;
        this.chat = chat;
    }

    /// <summary>
    /// Run the current active test
    /// </summary>
    public async Task RunCurrentTest()
    {
        if (isRunningTests)
        {
            chat.Print("[EDR] Tests already running");
            return;
        }

        switch (config.CurrentProject)
        {
            case "VideoPlayback":
                await RunVideoPlaybackTests();
                break;
            default:
                chat.Print($"[EDR] Unknown test project: {config.CurrentProject}");
                break;
        }
    }

    /// <summary>
    /// Start video playback test
    /// </summary>
    public async Task StartVideoTest()
    {
        if (string.IsNullOrEmpty(config.TestVideoPath))
        {
            chat.Print("[EDR] No test video configured");
            return;
        }

        chat.Print("[EDR] Starting video playback test...");
        var success = await videoService.PlayVideo(config.TestVideoPath);
        
        var result = new TestResult
        {
            TestName = "Video Playback",
            Success = success,
            ExecutionTime = DateTime.Now,
            Details = success ? "Video started successfully" : "Failed to start video"
        };

        testResults.Add(result);
        chat.Print($"[EDR] Video test result: {(success ? "PASS" : "FAIL")}");
    }

    /// <summary>
    /// Run comprehensive video playback tests
    /// </summary>
    private async Task RunVideoPlaybackTests()
    {
        isRunningTests = true;
        chat.Print("[EDR] Starting video playback test suite...");

        try
        {
            // Test 1: VLC availability
            await TestVLCAvailability();

            // Test 2: FFmpeg availability
            await TestFFmpegAvailability();

            // Test 3: Video file validation
            await TestVideoFileValidation();

            // Test 4: Video playback
            await TestVideoPlayback();

            // Test 5: Video controls
            await TestVideoControls();

            // Test 6: Performance benchmark
            if (config.EnablePerformanceMonitoring)
            {
                await RunPerformanceBenchmark();
            }

            // Generate test report
            GenerateTestReport();
        }
        catch (Exception ex)
        {
            log.Error(ex, "[EDR] Error during test execution");
            chat.Print("[EDR] Test execution failed");
        }
        finally
        {
            isRunningTests = false;
            chat.Print("[EDR] Video playback test suite completed");
        }
    }

    /// <summary>
    /// Test VLC availability
    /// </summary>
    private async Task TestVLCAvailability()
    {
        var stopwatch = Stopwatch.StartNew();
        var available = videoService.IsVLCAvailable();
        stopwatch.Stop();

        var result = new TestResult
        {
            TestName = "VLC Availability",
            Success = available,
            ExecutionTime = DateTime.Now,
            Duration = stopwatch.Elapsed,
            Details = available ? "VLC found and accessible" : "VLC not found - please install"
        };

        testResults.Add(result);
        log.Info($"[EDR] VLC test: {(available ? "PASS" : "FAIL")} ({stopwatch.ElapsedMilliseconds}ms)");
    }

    /// <summary>
    /// Test FFmpeg availability
    /// </summary>
    private async Task TestFFmpegAvailability()
    {
        var stopwatch = Stopwatch.StartNew();
        var available = videoService.IsFFmpegAvailable();
        stopwatch.Stop();

        var result = new TestResult
        {
            TestName = "FFmpeg Availability",
            Success = available,
            ExecutionTime = DateTime.Now,
            Duration = stopwatch.Elapsed,
            Details = available ? "FFmpeg found and accessible" : "FFmpeg not found - optional for metadata"
        };

        testResults.Add(result);
        log.Info($"[EDR] FFmpeg test: {(available ? "PASS" : "FAIL")} ({stopwatch.ElapsedMilliseconds}ms)");
    }

    /// <summary>
    /// Test video file validation
    /// </summary>
    private async Task TestVideoFileValidation()
    {
        var stopwatch = Stopwatch.StartNew();
        
        if (string.IsNullOrEmpty(config.TestVideoPath))
        {
            testResults.Add(new TestResult
            {
                TestName = "Video File Validation",
                Success = false,
                ExecutionTime = DateTime.Now,
                Duration = stopwatch.Elapsed,
                Details = "No test video configured"
            });
            return;
        }

        var fileExists = File.Exists(config.TestVideoPath);
        var videoInfo = fileExists ? videoService.GetVideoInfo(config.TestVideoPath) : null;
        
        stopwatch.Stop();

        var result = new TestResult
        {
            TestName = "Video File Validation",
            Success = fileExists && videoInfo != null,
            ExecutionTime = DateTime.Now,
            Duration = stopwatch.Elapsed,
            Details = fileExists 
                ? (videoInfo != null 
                    ? $"Valid video file: {videoInfo.Width}x{videoInfo.Height}, {videoInfo.Duration:hh\\:mm\\:ss}"
                    : "File exists but invalid format")
                : "Test video file not found"
        };

        testResults.Add(result);
        log.Info($"[EDR] Video validation test: {(result.Success ? "PASS" : "FAIL")} ({stopwatch.ElapsedMilliseconds}ms)");
    }

    /// <summary>
    /// Test video playback functionality
    /// </summary>
    private async Task TestVideoPlayback()
    {
        var stopwatch = Stopwatch.StartNew();
        
        if (string.IsNullOrEmpty(config.TestVideoPath))
        {
            testResults.Add(new TestResult
            {
                TestName = "Video Playback",
                Success = false,
                ExecutionTime = DateTime.Now,
                Duration = stopwatch.Elapsed,
                Details = "No test video configured"
            });
            return;
        }

        var playbackSuccess = await videoService.PlayVideo(config.TestVideoPath);
        
        // Wait a moment to see if playback starts
        await Task.Delay(2000);
        
        var isCurrentlyPlaying = videoService.IsPlaying;
        
        stopwatch.Stop();

        var result = new TestResult
        {
            TestName = "Video Playback",
            Success = playbackSuccess && isCurrentlyPlaying,
            ExecutionTime = DateTime.Now,
            Duration = stopwatch.Elapsed,
            Details = playbackSuccess 
                ? (isCurrentlyPlaying ? "Video playing successfully" : "Video started but stopped")
                : "Failed to start video playback"
        };

        testResults.Add(result);
        log.Info($"[EDR] Video playback test: {(result.Success ? "PASS" : "FAIL")} ({stopwatch.ElapsedMilliseconds}ms)");
    }

    /// <summary>
    /// Test video controls
    /// </summary>
    private async Task TestVideoControls()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Test stop functionality
            videoService.StopVideo();
            await Task.Delay(500);
            
            var stoppedCorrectly = !videoService.IsPlaying;
            
            stopwatch.Stop();

            var result = new TestResult
            {
                TestName = "Video Controls",
                Success = stoppedCorrectly,
                ExecutionTime = DateTime.Now,
                Duration = stopwatch.Elapsed,
                Details = stoppedCorrectly ? "Video controls working correctly" : "Video stop control failed"
            };

            testResults.Add(result);
            log.Info($"[EDR] Video controls test: {(result.Success ? "PASS" : "FAIL")} ({stopwatch.ElapsedMilliseconds}ms)");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            log.Error(ex, "[EDR] Video controls test failed");
            
            testResults.Add(new TestResult
            {
                TestName = "Video Controls",
                Success = false,
                ExecutionTime = DateTime.Now,
                Duration = stopwatch.Elapsed,
                Details = $"Exception: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Run performance benchmark
    /// </summary>
    public async Task RunPerformanceBenchmark()
    {
        chat.Print("[EDR] Starting performance benchmark...");
        
        var benchmarkResults = new List<long>();
        
        for (int i = 0; i < config.BenchmarkIterations; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Test video startup time
            if (!string.IsNullOrEmpty(config.TestVideoPath))
            {
                await videoService.PlayVideo(config.TestVideoPath);
                await Task.Delay(1000); // Let it play briefly
                videoService.StopVideo();
            }
            
            stopwatch.Stop();
            benchmarkResults.Add(stopwatch.ElapsedMilliseconds);
            
            if (config.LogDetailedMetrics)
            {
                log.Info($"[EDR] Benchmark iteration {i + 1}: {stopwatch.ElapsedMilliseconds}ms");
            }
            
            await Task.Delay(1000); // Brief pause between iterations
        }

        var avgTime = benchmarkResults.Average();
        var minTime = benchmarkResults.Min();
        var maxTime = benchmarkResults.Max();

        var result = new TestResult
        {
            TestName = "Performance Benchmark",
            Success = true,
            ExecutionTime = DateTime.Now,
            Details = $"Avg: {avgTime:F2}ms, Min: {minTime}ms, Max: {maxTime}ms over {config.BenchmarkIterations} iterations"
        };

        testResults.Add(result);
        chat.Print($"[EDR] Benchmark complete - Avg: {avgTime:F2}ms");
    }

    /// <summary>
    /// Generate and display test report
    /// </summary>
    private void GenerateTestReport()
    {
        var passed = testResults.Count(r => r.Success);
        var failed = testResults.Count(r => !r.Success);
        var total = testResults.Count;

        chat.Print($"[EDR] Test Report: {passed}/{total} passed, {failed} failed");

        if (config.ShowDebugInfo)
        {
            foreach (var result in testResults.TakeLast(10)) // Show last 10 results
            {
                var status = result.Success ? "PASS" : "FAIL";
                var duration = result.Duration?.TotalMilliseconds.ToString("F0") ?? "N/A";
                chat.Print($"[EDR] {status} {result.TestName}: {duration}ms - {result.Details}");
            }
        }
    }

    /// <summary>
    /// Get all test results
    /// </summary>
    public List<TestResult> GetTestResults()
    {
        return testResults.ToList();
    }

    /// <summary>
    /// Clear test results
    /// </summary>
    public void ClearTestResults()
    {
        testResults.Clear();
        chat.Print("[EDR] Test results cleared");
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}

public class TestResult
{
    public string TestName { get; set; } = "";
    public bool Success { get; set; }
    public DateTime ExecutionTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public string Details { get; set; } = "";
    public Dictionary<string, object> Metrics { get; set; } = new();
}
