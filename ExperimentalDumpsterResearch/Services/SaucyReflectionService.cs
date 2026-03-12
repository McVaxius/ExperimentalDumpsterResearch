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
            // Version check to ensure we're running the latest code
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            log.Information("[SaucyReflection] Starting test - Version: {version}", version);
            
            log.Information("[SaucyReflection] Testing access to Saucy plugin...");

            // Try to get Saucy plugin through reflection
            var saucyPlugin = GetSaucyPlugin();
            if (saucyPlugin == null)
            {
                log.Warning("[SaucyReflection] Saucy plugin not found - trying alternative methods");
                
                // Try alternative method - check OtherPlugins directly
                saucyPlugin = GetSaucyPluginAlternative();
                if (saucyPlugin == null)
                {
                    log.Warning("[SaucyReflection] Saucy plugin not found with any method");
                    LogAvailablePlugins();
                    return false;
                }
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
                LogConfigurationProperties(config);
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
            log.Information("[SaucyReflection] Attempting to find Saucy plugin...");
            
            // Method 1: Try reflection on PluginManager (most reliable)
            try
            {
                var pluginManagerProperty = pluginInterface.GetType().GetProperty("PluginManager");
                if (pluginManagerProperty?.GetValue(pluginInterface) is object pluginManager)
                {
                    log.Information("[SaucyReflection] Found PluginManager via reflection");
                    var pluginsProperty = pluginManager.GetType().GetProperty("InstalledPlugins");
                    if (pluginsProperty?.GetValue(pluginManager) is System.Collections.IEnumerable plugins)
                    {
                        foreach (var plugin in plugins)
                        {
                            var nameProp = plugin.GetType().GetProperty("InternalName");
                            var name = nameProp?.GetValue(plugin)?.ToString();
                            log.Information("[SaucyReflection] Checking plugin via reflection: {name}", name ?? "null");
                            if (name?.Equals("Saucy", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                var instanceProp = plugin.GetType().GetProperty("Instance");
                                log.Information("[SaucyReflection] Found Saucy via reflection");
                                return instanceProp?.GetValue(plugin);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Information("[SaucyReflection] PluginManager reflection failed: {msg}", ex.Message);
            }

            // Method 2: Try OtherPlugins (some Dalamud versions)
            try
            {
                var otherPluginsProp = pluginInterface.GetType().GetProperty("OtherPlugins");
                if (otherPluginsProp?.GetValue(pluginInterface) is System.Collections.IEnumerable plugins)
                {
                    log.Information("[SaucyReflection] Found OtherPlugins via reflection");
                    foreach (var plugin in plugins)
                    {
                        var nameProp = plugin.GetType().GetProperty("InternalName");
                        var name = nameProp?.GetValue(plugin)?.ToString();
                        log.Information("[SaucyReflection] Checking plugin via OtherPlugins: {name}", name ?? "null");
                        if (name?.Equals("Saucy", StringComparison.OrdinalIgnoreCase) == true)
                        {
                            var instanceProp = plugin.GetType().GetProperty("Instance");
                            log.Information("[SaucyReflection] Found Saucy via OtherPlugins");
                            return instanceProp?.GetValue(plugin);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Information("[SaucyReflection] OtherPlugins reflection failed: {msg}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error getting Saucy plugin");
        }
        return null;
    }

    private object? GetSaucyPluginAlternative()
    {
        try
        {
            // Try to access OtherPlugins property directly
            var otherPluginsProp = pluginInterface.GetType().GetProperty("OtherPlugins");
            if (otherPluginsProp?.GetValue(pluginInterface) is System.Collections.IEnumerable plugins)
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
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error with alternative Saucy plugin access");
        }
        return null;
    }

    private void LogAvailablePlugins()
    {
        try
        {
            log.Information("[SaucyReflection] Searching for available plugins...");
            
            // Try PluginManager method
            var pluginManagerProperty = pluginInterface.GetType().GetProperty("PluginManager");
            if (pluginManagerProperty?.GetValue(pluginInterface) is object pluginManager)
            {
                var pluginsProperty = pluginManager.GetType().GetProperty("InstalledPlugins");
                if (pluginsProperty?.GetValue(pluginManager) is System.Collections.IEnumerable plugins)
                {
                    var pluginList = new List<string>();
                    foreach (var plugin in plugins)
                    {
                        var nameProp = plugin.GetType().GetProperty("InternalName");
                        var name = nameProp?.GetValue(plugin)?.ToString() ?? "Unknown";
                        pluginList.Add(name);
                    }
                    log.Information("[SaucyReflection] Found {count} plugins via PluginManager: {plugins}", pluginList.Count, string.Join(", ", pluginList));
                }
            }
            
            // Try OtherPlugins method
            var otherPluginsProp = pluginInterface.GetType().GetProperty("OtherPlugins");
            if (otherPluginsProp?.GetValue(pluginInterface) is System.Collections.IEnumerable otherPlugins)
            {
                var pluginList = new List<string>();
                foreach (var plugin in otherPlugins)
                {
                    var nameProp = plugin.GetType().GetProperty("InternalName");
                    var name = nameProp?.GetValue(plugin)?.ToString() ?? "Unknown";
                    pluginList.Add(name);
                }
                log.Information("[SaucyReflection] Found {count} plugins via OtherPlugins: {plugins}", pluginList.Count, string.Join(", ", pluginList));
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error logging available plugins");
        }
    }

    private void LogConfigurationProperties(object config)
    {
        try
        {
            log.Information("[SaucyReflection] Configuration properties:");
            var properties = config.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in properties.Take(10)) // Limit to first 10 to avoid spam
            {
                try
                {
                    var value = prop.GetValue(config);
                    var valueStr = value?.ToString() ?? "null";
                    if (valueStr.Length > 50) valueStr = valueStr.Substring(0, 47) + "...";
                    log.Information("[SaucyReflection] - {propName}: {propType} = {valueStr}", prop.Name, prop.PropertyType.Name, valueStr);
                }
                catch
                {
                    log.Information("[SaucyReflection] - {propName}: {propType} = [error]", prop.Name, prop.PropertyType.Name);
                }
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error logging configuration properties");
        }
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
