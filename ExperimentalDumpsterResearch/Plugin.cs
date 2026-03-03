using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.IoC;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using ExperimentalDumpsterResearch.Services;
using ExperimentalDumpsterResearch.Windows;
using Dalamud.Plugin.Ipc;

namespace ExperimentalDumpsterResearch;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    public string Name => "Experimental Dumpster Research";
    
    private readonly WindowSystem windowSystem = new("ExperimentalDumpsterResearch");
    private Configuration configuration;
    private MainWindow mainWindow;
    private ConfigWindow configWindow;

    // Experimental services
    private VideoPlaybackService videoService;
    private TestBenchService testBenchService;

    // IPC providers
    private readonly ICallGateProvider<string, string> _playVideoIpc;
    private readonly ICallGateProvider<bool> _stopVideoIpc;
    private readonly ICallGateProvider<List<string>> _getVideosIpc;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        configuration.Initialize(pluginInterface);
        configuration.SetPluginInterface(pluginInterface);

        // Initialize experimental services
        videoService = new VideoPlaybackService(configuration, Log, ChatGui);
        testBenchService = new TestBenchService(configuration, videoService, Log, ChatGui);

        mainWindow = new MainWindow(this, configuration, videoService, testBenchService);
        configWindow = new ConfigWindow(this, configuration);

        windowSystem.AddWindow(mainWindow);
        windowSystem.AddWindow(configWindow);

        // Command registration following PlogonRules Section 5
        CommandManager.AddHandler("/dumpster", new CommandInfo(OnCommand)
        {
            HelpMessage = "Experimental Dumpster Research commands"
        });

        CommandManager.AddHandler("/edr", new CommandInfo(OnCommand)
        {
            HelpMessage = "Experimental Dumpster Research commands (short)"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

        Log.Information("[EDR] Plugin initialized successfully");
    }

    public void Dispose()
    {
        windowSystem.RemoveAllWindows();
        mainWindow?.Dispose();
        configWindow?.Dispose();
        videoService?.Dispose();
        testBenchService?.Dispose();

        CommandManager.RemoveHandler("/dumpster");
        CommandManager.RemoveHandler("/edr");

        Log.Information("[EDR] Plugin disposed");
    }

    private void OnCommand(string command, string args)
    {
        try
        {
            switch (args.ToLower())
            {
                case "config":
                case "":
                    // Don't open config window to avoid ImGui crash
                    ChatGui.Print("[EDR] Config window disabled due to ImGui issues");
                    ChatGui.Print("[EDR] Use commands: status, play, stop, forcestop, test, bench, setvideo");
                    break;
                case "video":
                    // Don't open main window to avoid ImGui crash
                    ChatGui.Print("[EDR] Video window disabled due to ImGui issues");
                    ChatGui.Print("[EDR] Use commands: status, play, stop, forcestop, test, bench, setvideo");
                    break;
                case "status":
                    ShowStatus();
                    break;
                case "test":
                    _ = testBenchService.RunCurrentTest();
                    break;
                case "play":
                    PlayVideo();
                    break;
                case "stop":
                    StopVideo();
                    break;
                case "forcestop":
                    ForceStopVideo();
                    break;
                case "bench":
                    _ = testBenchService.RunPerformanceBenchmark();
                    break;
                case "overlay":
                    ChatGui.Print("[EDR] Overlay disabled due to ImGui crashes on this system");
                    ChatGui.Print("[EDR] Use commands: play, stop, status instead");
                    break;
                case "setvideo":
                    SetVideoCommand();
                    break;
                default:
                    if (args.ToLower().StartsWith("setvideo "))
                    {
                        SetVideoPath(args.Substring(9).Trim());
                    }
                    else
                    {
                        ChatGui.Print($"[EDR] Unknown command: {args}");
                        ChatGui.Print("[EDR] Available: status, play, stop, forcestop, test, bench, overlay, setvideo <path>");
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[EDR] Command error: {command} {args}");
            ChatGui.Print("[EDR] Command failed - check /xllog for details");
        }
    }

    private void PlayVideo()
    {
        try
        {
            if (string.IsNullOrEmpty(configuration.VideoPath))
            {
                ChatGui.Print("[EDR] No video file set. Use: /edr setvideo <path>");
                return;
            }

            if (!File.Exists(configuration.VideoPath))
            {
                ChatGui.Print($"[EDR] Video file not found: {configuration.VideoPath}");
                ChatGui.Print("[EDR] Use: /edr setvideo <path> to set a valid video file");
                return;
            }

            // Show fake overlay messages
            ShowFakeOverlay();

            ChatGui.Print($"[EDR] Starting video: {Path.GetFileName(configuration.VideoPath)}");
            _ = videoService.PlayVideo(configuration.VideoPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[EDR] Play command failed");
            ChatGui.Print("[EDR] Failed to start video - check /xllog");
        }
    }

    private void ShowFakeOverlay()
    {
        // Simulate in-game overlay with chat messages
        ChatGui.Print("╔══════════════════════════════════════════════════════════════╗");
        ChatGui.Print("║                    🎬 VIDEO OVERLAY 🎬                      ║");
        ChatGui.Print("║                                                              ║");
        ChatGui.Print($"║  Now Playing: {Path.GetFileName(configuration.VideoPath).PadRight(45)} ║");
        ChatGui.Print("║  Status: ▶️ Playing                                           ║");
        ChatGui.Print("║  Mode: Play Once + Auto-Close                                ║");
        ChatGui.Print("║                                                              ║");
        ChatGui.Print("║  💡 Tip: Video plays in background (no VLC UI)               ║");
        ChatGui.Print("╚══════════════════════════════════════════════════════════════╝");
    }

    private void StopVideo()
    {
        try
        {
            videoService.StopVideo();
            ChatGui.Print("[EDR] Video stopped");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[EDR] Stop command failed");
            ChatGui.Print("[EDR] Failed to stop video - check /xllog");
        }
    }

    private void ForceStopVideo()
    {
        try
        {
            // Force kill all VLC processes
            foreach (var process in Process.GetProcessesByName("vlc"))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(1000);
                    ChatGui.Print("[EDR] Force killed VLC process");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[EDR] Failed to kill VLC process");
                }
            }
            
            // Also try normal stop
            videoService.StopVideo();
            ChatGui.Print("[EDR] Force stop completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[EDR] Force stop command failed");
            ChatGui.Print("[EDR] Force stop failed - check /xllog");
        }
    }

    private void SetVideoCommand()
    {
        ChatGui.Print("[EDR] Usage: /edr setvideo <full_path_to_video_file>");
        ChatGui.Print("[EDR] Example: /edr setvideo C:\\Videos\\test.mp4");
        ChatGui.Print($"[EDR] Current video: {configuration.VideoPath}");
    }

    private void SetVideoPath(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                ChatGui.Print("[EDR] Please provide a video file path");
                return;
            }

            if (!File.Exists(path))
            {
                ChatGui.Print($"[EDR] File not found: {path}");
                return;
            }

            var extension = Path.GetExtension(path).ToLowerInvariant();
            if (extension != ".mp4" && extension != ".avi" && extension != ".mkv" && extension != ".mov")
            {
                ChatGui.Print($"[EDR] Unsupported file type: {extension}");
                ChatGui.Print("[EDR] Supported: .mp4, .avi, .mkv, .mov");
                return;
            }

            configuration.VideoPath = path;
            configuration.Save();
            ChatGui.Print($"[EDR] Video set to: {path}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[EDR] SetVideoPath failed");
            ChatGui.Print("[EDR] Failed to set video path - check /xllog");
        }
    }

    private void ShowStatus()
    {
        try
        {
            ChatGui.Print("[EDR] === Plugin Status ===");
            ChatGui.Print($"[EDR] VLC Available: {videoService.IsVLCAvailable()}");
            ChatGui.Print($"[EDR] FFmpeg Available: {videoService.IsFFmpegAvailable()}");
            ChatGui.Print($"[EDR] Currently Playing: {videoService.IsPlaying}");
            ChatGui.Print($"[EDR] Current Video: {videoService.CurrentVideo}");
            
            var videoInfo = videoService.GetVideoInfo(configuration.VideoPath);
            if (videoInfo != null)
            {
                ChatGui.Print($"[EDR] Video Info: {videoInfo.FileSize / 1024 / 1024}MB, {videoInfo.Duration}");
            }
            else
            {
                ChatGui.Print("[EDR] Video Info: Unable to analyze video file");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[EDR] Status command failed");
            ChatGui.Print("[EDR] Failed to get status - check /xllog");
        }
    }

    private void DrawUI()
    {
        // DISABLED: ImGui crashes on this system
        // All functionality available via chat commands
        // if (isVideoOverlayVisible)
        // {
        //     DrawVideoOverlay();
        // }
    }

    private bool isVideoOverlayVisible = false;

    private void DrawVideoOverlay()
    {
        // Simple transparent overlay window
        ImGui.SetNextWindowBgAlpha(0.3f); // Semi-transparent
        ImGui.SetNextWindowPos(new Vector2(100, 100), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(640, 480), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Video Overlay", ref isVideoOverlayVisible, 
            ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize))
        {
            ImGui.Text($"🎬 Video: {Path.GetFileName(configuration.VideoPath)}");
            ImGui.Text($"Status: {(videoService.IsPlaying ? "Playing" : "Stopped")}");
            
            ImGui.Spacing();
            
            if (ImGui.Button("Play"))
            {
                _ = videoService.PlayVideo(configuration.VideoPath);
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Stop"))
            {
                videoService.StopVideo();
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Close"))
            {
                isVideoOverlayVisible = false;
            }
            
            ImGui.Spacing();
            ImGui.Text("💡 Tip: VLC opens in separate window");
            ImGui.Text("Position this overlay over your game area");
        }
        ImGui.End();
    }

    private void OpenConfigUi()
    {
        // Disabled to prevent ImGui crashes
        ChatGui.Print("[EDR] Config window disabled - use chat commands instead");
        ChatGui.Print("[EDR] Available: status, play, stop, test, bench");
    }

    public IChatGui GetChatGui() => ChatGui;
}
