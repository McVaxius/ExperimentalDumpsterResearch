using Dalamud.Configuration;
using Dalamud.Plugin;
using System.Collections.Generic;

namespace ExperimentalDumpsterResearch;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsEnabled { get; set; } = true;
    public bool ShowMainWindow { get; set; } = true;
    public bool ShowDebugInfo { get; set; } = true;

    // Video playback settings
    public string VideoPath { get; set; } = @"\\zell\ff14\frenrider icon options\grok-video-3166b41b-3ede-4e30-8130-41472ebbfaa6.mp4";
    public bool LoopVideo { get; set; } = true;
    public bool AutoPlay { get; set; } = false;
    public float Volume { get; set; } = 0.5f;
    public bool MuteAudio { get; set; } = true;
    public float PlaybackSpeed { get; set; } = 1.0f;

    // Video display settings
    public int VideoWindowWidth { get; set; } = 640;
    public int VideoWindowHeight { get; set; } = 480;
    public bool MaintainAspectRatio { get; set; } = true;
    public bool AlwaysOnTop { get; set; } = false;
    public float Opacity { get; set; } = 1.0f;

    // Test bench settings
    public string TestVideoPath { get; set; } = @"\\zell\ff14\frenrider icon options\grok-video-3166b41b-3ede-4e30-8130-41472ebbfaa6.mp4";
    public bool EnablePerformanceMonitoring { get; set; } = true;
    public int BenchmarkIterations { get; set; } = 10;
    public bool LogDetailedMetrics { get; set; } = false;

    // Experimental features
    public bool EnableVideoOverlays { get; set; } = false;
    public bool EnableGpuAcceleration { get; set; } = true;
    public bool EnableMultiThreading { get; set; } = true;
    public string VideoBackend { get; set; } = "DirectX"; // DirectX, OpenGL, Software

    // Research projects
    public List<ResearchProject> ActiveProjects { get; set; } = new();
    public string CurrentProject { get; set; } = "VideoPlayback";

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.SavePluginConfig(this);
        
        // Initialize default projects
        if (ActiveProjects.Count == 0)
        {
            ActiveProjects.Add(new ResearchProject
            {
                Name = "VideoPlayback",
                Description = "MP4 video playback in Dalamud",
                Status = "Active",
                Progress = 0
            });
        }
    }

    public void Save() => Service<IDalamudPluginInterface>.Instance?.SavePluginConfig(this);
}

[Serializable]
public class ResearchProject
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Pending"; // Pending, Active, Completed, Failed
    public int Progress { get; set; } = 0; // 0-100
    public List<string> Notes { get; set; } = new();
    public DateTime LastModified { get; set; } = DateTime.Now;
}
