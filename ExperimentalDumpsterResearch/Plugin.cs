using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using ExperimentalDumpsterResearch.Services;
using ExperimentalDumpsterResearch.Windows;

namespace ExperimentalDumpsterResearch;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Experimental Dumpster Research";
    
    private readonly WindowSystem windowSystem = new("ExperimentalDumpsterResearch");
    private Configuration configuration;
    private MainWindow mainWindow;
    private ConfigWindow configWindow;

    // Experimental services
    private VideoPlaybackService videoService;
    private TestBenchService testBenchService;

    // Dalamud services following PlogonRules pattern
    private readonly ICommandManager commandManager;
    private readonly IChatGui chatGui;
    private readonly IClientState clientState;
    private readonly IFramework framework;
    private readonly IPluginLog pluginLog;

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        // Initialize services following PlogonRules Section 2.1
        var serviceManager = pluginInterface.GetServiceProvider();
        commandManager = serviceManager.GetRequiredService<ICommandManager>();
        chatGui = serviceManager.GetRequiredService<IChatGui>();
        clientState = serviceManager.GetRequiredService<IClientState>();
        framework = serviceManager.GetRequiredService<IFramework>();
        pluginLog = serviceManager.GetRequiredService<IPluginLog>();

        configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        configuration.Initialize(pluginInterface);

        // Initialize experimental services
        videoService = new VideoPlaybackService(configuration, pluginLog, chatGui);
        testBenchService = new TestBenchService(configuration, videoService, pluginLog, chatGui);

        mainWindow = new MainWindow(this, configuration, videoService, testBenchService);
        configWindow = new ConfigWindow(this, configuration);

        windowSystem.AddWindow(mainWindow);
        windowSystem.AddWindow(configWindow);

        // Command registration following PlogonRules Section 5
        commandManager.AddHandler("/dumpster", new CommandInfo(OnCommand)
        {
            HelpMessage = "Experimental Dumpster Research commands"
        });

        commandManager.AddHandler("/edr", new CommandInfo(OnCommand)
        {
            HelpMessage = "Experimental Dumpster Research commands (short)"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

        pluginLog.Information("[EDR] Plugin initialized successfully");
    }

    public void Dispose()
    {
        windowSystem.RemoveAllWindows();
        mainWindow?.Dispose();
        configWindow?.Dispose();
        videoService?.Dispose();
        testBenchService?.Dispose();

        commandManager.RemoveHandler("/dumpster");
        commandManager.RemoveHandler("/edr");

        pluginLog.Information("[EDR] Plugin disposed");
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
                    chatGui.Print($"[EDR] Unknown command: {args}");
                    chatGui.Print("[EDR] Available: config, video, test, play, stop, bench");
                    break;
            }
        }
        catch (Exception ex)
        {
            pluginLog.Error(ex, $"[EDR] Command error: {command} {args}");
            chatGui.Print("[EDR] Command failed - check /xllog for details");
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

    public IChatGui GetChatGui() => chatGui;
}
