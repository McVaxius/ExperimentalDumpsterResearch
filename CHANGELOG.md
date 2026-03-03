# Changelog

All notable changes to Experimental Dumpster Research will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.0.1] - 2026-03-03

### Added
- Initial plugin structure following PlogonRules best practices
- VLC-based MP4 video playback functionality
- FFmpeg integration for video metadata analysis
- Comprehensive test suite with performance benchmarking
- Configurable video window with positioning and sizing options
- Audio support with volume controls and mute option
- Playback speed control (0.25x - 2.0x)
- Loop video functionality
- GPU acceleration options
- Research project management framework
- Extensible test bench architecture
- Real-time status monitoring
- Debug logging and diagnostics

### Features
- **Video Playback Service**: Core video playback using VLC media player
- **Test Bench Service**: Automated testing and performance analysis
- **Configuration System**: Comprehensive settings management
- **UI Windows**: Main interface and configuration panels
- **Command System**: Slash commands for all major functions

### Dependencies
- Dalamud.NET.Sdk 14.0.2
- FFMpegCore 4.8.0
- System.Drawing.Common 8.0.0
- VLC media player (external dependency)

### Commands
- `/edr` or `/dumpster` - Main command interface
- `/edr config` - Open configuration window
- `/edr video` - Open main video window
- `/edr test` - Run current test suite
- `/edr play` - Start video playback
- `/edr stop` - Stop video playback
- `/edr bench` - Run performance benchmark

### Technical Notes
- Uses VLC for video decoding and playback
- FFmpeg for metadata extraction and analysis
- Windows Forms for video window display
- Follows XA PlogonRules architecture patterns
- Implements proper error handling and logging
- Includes comprehensive test coverage

### Known Issues
- Requires VLC media player to be installed separately
- Video window positioning may need fine-tuning
- Performance varies with video resolution and system specs

### Future Research
- Direct video rendering without external dependencies
- Audio visualization and analysis
- Real-time video effects and filters
- Network streaming capabilities
- Integration with other Dalamud plugins
