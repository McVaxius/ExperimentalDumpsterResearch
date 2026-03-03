using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;
using System.IO;
using System.Linq;
using ExperimentalDumpsterResearch.Services;

namespace ExperimentalDumpsterResearch.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Configuration config;
    private readonly VideoPlaybackService videoService;
    private readonly TestBenchService testBenchService;
    private readonly IChatGui chatGui;

    private string videoPathInput = "";
    private bool showTestResults = false;

    public MainWindow(Plugin plugin, Configuration configuration, 
        VideoPlaybackService videoService, TestBenchService testBenchService) 
        : base("Experimental Dumpster Research", (Dalamud.Bindings.ImGui.ImGuiWindowFlags)(ImGuiNET.ImGuiWindowFlags.NoScrollbar | ImGuiNET.ImGuiWindowFlags.NoScrollWithMouse))
    {
        Size = new Vector2(500, 600);
        SizeCondition = (Dalamud.Bindings.ImGui.ImGuiCond)ImGuiNET.ImGuiCond.FirstUseEver;
        
        this.config = configuration;
        this.videoService = videoService;
        this.testBenchService = testBenchService;
        this.chatGui = plugin.GetChatGui();
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("🗑️ Experimental Dumpster Research");
        ImGui.Text("Researching new ways to cook garbage...");
        ImGui.Separator();

        // Current project status
        ImGui.Text($"Current Project: {config.CurrentProject}");
        var project = config.ActiveProjects.FirstOrDefault(p => p.Name == config.CurrentProject);
        if (project != null)
        {
            ImGui.Text($"Status: {project.Status}");
            ImGui.Text($"Progress: {project.Progress}%");
            ImGui.ProgressBar(project.Progress / 100f, new Vector2(200, 20));
        }

        ImGui.Separator();

        // Video playback section
        if (ImGui.CollapsingHeader("🎬 Video Playback Test"))
        {
            // Video file selection
            ImGui.Text("Video File:");
            ImGui.InputText("##videopath", ref videoPathInput, 500);
            ImGui.SameLine();
            if (ImGui.Button("Browse..."))
            {
                // This would open a file dialog
                chatGui.Print("[EDR] Use /edr config to set video path");
            }

            if (ImGui.Button("Play Video"))
            {
                if (!string.IsNullOrEmpty(videoPathInput))
                {
                    _ = videoService.PlayVideo(videoPathInput);
                }
                else if (!string.IsNullOrEmpty(config.TestVideoPath))
                {
                    _ = videoService.PlayVideo(config.TestVideoPath);
                }
                else
                {
                    chatGui.Print("[EDR] No video file specified");
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Stop Video"))
            {
                videoService.StopVideo();
            }

            // Video status
            ImGui.Text($"Status: {(videoService.IsPlaying ? "Playing" : "Stopped")}");
            if (!string.IsNullOrEmpty(videoService.CurrentVideo))
            {
                ImGui.Text($"Current: {Path.GetFileName(videoService.CurrentVideo)}");
            }

            // FFmpeg status
            var vlcAvailable = videoService.IsVLCAvailable();
            var ffmpegAvailable = videoService.IsFFmpegAvailable();
            ImGui.Text($"VLC: {(vlcAvailable ? "Available" : "Not Found")}");
            ImGui.Text($"FFmpeg: {(ffmpegAvailable ? "Available" : "Not Found")}");
            if (!vlcAvailable)
            {
                ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), "Please install VLC for video playback");
                ImGui.Text("Download from: https://www.videolan.org/vlc/");
            }
        }

        ImGui.Separator();

        // Test bench section
        if (ImGui.CollapsingHeader("🧪 Test Bench"))
        {
            if (ImGui.Button("Run Current Test"))
            {
                _ = testBenchService.RunCurrentTest();
            }

            ImGui.SameLine();
            if (ImGui.Button("Performance Benchmark"))
            {
                _ = testBenchService.RunPerformanceBenchmark();
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear Results"))
            {
                testBenchService.ClearTestResults();
            }

            // Quick tests
            if (ImGui.Button("Quick Video Test"))
            {
                _ = testBenchService.StartVideoTest();
            }

            ImGui.SameLine();
            if (ImGui.Button("FFmpeg Test"))
            {
                chatGui.Print($"[EDR] VLC Available: {videoService.IsVLCAvailable()}");
                chatGui.Print($"[EDR] FFmpeg Available: {videoService.IsFFmpegAvailable()}");
            }

            // Test results toggle
            ImGui.Checkbox("Show Test Results", ref showTestResults);
        }

        ImGui.Separator();

        // Research projects section
        if (ImGui.CollapsingHeader("🔬 Research Projects"))
        {
            foreach (var researchProject in config.ActiveProjects)
            {
                ImGui.PushID(researchProject.Name);
                
                var statusColor = researchProject.Status switch
                {
                    "Active" => new Vector4(0, 1, 0, 1),
                    "Completed" => new Vector4(0, 0.5f, 1, 1),
                    "Failed" => new Vector4(1, 0, 0, 1),
                    _ => new Vector4(0.7f, 0.7f, 0.7f, 1)
                };

                ImGui.TextColored(statusColor, $"{researchProject.Name} - {researchProject.Status}");
                ImGui.Text($"  {researchProject.Description}");
                ImGui.ProgressBar(researchProject.Progress / 100f, new Vector2(150, 15));
                
                if (ImGui.Button($"Select##{researchProject.Name}"))
                {
                    config.CurrentProject = researchProject.Name;
                }
                
                ImGui.PopID();
            }
        }

        ImGui.Separator();

        // System info
        if (ImGui.CollapsingHeader("💻 System Info"))
        {
            ImGui.Text($"Plugin Version: {config.Version}");
            ImGui.Text($"Framework: .NET 10");
            ImGui.Text($"Video Backend: {config.VideoBackend}");
            ImGui.Text($"GPU Acceleration: {(config.EnableGpuAcceleration ? "Enabled" : "Disabled")}");
            ImGui.Text($"Multi-threading: {(config.EnableMultiThreading ? "Enabled" : "Disabled")}");
        }

        // Test results display
        if (showTestResults)
        {
            ImGui.Separator();
            ImGui.Text("📊 Recent Test Results:");
            
            var results = testBenchService.GetTestResults().TakeLast(10).ToList();
            foreach (var result in results)
            {
                var statusColor = result.Success ? new Vector4(0, 1, 0, 1) : new Vector4(1, 0, 0, 1);
                var duration = result.Duration?.TotalMilliseconds.ToString("F0") ?? "N/A";
                
                ImGui.TextColored(statusColor, $"{result.TestName}: {duration}ms");
                if (config.ShowDebugInfo && !string.IsNullOrEmpty(result.Details))
                {
                    ImGui.Text($"  {result.Details}");
                }
            }
        }

        ImGui.Separator();

        // Actions
        if (ImGui.Button("Config"))
        {
            // Open config window
            chatGui.Print("[EDR] Use /edr config to open configuration");
        }

        ImGui.SameLine();
        if (ImGui.Button("Help"))
        {
            chatGui.Print("[EDR] Commands: /edr config, /edr test, /edr play, /edr stop, /edr bench");
        }
    }
}
