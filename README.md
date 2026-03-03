# Experimental Dumpster Research

> Here at The Dumpster Fire we are researching new ways to cook garbage, perhaps one day we will have a smartpster.

## Overview

Experimental Dumpster Research (EDR) is a test bench plugin for experimenting with new and experimental features for FFXIV Dalamud plugins. The primary focus is on video playback capabilities, but the plugin is designed to be extensible for various research projects.

## Current Research Projects

### 🎬 Video Playback
**Goal**: Enable MP4 video playback within Dalamud plugins

**Status**: Active Research

**Features**:
- MP4 video file playback using VLC media player
- FFmpeg integration for video metadata analysis
- Configurable video window
- Audio support (optional)
- Performance benchmarking
- Test suite for validation

## Installation

1. Build the plugin using Visual Studio or `dotnet build`
2. Copy the built plugin to your Dalamud plugins folder
3. Install VLC media player (required for video playback)
4. Restart FFXIV with Dalamud

## VLC Requirements

The video playback feature requires VLC media player to be installed:

### Windows Installation Options:

1. **Download from VLC website**:
   - Visit https://www.videolan.org/vlc/
   - Download and install VLC media player
   - Default installation location is usually detected automatically

2. **Via Package Manager**:
   ```bash
   # Using Chocolatey
   choco install vlc
   
   # Using Scoop
   scoop install vlc
   ```

3. **Automatic Detection**:
   The plugin will search for VLC in:
   - `%ProgramFiles%\VideoLAN\VLC\vlc.exe`
   - `%ProgramFiles(x86)%\VideoLAN\VLC\vlc.exe`
   - `%LocalAppData%\Programs\VLC\vlc.exe`
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

1. **Setup VLC**: Ensure VLC media player is installed and accessible
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
- Handles video file playback using VLC media player
- Manages video window creation and control
- Provides video file information and validation via FFmpeg

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

1. **VLC Availability**: Verify VLC installation
2. **FFmpeg Availability**: Check FFmpeg for metadata analysis
3. **Video File Validation**: Check video file format and accessibility
4. **Video Playback**: Test video startup and playback
5. **Video Controls**: Test stop/pause functionality
6. **Performance Benchmark**: Measure startup times and performance

### Manual Testing

1. Load various video formats and resolutions
2. Test audio playback and volume controls
3. Verify window positioning and sizing
4. Test performance with different video files
5. Validate error handling for corrupted files

## Troubleshooting

### Common Issues

**VLC Not Found**
- Install VLC media player and ensure it's accessible
- Verify `vlc.exe` is accessible
- Check plugin logs for VLC detection errors

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
- VLC command execution
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
