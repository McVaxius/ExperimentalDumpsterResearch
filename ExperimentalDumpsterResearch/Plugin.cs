using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.IoC;
using System.Collections.Generic;
using ExperimentalDumpsterResearch.Services;
using ExperimentalDumpsterResearch.Windows;

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
                    configWindow.IsOpen = true;
                    break;
                case "video":
                    mainWindow.IsOpen = true;
                    break;
                case "test":
                    _ = testBenchService.RunCurrentTest();
                    break;
                case "play":
                    _ = testBenchService.StartVideoTest();
                    break;
                case "stop":
                    videoService.StopVideo();
                    break;
                case "bench":
                    _ = testBenchService.RunPerformanceBenchmark();
                    break;
                default:
                    ChatGui.Print($"[EDR] Unknown command: {args}");
                    ChatGui.Print("[EDR] Available: config, video, test, play, stop, bench");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"[EDR] Command error: {command} {args}");
            ChatGui.Print("[EDR] Command failed - check /xllog for details");
        }
    }

    private void DrawUI()
    {
        windowSystem.Draw();
    }

    private void OpenConfigUi()
    {
        configWindow.IsOpen = true;
    }

    public IChatGui GetChatGui() => ChatGui;
}
