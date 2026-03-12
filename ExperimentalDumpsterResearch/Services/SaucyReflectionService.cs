using System;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ExperimentalDumpsterResearch.Services;

/// <summary>
/// Simple reflection service to test accessing Saucy Mini Cactpot settings
/// </summary>
public class SaucyReflectionService
{
    private readonly IPluginLog log;
    private readonly IDalamudPluginInterface pluginInterface;

    public SaucyReflectionService(IPluginLog log, IDalamudPluginInterface pluginInterface)
    {
        this.log = log;
        this.pluginInterface = pluginInterface;
    }

    /// <summary>
    /// Test accessing Saucy's EnableAutoMiniCactpot setting
    /// </summary>
    public bool TestSaucyMiniCactpotAccess()
    {
        try
        {
            log.Information("[SaucyReflection] Testing access to Saucy plugin...");

            // Try to get Saucy plugin through reflection
            var saucyPlugin = GetSaucyPlugin();
            if (saucyPlugin == null)
            {
                log.Warning("[SaucyReflection] Saucy plugin not found");
                return false;
            }

            log.Information("[SaucyReflection] Found Saucy plugin: {saucyType}", saucyPlugin.GetType().Name);

            // Try to get configuration
            var config = GetSaucyConfiguration(saucyPlugin);
            if (config == null)
            {
                log.Warning("[SaucyReflection] Could not access Saucy configuration");
                return false;
            }

            log.Information("[SaucyReflection] Found Saucy configuration: {configType}", config.GetType().Name);

            // Try to get EnableAutoMiniCactpot property
            var currentValue = GetEnableAutoMiniCactpot(config);
            if (currentValue == null)
            {
                log.Warning("[SaucyReflection] Could not read EnableAutoMiniCactpot setting");
                return false;
            }

            log.Information("[SaucyReflection] Current EnableAutoMiniCactpot value: {value}", currentValue.Value);

            // Try to toggle the setting
            var newValue = !currentValue.Value;
            if (SetEnableAutoMiniCactpot(config, newValue))
            {
                log.Information("[SaucyReflection] Successfully set EnableAutoMiniCactpot to: {value}", newValue);

                // Try to save the configuration
                if (SaveSaucyConfiguration(saucyPlugin))
                {
                    log.Information("[SaucyReflection] Successfully saved Saucy configuration");
                    return true;
                }
                else
                {
                    log.Warning("[SaucyReflection] Could not save Saucy configuration");
                }
            }
            else
            {
                log.Warning("[SaucyReflection] Could not set EnableAutoMiniCactpot");
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error testing Saucy Mini Cactpot access");
        }

        return false;
    }

    private object? GetSaucyPlugin()
    {
        try
        {
            // Try to get the plugin through reflection on the plugin interface
            var pluginManagerProperty = pluginInterface.GetType().GetProperty("PluginManager");
            if (pluginManagerProperty?.GetValue(pluginInterface) is object pluginManager)
            {
                var pluginsProperty = pluginManager.GetType().GetProperty("InstalledPlugins");
                if (pluginsProperty?.GetValue(pluginManager) is System.Collections.IEnumerable plugins)
                {
                    foreach (var plugin in plugins)
                    {
                        var nameProp = plugin.GetType().GetProperty("InternalName");
                        if (nameProp?.GetValue(plugin)?.ToString().Equals("Saucy", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            var instanceProp = plugin.GetType().GetProperty("Instance");
                            return instanceProp?.GetValue(plugin);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error getting Saucy plugin");
        }
        return null;
    }

    private object? GetSaucyConfiguration(object saucyPlugin)
    {
        try
        {
            // Try to get Configuration property
            var configProp = saucyPlugin.GetType().GetProperty("Configuration", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (configProp != null)
            {
                return configProp.GetValue(saucyPlugin);
            }

            // Try direct field access
            var configField = saucyPlugin.GetType().GetField("Configuration", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (configField != null)
            {
                return configField.GetValue(saucyPlugin);
            }

            // Try Config property (common pattern)
            var configProp2 = saucyPlugin.GetType().GetProperty("Config", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (configProp2 != null)
            {
                return configProp2.GetValue(saucyPlugin);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error getting Saucy configuration");
        }
        return null;
    }

    private bool? GetEnableAutoMiniCactpot(object config)
    {
        try
        {
            var prop = config.GetType().GetProperty("EnableAutoMiniCactpot", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                var value = prop.GetValue(config);
                return value != null ? (bool)value : null;
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error reading EnableAutoMiniCactpot");
        }
        return null;
    }

    private bool SetEnableAutoMiniCactpot(object config, bool value)
    {
        try
        {
            var prop = config.GetType().GetProperty("EnableAutoMiniCactpot", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
            {
                prop.SetValue(config, value);
                return true;
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error setting EnableAutoMiniCactpot");
        }
        return false;
    }

    private bool SaveSaucyConfiguration(object saucyPlugin)
    {
        try
        {
            // Try SaveConfig method
            var saveMethod = saucyPlugin.GetType().GetMethod("SaveConfig", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (saveMethod != null)
            {
                saveMethod.Invoke(saucyPlugin, null);
                return true;
            }

            // Try calling Save() method on config directly
            var config = GetSaucyConfiguration(saucyPlugin);
            if (config != null)
            {
                var saveMethod2 = config.GetType().GetMethod("Save", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (saveMethod2 != null)
                {
                    saveMethod2.Invoke(config, null);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error saving Saucy configuration");
        }
        return false;
    }
}
