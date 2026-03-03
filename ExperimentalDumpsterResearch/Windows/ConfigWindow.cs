using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System.Numerics;

namespace ExperimentalDumpsterResearch.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration config;
    private readonly Plugin plugin;

    public ConfigWindow(Plugin plugin, Configuration configuration) 
        : base("EDR Configuration", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        Size = new Vector2(600, 700);
        SizeCondition = ImGuiCond.FirstUseEver;
        
        this.config = configuration;
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // General Settings
        if (ImGui.CollapsingHeader("General Settings"))
        {
            ImGui.Checkbox("Enable Plugin", ref config.IsEnabled);
            ImGui.Checkbox("Show Main Window", ref config.ShowMainWindow);
            ImGui.Checkbox("Show Debug Info", ref config.ShowDebugInfo);
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
                Service<ChatGui>.Instance.Print("[EDR] Manual file path entry required");
            }

            ImGui.Checkbox("Loop Video", ref config.LoopVideo);
            ImGui.Checkbox("Auto Play", ref config.AutoPlay);
            ImGui.Checkbox("Mute Audio", ref config.MuteAudio);
            
            if (!config.MuteAudio)
            {
                ImGui.SliderFloat("Volume", ref config.Volume, 0.0f, 1.0f);
            }
            
            ImGui.SliderFloat("Playback Speed", ref config.PlaybackSpeed, 0.25f, 2.0f);

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
            ImGui.SliderInt("Window Width", ref config.VideoWindowWidth, 320, 1920);
            ImGui.SliderInt("Window Height", ref config.VideoWindowHeight, 240, 1080);
            ImGui.Checkbox("Maintain Aspect Ratio", ref config.MaintainAspectRatio);
            ImGui.Checkbox("Always On Top", ref config.AlwaysOnTop);
            ImGui.SliderFloat("Window Opacity", ref config.Opacity, 0.1f, 1.0f);
        }

        // Test Bench Settings
        if (ImGui.CollapsingHeader("🧪 Test Bench"))
        {
            ImGui.Checkbox("Enable Performance Monitoring", ref config.EnablePerformanceMonitoring);
            ImGui.SliderInt("Benchmark Iterations", ref config.BenchmarkIterations, 1, 100);
            ImGui.Checkbox("Log Detailed Metrics", ref config.LogDetailedMetrics);
        }

        // Experimental Features
        if (ImGui.CollapsingHeader("🔬 Experimental Features"))
        {
            ImGui.Checkbox("Enable Video Overlays", ref config.EnableVideoOverlays);
            ImGui.Checkbox("Enable GPU Acceleration", ref config.EnableGpuAcceleration);
            ImGui.Checkbox("Enable Multi-threading", ref config.EnableMultiThreading);
            
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
                
                ImGui.SliderInt($"##progress{i}", ref project.Progress, 0, 100);
                
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
            plugin.SaveConfig();
            Service<ChatGui>.Instance.Print("[EDR] Configuration saved");
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
            
            plugin.SaveConfig();
            Service<ChatGui>.Instance.Print("[EDR] Configuration reset to defaults");
        }

        // Info section
        ImGui.Separator();
        ImGui.Text("🗑️ Experimental Dumpster Research");
        ImGui.Text("Here at The Dumpster Fire we are researching new ways");
        ImGui.Text("to cook garbage, perhaps one day we will have a smartpster.");
        
        ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), "Version 1.0.0 - Experimental");
    }
}
