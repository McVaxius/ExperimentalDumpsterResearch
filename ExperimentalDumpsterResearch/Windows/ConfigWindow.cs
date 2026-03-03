using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Numerics;

namespace ExperimentalDumpsterResearch.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration config;
    private readonly Plugin plugin;
    private readonly IChatGui chatGui;

    public ConfigWindow(Plugin plugin, Configuration configuration) 
        : base("EDR Configuration", (Dalamud.Bindings.ImGui.ImGuiWindowFlags)(ImGuiNET.ImGuiWindowFlags.NoScrollbar | ImGuiNET.ImGuiWindowFlags.NoScrollWithMouse))
    {
        Size = new Vector2(600, 700);
        SizeCondition = (Dalamud.Bindings.ImGui.ImGuiCond)ImGuiNET.ImGuiCond.FirstUseEver;
        
        this.config = configuration;
        this.plugin = plugin;
        this.chatGui = plugin.GetChatGui();
    }

    public void Dispose() { }

    public override void Draw()
    {
        // General Settings
        if (ImGui.CollapsingHeader("General Settings"))
        {
            var isEnabled = config.IsEnabled;
            if (ImGui.Checkbox("Enable Plugin", ref isEnabled))
            {
                config.IsEnabled = isEnabled;
            }
            
            var showMainWindow = config.ShowMainWindow;
            if (ImGui.Checkbox("Show Main Window", ref showMainWindow))
            {
                config.ShowMainWindow = showMainWindow;
            }
            
            var showDebugInfo = config.ShowDebugInfo;
            if (ImGui.Checkbox("Show Debug Info", ref showDebugInfo))
            {
                config.ShowDebugInfo = showDebugInfo;
            }
        }

        // Video Playback Settings
        if (ImGui.CollapsingHeader("🎬 Video Playback"))
        {
            ImGui.Text("Video File:");
            var videoPath = config.VideoPath;
            if (ImGui.InputText("##videopath", ref videoPath, 500))
            {
                config.VideoPath = videoPath;
            }

            ImGui.SameLine();
            if (ImGui.Button("Browse...##video"))
            {
                // File browser would go here
                chatGui.Print("[EDR] Manual file path entry required");
            }

            var loopVideo = config.LoopVideo;
            if (ImGui.Checkbox("Loop Video", ref loopVideo))
            {
                config.LoopVideo = loopVideo;
            }
            
            var autoPlay = config.AutoPlay;
            if (ImGui.Checkbox("Auto Play", ref autoPlay))
            {
                config.AutoPlay = autoPlay;
            }
            
            var muteAudio = config.MuteAudio;
            if (ImGui.Checkbox("Mute Audio", ref muteAudio))
            {
                config.MuteAudio = muteAudio;
            }
            
            if (!config.MuteAudio)
            {
                var volume = config.Volume;
                if (ImGui.SliderFloat("Volume", ref volume, 0.0f, 1.0f))
                {
                    config.Volume = volume;
                }
            }
            
            var playbackSpeed = config.PlaybackSpeed;
            if (ImGui.SliderFloat("Playback Speed", ref playbackSpeed, 0.25f, 2.0f))
            {
                config.PlaybackSpeed = playbackSpeed;
            }

            // Test video path
            ImGui.Separator();
            ImGui.Text("Test Video:");
            var testPath = config.TestVideoPath;
            if (ImGui.InputText("##testvideopath", ref testPath, 500))
            {
                config.TestVideoPath = testPath;
            }
        }

        // Video Display Settings
        if (ImGui.CollapsingHeader("🖼️ Video Display"))
        {
            var windowWidth = config.VideoWindowWidth;
            if (ImGui.SliderInt("Window Width", ref windowWidth, 320, 1920))
            {
                config.VideoWindowWidth = windowWidth;
            }
            
            var windowHeight = config.VideoWindowHeight;
            if (ImGui.SliderInt("Window Height", ref windowHeight, 240, 1080))
            {
                config.VideoWindowHeight = windowHeight;
            }
            var maintainAspectRatio = config.MaintainAspectRatio;
            if (ImGui.Checkbox("Maintain Aspect Ratio", ref maintainAspectRatio))
            {
                config.MaintainAspectRatio = maintainAspectRatio;
            }
            
            var alwaysOnTop = config.AlwaysOnTop;
            if (ImGui.Checkbox("Always On Top", ref alwaysOnTop))
            {
                config.AlwaysOnTop = alwaysOnTop;
            }
            
            var opacity = config.Opacity;
            if (ImGui.SliderFloat("Window Opacity", ref opacity, 0.1f, 1.0f))
            {
                config.Opacity = opacity;
            }
        }

        // Test Bench Settings
        if (ImGui.CollapsingHeader("🧪 Test Bench"))
        {
            var enablePerfMonitoring = config.EnablePerformanceMonitoring;
            if (ImGui.Checkbox("Enable Performance Monitoring", ref enablePerfMonitoring))
            {
                config.EnablePerformanceMonitoring = enablePerfMonitoring;
            }
            
            var benchmarkIterations = config.BenchmarkIterations;
            if (ImGui.SliderInt("Benchmark Iterations", ref benchmarkIterations, 1, 100))
            {
                config.BenchmarkIterations = benchmarkIterations;
            }
            
            var logDetailedMetrics = config.LogDetailedMetrics;
            if (ImGui.Checkbox("Log Detailed Metrics", ref logDetailedMetrics))
            {
                config.LogDetailedMetrics = logDetailedMetrics;
            }
        }

        // Experimental Features
        if (ImGui.CollapsingHeader("🔬 Experimental Features"))
        {
            var enableVideoOverlays = config.EnableVideoOverlays;
            if (ImGui.Checkbox("Enable Video Overlays", ref enableVideoOverlays))
            {
                config.EnableVideoOverlays = enableVideoOverlays;
            }
            
            var enableGpuAcceleration = config.EnableGpuAcceleration;
            if (ImGui.Checkbox("Enable GPU Acceleration", ref enableGpuAcceleration))
            {
                config.EnableGpuAcceleration = enableGpuAcceleration;
            }
            
            var enableMultiThreading = config.EnableMultiThreading;
            if (ImGui.Checkbox("Enable Multi-threading", ref enableMultiThreading))
            {
                config.EnableMultiThreading = enableMultiThreading;
            }
            
            ImGui.Text("Video Backend:");
            var backend = config.VideoBackend;
            if (ImGui.BeginCombo("##backend", backend))
            {
                if (ImGui.Selectable("DirectX"))
                    config.VideoBackend = "DirectX";
                if (ImGui.Selectable("OpenGL"))
                    config.VideoBackend = "OpenGL";
                if (ImGui.Selectable("Software"))
                    config.VideoBackend = "Software";
                ImGui.EndCombo();
            }

            ImGui.TextColored(new Vector4(1, 1, 0, 1), "⚠️ Experimental features may be unstable");
        }

        // Research Projects
        if (ImGui.CollapsingHeader("🔬 Research Projects"))
        {
            ImGui.Text("Active Projects:");
            
            // Add new project
            if (ImGui.Button("Add Project"))
            {
                config.ActiveProjects.Add(new ResearchProject
                {
                    Name = "New Project",
                    Description = "Enter description",
                    Status = "Pending",
                    Progress = 0
                });
            }

            // Project list
            for (int i = 0; i < config.ActiveProjects.Count; i++)
            {
                var project = config.ActiveProjects[i];
                ImGui.PushID(i);
                
                ImGui.Text($"Project {i + 1}:");
                
                var name = project.Name;
                if (ImGui.InputText($"##name{i}", ref name, 100))
                {
                    project.Name = name;
                }
                
                var description = project.Description;
                if (ImGui.InputText($"##desc{i}", ref description, 200))
                {
                    project.Description = description;
                }
                
                var status = project.Status;
                if (ImGui.BeginCombo($"##status{i}", status))
                {
                    if (ImGui.Selectable("Pending"))
                        project.Status = "Pending";
                    if (ImGui.Selectable("Active"))
                        project.Status = "Active";
                    if (ImGui.Selectable("Completed"))
                        project.Status = "Completed";
                    if (ImGui.Selectable("Failed"))
                        project.Status = "Failed";
                    ImGui.EndCombo();
                }
                
                var progress = project.Progress;
                if (ImGui.SliderInt($"##progress{i}", ref progress, 0, 100))
                {
                    project.Progress = progress;
                }
                
                if (ImGui.Button($"Select##select{i}"))
                {
                    config.CurrentProject = project.Name;
                }
                
                ImGui.SameLine();
                if (ImGui.Button($"Remove##remove{i}"))
                {
                    config.ActiveProjects.RemoveAt(i);
                    i--;
                }
                
                ImGui.PopID();
                ImGui.Separator();
            }
        }

        ImGui.Separator();

        // Save/Reset buttons
        if (ImGui.Button("Save Configuration"))
        {
            config.Save();
            chatGui.Print("[EDR] Configuration saved");
        }

        ImGui.SameLine();
        if (ImGui.Button("Reset to Defaults"))
        {
            // Reset to defaults
            config.IsEnabled = true;
            config.ShowMainWindow = true;
            config.ShowDebugInfo = true;
            config.VideoPath = "";
            config.LoopVideo = true;
            config.AutoPlay = false;
            config.Volume = 0.5f;
            config.MuteAudio = true;
            config.PlaybackSpeed = 1.0f;
            config.VideoWindowWidth = 640;
            config.VideoWindowHeight = 480;
            config.MaintainAspectRatio = true;
            config.AlwaysOnTop = false;
            config.Opacity = 1.0f;
            config.TestVideoPath = "";
            config.EnablePerformanceMonitoring = true;
            config.BenchmarkIterations = 10;
            config.LogDetailedMetrics = false;
            config.EnableVideoOverlays = false;
            config.EnableGpuAcceleration = true;
            config.EnableMultiThreading = true;
            config.VideoBackend = "DirectX";
            config.CurrentProject = "VideoPlayback";
            
            // Reset projects
            config.ActiveProjects.Clear();
            config.ActiveProjects.Add(new ResearchProject
            {
                Name = "VideoPlayback",
                Description = "MP4 video playback in Dalamud",
                Status = "Active",
                Progress = 0
            });
            
            config.Save();
            chatGui.Print("[EDR] Configuration reset to defaults");
        }

        // Info section
        ImGui.Separator();
        ImGui.Text("🗑️ Experimental Dumpster Research");
        ImGui.Text("Here at The Dumpster Fire we are researching new ways");
        ImGui.Text("to cook garbage, perhaps one day we will have a smartpster.");
        
        ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), "Version 1.0.0 - Experimental");
    }
}
