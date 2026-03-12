using System;
using System.Linq;
using System.Reflection;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace ExperimentalDumpsterResearch.Services;

/// <summary>
/// Punish-style reflection service to test accessing Saucy Mini Cactpot settings
/// Uses ECommons pattern like Punish plugins
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
    /// Test accessing Saucy's EnableAutoMiniCactpot setting using Punish-style access
    /// </summary>
    public bool TestSaucyMiniCactpotAccess()
    {
        try
        {
            // Version check to ensure we're running the latest code
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            log.Information("[SaucyReflection] Starting Punish-style test - Version: {version}", version);
            
            log.Information("[SaucyReflection] Testing Punish-style access to Saucy plugin...");

            // Try Punish-style plugin access through ECommons Svc pattern
            var saucyPlugin = GetSaucyPluginPunishStyle();
            if (saucyPlugin == null)
            {
                log.Warning("[SaucyReflection] Saucy plugin not found via Punish-style access");
                return false;
            }

            log.Information("[SaucyReflection] Found Saucy plugin: {saucyType}", saucyPlugin.GetType().Name);

            // Try to get configuration using Punish patterns
            var config = GetSaucyConfigurationPunishStyle(saucyPlugin);
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
                if (SaveSaucyConfigurationPunishStyle(saucyPlugin))
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

    private object? GetSaucyPluginPunishStyle()
    {
        try
        {
            log.Information("[SaucyReflection] Trying Punish-style plugin access...");
            
            // Method 1: Try ECommons Svc pattern (if available)
            try
            {
                // Check if ECommons is available and has Svc
                var ecommonsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ECommons");
                
                if (ecommonsAssembly != null)
                {
                    log.Information("[SaucyReflection] ECommons assembly found");
                    
                    // Try to get Svc.PluginInterface
                    var svcType = ecommonsAssembly.GetType("ECommons.Svc");
                    if (svcType != null)
                    {
                        var pluginInterfaceProperty = svcType.GetProperty("PluginInterface");
                        if (pluginInterfaceProperty != null)
                        {
                            var svcPluginInterface = pluginInterfaceProperty.GetValue(null);
                            if (svcPluginInterface != null)
                            {
                                log.Information("[SaucyReflection] Found ECommons Svc.PluginInterface");
                                
                                // Try to get plugins through ECommons if it has that capability
                                return GetPluginThroughECommons(svcPluginInterface);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Information("[SaucyReflection] ECommons access failed: {msg}", ex.Message);
            }

            // Method 2: Try direct reflection on our PluginInterface (fallback)
            log.Information("[SaucyReflection] Falling back to direct reflection...");
            return GetSaucyPluginDirectReflection();
        }
        catch (Exception ex)
        {
            log.Error(ex, "[SaucyReflection] Error in Punish-style plugin access");
        }
        return null;
    }

    private object? GetPluginThroughECommons(object svcPluginInterface)
    {
        try
        {
            // Check if ECommons has plugin utilities
            var ecommonsAssembly = svcPluginInterface.GetType().Assembly;
            
            // Look for any plugin-related methods in ECommons
            var methods = ecommonsAssembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Where(m => m.Name.Contains("Plugin") && m.GetParameters().Length == 0)
                .ToList();

            log.Information("[SaucyReflection] Found {count} potential plugin methods in ECommons", methods.Count);
            
            foreach (var method in methods.Take(3))
            {
                try
                {
                    log.Information("[SaucyReflection] Trying method: {method}", method.Name);
                    var result = method.Invoke(null, null);
                    if (result != null)
                    {
                        log.Information("[SaucyReflection] Method {method} returned: {type}", method.Name, result.GetType().Name);
                    }
                }
                catch (Exception ex)
                {
                    log.Information("[SaucyReflection] Method {method} failed: {msg}", method.Name, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            log.Information("[SaucyReflection] ECommons plugin search failed: {msg}", ex.Message);
        }
        
        return null;
    }

    private object? GetSaucyPluginDirectReflection()
    {
        try
        {
            // Use the reflection method we had before
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
                        log.Information("[SaucyReflection] Checking plugin: {name}", name ?? "null");
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
            log.Information("[SaucyReflection] Direct reflection failed: {msg}", ex.Message);
        }
        return null;
    }

    private object? GetSaucyConfigurationPunishStyle(object saucyPlugin)
    {
        try
        {
            // Try common Punish plugin configuration patterns
            // Pattern 1: Static C property (like Saucy.C)
            var saucyType = saucyPlugin.GetType();
            var configProperty = saucyType.GetProperty("C", BindingFlags.Static | BindingFlags.Public);
            if (configProperty != null)
            {
                var config = configProperty.GetValue(null);
                if (config != null)
                {
                    log.Information("[SaucyReflection] Found configuration via static C property");
                    return config;
                }
            }

            // Pattern 2: Configuration property
            var configProp = saucyPlugin.GetType().GetProperty("Configuration", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (configProp != null)
            {
                return configProp.GetValue(saucyPlugin);
            }

            // Pattern 3: Config property
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

    private bool SaveSaucyConfigurationPunishStyle(object saucyPlugin)
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
            var config = GetSaucyConfigurationPunishStyle(saucyPlugin);
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

    private void LogConfigurationProperties(object config)
    {
        try
        {
            log.Information("[SaucyReflection] Configuration properties:");
            var properties = config.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in properties.Take(10))
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
}
