# Experimental Dumpster Research

> Here at The Dumpster Fire we are researching new ways to cook garbage, perhaps one day we will have a smartpster.

## Overview

Experimental Dumpster Research (EDR) is a test bench plugin for experimenting with new and experimental features for FFXIV Dalamud plugins. The primary focus is on video playback capabilities, but the plugin is designed to be extensible for various research projects.

## Current Research Projects

### 🎬 Video Playback
**Goal**: Enable MP4 video playback within Dalamud plugins

**Status**: Active Research

**Features**:
- MP4 video file playback
- FFmpeg integration for video decoding
- Configurable video window
- Audio support (optional)
- Performance benchmarking
- Test suite for validation

## Installation

1. Build the plugin using Visual Studio or `dotnet build`
2. Copy the built plugin to your Dalamud plugins folder
3. Install FFmpeg (required for video playback)
4. Restart FFXIV with Dalamud

## FFmpeg Requirements

The video playback feature requires FFmpeg to be installed and accessible:

### Windows Installation Options:

1. **Download from FFmpeg.org**:
   - Visit https://ffmpeg.org/download.html
   - Download the Windows build
   - Extract to a location (e.g., `C:\FFmpeg\bin`)
   - Add to PATH or place `ffmpeg.exe` in plugin directory

2. **Via Package Manager**:
   ```bash
   # Using Chocolatey
   choco install ffmpeg
   
   # Using Scoop
   scoop install ffmpeg
   ```

3. **Automatic Detection**:
   The plugin will search for FFmpeg in:
   - `%ProgramFiles%\FFmpeg\bin\ffmpeg.exe`
   - `%LocalAppData%\FFmpeg\bin\ffmpeg.exe`
   - System PATH

## Usage

### Commands

- `/edr` or `/dumpster` - Main command interface
- `/edr config` - Open configuration window
- `/edr video` - Open main video window
- `/edr test` - Run current test suite
- `/edr play` - Start video playback
- `/edr stop` - Stop video playback
- `/edr bench` - Run performance benchmark

### Quick Start

1. **Setup FFmpeg**: Ensure FFmpeg is installed and accessible
2. **Configure Video**: Use `/edr config` to set up video file paths
3. **Run Tests**: Use `/edr test` to verify video playback functionality
4. **Play Video**: Use `/edr play` to start video playback

## Configuration

### Video Settings
- **Video Path**: Path to MP4 file for playback
- **Test Video Path**: Video file used for automated testing
- **Loop Video**: Automatically restart video when finished
- **Auto Play**: Start video on plugin load
- **Volume**: Audio volume (0.0 - 1.0)
- **Mute Audio**: Disable audio playback
- **Playback Speed**: Video playback speed (0.25x - 2.0x)

### Display Settings
- **Window Size**: Video window dimensions
- **Maintain Aspect Ratio**: Preserve video aspect ratio
- **Always On Top**: Keep video window above other windows
- **Window Opacity**: Window transparency (0.1 - 1.0)

### Test Bench Settings
- **Performance Monitoring**: Enable detailed performance tracking
- **Benchmark Iterations**: Number of iterations for performance tests
- **Log Detailed Metrics**: Enable verbose logging

## Architecture

### Services

#### VideoPlaybackService
- Handles video file playback using FFmpeg
- Manages video window creation and control
- Provides video file information and validation

#### TestBenchService
- Automated testing framework
- Performance benchmarking
- Test result collection and reporting

### Windows

#### MainWindow
- Primary user interface
- Real-time status display
- Quick action controls
- Test results viewer

#### ConfigWindow
- Comprehensive configuration interface
- Research project management
- Experimental feature toggles

## Research Projects

The plugin supports multiple concurrent research projects:

### Adding New Projects

1. Create a new service class for the research area
2. Add project configuration in `Configuration.cs`
3. Implement test methods in `TestBenchService`
4. Add UI controls in `MainWindow`
5. Update configuration options in `ConfigWindow`

### Project Structure

Each research project should include:
- Service class for core functionality
- Configuration options
- Test methods
- UI integration
- Documentation

## Testing

### Automated Tests

The plugin includes a comprehensive test suite:

1. **FFmpeg Availability**: Verify FFmpeg installation
2. **Video File Validation**: Check video file format and accessibility
3. **Video Playback**: Test video startup and playback
4. **Video Controls**: Test stop/pause functionality
5. **Performance Benchmark**: Measure startup times and performance

### Manual Testing

1. Load various video formats and resolutions
2. Test audio playback and volume controls
3. Verify window positioning and sizing
4. Test performance with different video files
5. Validate error handling for corrupted files

## Troubleshooting

### Common Issues

**FFmpeg Not Found**
- Install FFmpeg and ensure it's in PATH
- Verify `ffmpeg.exe` is accessible
- Check plugin logs for FFmpeg detection errors

**Video Won't Play**
- Verify video file exists and is accessible
- Check video format compatibility (MP4 recommended)
- Ensure sufficient system resources
- Review plugin debug logs

**Performance Issues**
- Enable GPU acceleration in settings
- Reduce video resolution or window size
- Close other applications using system resources
- Run performance benchmark to identify bottlenecks

### Debug Information

Enable debug mode in configuration to see:
- FFmpeg command execution
- Video processing details
- Performance metrics
- Error messages and stack traces

## Development

### Building

```bash
# Build the plugin
dotnet build

# Build with specific configuration
dotnet build --configuration Release
```

### Dependencies

- .NET 10.0
- Dalamud.NET.Sdk 14.0.2
- FFMpegCore 4.8.0
- System.Drawing.Common 8.0.0
- FFXIVClientStructs (for game integration)

### Contributing

1. Fork the repository
2. Create a feature branch
3. Implement your research project
4. Add comprehensive tests
5. Update documentation
6. Submit a pull request

## License

This project is part of the Dumpster Fire experimental research initiative. Please refer to the repository license for usage terms.

## Support

For issues, questions, or research collaboration:
- Check existing issues and documentation
- Review test results and debug logs
- Provide detailed system information
- Include error messages and reproduction steps

---

**Disclaimer**: This is experimental software. Features may be unstable, incomplete, or change without notice. Use at your own risk.
