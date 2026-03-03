#!/usr/bin/env python3
"""
Experimental Dumpster Research Release Preparation
Handles plugin packaging with proper zip structure to avoid double zip issues
"""

import os
import shutil
import zipfile
from pathlib import Path

def main():
    print("🗑️ Preparing Experimental Dumpster Research release...")
    
    # Configuration
    PROJECT_NAME = "ExperimentalDumpsterResearch"
    TARGET_RUNTIME = "win-x64"
    RELEASE_DIR = Path("github_release")
    BUILD_DIR = Path(f"{PROJECT_NAME}/bin/x64/Release")
    
    # Clean and create release directory
    if RELEASE_DIR.exists():
        shutil.rmtree(RELEASE_DIR)
    RELEASE_DIR.mkdir(exist_ok=True)
    
    # Collect files for packaging
    files_to_package = []
    
    # Find built DLLs and dependencies
    if BUILD_DIR.exists():
        for item in BUILD_DIR.iterdir():
            if item.is_file() and item.suffix in ['.dll', '.json']:
                files_to_package.append((item, item.name))
            elif item.is_dir() and item.name == "runtimes":
                # Only include win-x64 native libraries
                native_dir = item / TARGET_RUNTIME / "native"
                if native_dir.exists():
                    for native_file in native_dir.iterdir():
                        if native_file.is_file():
                            files_to_package.append((native_file, native_file.name))
                continue  # Skip other platform directories
    
    # Copy README and LICENSE to release directory (not in zip)
    readme_src = Path("README.md")
    if readme_src.exists():
        shutil.copy2(readme_src, RELEASE_DIR / "README.md")
    
    # Create the main zip file
    zip_path = RELEASE_DIR / "latest.zip"
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zip_file:
        for src_file, dest_name in files_to_package:
            print(f"  Adding: {dest_name}")
            zip_file.write(src_file, dest_name)
    
    # Safety verification
    print("\n🔍 Safety verification:")
    
    # Check for source code leaks
    source_extensions = ['.cs', '.csproj', '.sln']
    for src_file, dest_name in files_to_package:
        if any(dest_name.endswith(ext) for ext in source_extensions):
            print(f"  ❌ SOURCE LEAK DETECTED: {dest_name}")
            return False
    
    # Check for debug symbols
    for src_file, dest_name in files_to_package:
        if dest_name.endswith('.pdb'):
            print(f"  ❌ DEBUG SYMBOLS FOUND: {dest_name}")
            return False
    
    # Verify manifest exists
    manifest_found = any(dest_name == f"{PROJECT_NAME}.json" for _, dest_name in files_to_package)
    if not manifest_found:
        print(f"  ❌ MANIFEST NOT FOUND: {PROJECT_NAME}.json")
        return False
    
    # Verify main DLL exists
    main_dll_found = any(dest_name == f"{PROJECT_NAME}.dll" for _, dest_name in files_to_package)
    if not main_dll_found:
        print(f"  ❌ MAIN DLL NOT FOUND: {PROJECT_NAME}.dll")
        return False
    
    print("  ✅ No source code leaks detected")
    print("  ✅ No debug symbols found")
    print("  ✅ Manifest file present")
    print("  ✅ Main DLL present")
    
    # Report package contents
    print(f"\n📦 Package contents ({len(files_to_package)} files):")
    for _, dest_name in sorted(files_to_package):
        file_size = os.path.getsize(Path(BUILD_DIR) / dest_name) if (BUILD_DIR / dest_name).exists() else 0
        print(f"  {dest_name} ({file_size:,} bytes)")
    
    print(f"\n✅ Release prepared successfully!")
    print(f"📁 Release directory: {RELEASE_DIR.absolute()}")
    print(f"📦 Package file: {zip_path.absolute()}")
    print(f"📊 Package size: {zip_path.stat().st_size:,} bytes")
    
    # Instructions for GitHub release
    print(f"\n📋 Next steps for GitHub release:")
    print(f"1. Create a new tag: git tag v0.0.0.1")
    print(f"2. Push tag: git push origin v0.0.0.1")
    print(f"3. Create GitHub Release with tag v0.0.0.1")
    print(f"4. Upload {zip_path.name} as release asset")
    print(f"5. Update pluginmaster.json (manual or XARepoUpdater)")

if __name__ == "__main__":
    main()
