using Dalamud.Plugin.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Timer = System.Windows.Forms.Timer;

namespace ExperimentalDumpsterResearch.Services;

public class VideoPlaybackService : IDisposable
{
    private readonly Configuration config;
    private readonly IPluginLog log;
    private readonly IChatGui chat;

    private Form? videoForm;
    private PictureBox? pictureBox;
    private Timer? playbackTimer;
    private Process? vlcProcess;
    private bool isPlaying = false;
    private string currentVideoPath = "";

    public bool IsPlaying => isPlaying;
    public string CurrentVideo => currentVideoPath;

    public VideoPlaybackService(Configuration config, IPluginLog log, IChatGui chat)
    {
        this.config = config;
        this.log = log;
        this.chat = chat;
    }

    /// <summary>
    /// Play an MP4 video file using VLC media player
    /// </summary>
    public async Task<bool> PlayVideo(string videoPath)
    {
        if (string.IsNullOrEmpty(videoPath) || !File.Exists(videoPath))
        {
            chat.Print("[EDR] Video file not found: " + videoPath);
            return false;
        }

        try
        {
            // Stop any currently playing video
            StopVideo();

            currentVideoPath = videoPath;
            log.Info($"[EDR] Starting video playback: {videoPath}");

            // Start VLC process for video playback (simplified - no custom window)
            await StartVLCPlayback(videoPath);

            isPlaying = true;
            chat.Print("[EDR] Video playback started");
            return true;
        }
        catch (Exception ex)
        {
            log.Error(ex, "[EDR] Failed to start video playback");
            chat.Print("[EDR] Failed to start video playback: " + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Stop video playback
    /// </summary>
    public void StopVideo()
    {
        try
        {
            if (vlcProcess != null && !vlcProcess.HasExited)
            {
                vlcProcess.Kill();
                vlcProcess.Dispose();
                vlcProcess = null;
            }

            if (playbackTimer != null)
            {
                playbackTimer.Stop();
                playbackTimer.Dispose();
                playbackTimer = null;
            }

            // Skip UI cleanup to avoid threading issues
            // if (videoForm != null)
            // {
            //     videoForm.Close();
            //     videoForm.Dispose();
            //     videoForm = null;
            //     pictureBox = null;
            // }

            isPlaying = false;
            currentVideoPath = "";
            chat.Print("[EDR] Video playback stopped");
        }
        catch (Exception ex)
        {
            log.Error(ex, "[EDR] Error stopping video playback");
        }
    }

    /// <summary>
    /// Create the video display window
    /// </summary>
    private async Task CreateVideoWindow()
    {
        await Task.Run(() =>
        {
            videoForm = new Form
            {
                Text = "🗑️ EDR Video Player",
                Width = config.VideoWindowWidth,
                Height = config.VideoWindowHeight,
                TopMost = config.AlwaysOnTop,
                Opacity = config.Opacity,
                BackColor = Color.Black,
                StartPosition = FormStartPosition.CenterScreen
            };

            pictureBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            videoForm.Controls.Add(pictureBox);
            videoForm.FormClosing += (s, e) => StopVideo();

            // Show the window on the main thread
            if (videoForm.InvokeRequired)
            {
                videoForm.Invoke(new Action(() => videoForm.Show()));
            }
            else
            {
                videoForm.Show();
            }

            log.Debug("[EDR] Video window created");
        });
    }

    /// <summary>
    /// Start VLC process for video playback
    /// </summary>
    private async Task StartVLCPlayback(string videoPath)
    {
        await Task.Run(() =>
        {
            try
            {
                var vlcPath = GetVLCPath();
                if (string.IsNullOrEmpty(vlcPath))
                {
                    chat.Print("[EDR] VLC not found. Please install VLC media player.");
                    chat.Print("[EDR] Download from: https://www.videolan.org/vlc/");
                    return;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = vlcPath,
                    Arguments = BuildVLCArguments(videoPath),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                vlcProcess = new Process { StartInfo = startInfo };
                
                vlcProcess.Start();
                
                log.Info("[EDR] VLC process started successfully");
                
                // Start monitoring playback
                StartPlaybackMonitoring();
            }
            catch (Exception ex)
            {
                log.Error(ex, "[EDR] Failed to start VLC process");
            }
        });
    }

    /// <summary>
    /// Build VLC command line arguments
    /// </summary>
    private string BuildVLCArguments(string videoPath)
    {
        // VLC arguments - completely borderless, positioned, play once and close
        var args = "-I dummy --no-video-deco --no-embedded-video --play-and-exit";
        
        // Position window (optional - can be configured)
        args += $" --video-x={config.VideoWindowX} --video-y={config.VideoWindowY}";
        
        // Size window to video dimensions
        args += $" --width={config.VideoWindowWidth} --height={config.VideoWindowHeight}";
        
        // Add audio settings
        if (config.MuteAudio)
        {
            args += " --no-audio";
        }

        // Properly escape special characters in filename
        var escapedPath = videoPath.Replace("[", "\\[").Replace("]", "\\]");
        args += $" \"{escapedPath}\"";
        
        return args;
    }

    /// <summary>
    /// Get VLC executable path
    /// </summary>
    private string GetVLCPath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VideoLAN", "VLC", "vlc.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "VideoLAN", "VLC", "vlc.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "VLC", "vlc.exe"),
            "vlc.exe" // Assume it's in PATH
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
                return path;
        }

        return "";
    }

    /// <summary>
    /// Get FFmpeg executable path
    /// </summary>
    private string GetFFmpegPath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "FFmpeg", "bin"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FFmpeg", "bin"),
            "" // Assume it's in PATH
        };

        foreach (var path in possiblePaths)
        {
            if (string.IsNullOrEmpty(path) || Directory.Exists(path))
                return path;
        }

        return "";
    }

    /// <summary>
    /// Start monitoring playback status
    /// </summary>
    private void StartPlaybackMonitoring()
    {
        playbackTimer = new Timer
        {
            Interval = 1000 // Check every second
        };
        
        playbackTimer.Tick += (s, e) =>
        {
            if (vlcProcess == null || vlcProcess.HasExited)
            {
                isPlaying = false;
                playbackTimer?.Stop();
                log.Debug("[EDR] VLC process ended");
            }
        };
        
        playbackTimer.Start();
    }

    /// <summary>
    /// Check if VLC is available
    /// </summary>
    public bool IsVLCAvailable()
    {
        return !string.IsNullOrEmpty(GetVLCPath());
    }

    /// <summary>
    /// Check if FFmpeg is available
    /// </summary>
    public bool IsFFmpegAvailable()
    {
        return !string.IsNullOrEmpty(GetFFmpegPath());
    }

    /// <summary>
    /// Get video file information using FFmpeg
    /// </summary>
    public VideoInfo? GetVideoInfo(string videoPath)
    {
        if (!File.Exists(videoPath))
            return null;

        try
        {
            var fileInfo = new FileInfo(videoPath);
            
            return new VideoInfo
            {
                Duration = TimeSpan.Zero, // Can't get duration without FFmpeg
                Width = 0, // Can't get dimensions without FFmpeg
                Height = 0,
                FrameRate = 0,
                Codec = Path.GetExtension(videoPath).ToUpperInvariant().TrimStart('.'),
                FileSize = fileInfo.Length
            };
        }
        catch (Exception ex)
        {
            log.Error(ex, "[EDR] Failed to get video info");
            return null;
        }
    }

    public void Dispose()
    {
        StopVideo();
    }
}

public class VideoInfo
{
    public TimeSpan Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double FrameRate { get; set; }
    public string Codec { get; set; } = "";
    public long FileSize { get; set; }
}
