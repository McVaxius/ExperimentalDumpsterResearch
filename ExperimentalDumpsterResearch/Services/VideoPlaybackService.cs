using Dalamud.Plugin.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FFMpegCore;
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
        
        // Set up FFMpeg options
        GlobalFFOptions.Configure(new FFOptions { BinaryFolder = GetFFmpegPath() });
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

            // Create video window
            await CreateVideoWindow();

            // Start VLC process for video playback
            await StartVLCPlayback(videoPath);

            isPlaying = true;
            chat.Print("[EDR] Video playback started");
            return true;
        }
        catch (Exception ex)
        {
            log.Error(ex, "[EDR] Failed to start video playback");
            chat.Print("[EDR] Failed to start video playback");
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

            if (videoForm != null)
            {
                videoForm.Close();
                videoForm.Dispose();
                videoForm = null;
                pictureBox = null;
            }

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
        var args = $"--video-x=0 --video-y=0 --video-width={config.VideoWindowWidth} --video-height={config.VideoWindowHeight}";
        
        // Add audio settings
        if (config.MuteAudio)
        {
            args += " --no-audio";
        }
        else
        {
            args += $" --volume={(int)(config.Volume * 100)}";
        }

        // Add loop setting
        if (config.LoopVideo)
        {
            args += " --loop";
        }

        // Add playback speed
        if (config.PlaybackSpeed != 1.0f)
        {
            args += $" --rate={config.PlaybackSpeed}";
        }

        // Add direct3D output for better performance
        if (config.EnableGpuAcceleration)
        {
            args += " --directx-hw-accel";
        }

        args += $" \"{videoPath}\"";
        
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
            var analysis = FFProbe.Analyze(videoPath);
            
            return new VideoInfo
            {
                Duration = analysis.Duration,
                Width = (int)analysis.VideoStreams.FirstOrDefault()?.Width ?? 0,
                Height = (int)analysis.VideoStreams.FirstOrDefault()?.Height ?? 0,
                FrameRate = analysis.VideoStreams.FirstOrDefault()?.FrameRate ?? 0,
                Codec = analysis.VideoStreams.FirstOrDefault()?.CodecName ?? "",
                FileSize = new FileInfo(videoPath).Length
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
