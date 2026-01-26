# RTSS Migration Guide: Moving to Store-Compatible Framerate Detection

**Document Version:** 1.0  
**Created:** 2026-01-26  
**Status:** Planning & Implementation Guide

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Available Alternatives](#available-alternatives)
3. [Recommended Solution](#recommended-solution)
4. [Migration Strategy](#migration-strategy)
5. [Phase 1: Research & Planning](#phase-1-research--planning)
6. [Phase 2: Create Abstraction Layer](#phase-2-create-abstraction-layer)
7. [Phase 3: Implement PresentMon Integration](#phase-3-implement-presentmon-integration)
8. [Phase 4: Testing & Validation](#phase-4-testing--validation)
9. [Phase 5: Production Deployment](#phase-5-production-deployment)
10. [Rollback Plan](#rollback-plan)
11. [Appendix](#appendix)

---

## Executive Summary

### Current Situation
- **Dependency:** Rivatuner Statistics Server (RTSS) for framerate detection and overlay
- **Problem:** RTSS uses process injection which violates Microsoft Store policies
- **Impact:** Cannot ship full functionality to Store without finding an alternative

### Goal
Replace RTSS dependency with a Microsoft Store-compatible solution that:
- ✅ Detects framerate across D3D9/D3D10/D3D11/D3D12/Vulkan/OpenGL
- ✅ Provides frame timing statistics (min/max/avg)
- ✅ Works without process injection
- ✅ Maintains existing overlay functionality

### Recommendation
**Primary:** Intel PresentMon (subprocess integration)  
**Fallback:** Windows ETW (Event Tracing for Windows)

---

## Available Alternatives

### Option 1: Intel PresentMon ⭐ (RECOMMENDED)

**Overview:** Official Intel tool using Windows ETW to capture framerate data from graphics applications.

**Pros:**
- ✅ **Microsoft Store Safe** - Uses Windows ETW APIs (no injection)
- ✅ **Broad API Support** - D3D9, D3D10, D3D11, D3D12, Vulkan, OpenGL
- ✅ **Vendor Agnostic** - Works with Intel, NVIDIA, AMD GPUs
- ✅ **Official SDK Available** - Includes C++ SDK with API
- ✅ **Detailed Metrics** - Frame time, CPU/GPU time, latency, dropped frames
- ✅ **Active Development** - Regularly updated by Intel
- ✅ **Open Source** - MIT License

**Cons:**
- ⚠️ Requires C++/CLI wrapper or subprocess integration for C#
- ⚠️ User must be in "Performance Log Users" Windows group (can be added programmatically with admin)
- ⚠️ Additional ~5-10MB binary to bundle

**Store Compatibility:** ✅ **SAFE**

**Resources:**
- GitHub: https://github.com/GameTechDev/PresentMon
- Website: https://www.presentmon.com
- License: MIT

---

### Option 2: Windows ETW (Raw API)

**Overview:** Direct use of Windows Event Tracing for Windows to capture DXGI Present events.

**Pros:**
- ✅ **Microsoft Store Safe** - Native Windows API
- ✅ **No External Dependencies** - Built into Windows
- ✅ **C# Compatible** - Use `Microsoft.Diagnostics.Tracing.TraceEvent` NuGet
- ✅ **Full Control** - Customize what events to capture

**Cons:**
- ⚠️ Complex implementation - Requires understanding of ETW providers
- ⚠️ May require elevated permissions
- ⚠️ More low-level than PresentMon
- ⚠️ Need to manually parse events and calculate metrics

**Store Compatibility:** ✅ **SAFE**

**Resources:**
- NuGet: Microsoft.Diagnostics.Tracing.TraceEvent
- Provider: Microsoft-Windows-DXGI
- Events: IDXGISwapChain::Present

---

### Option 3: GPU Vendor APIs

**Overview:** Use AMD ADLX (already integrated) or NVIDIA APIs.

**Current Status:** You already use AMD ADLX for GPU metrics.

**Pros:**
- ✅ Vendor-optimized
- ✅ Already familiar with ADLX pattern
- ✅ Comprehensive GPU metrics

**Cons:**
- ❌ **No Direct FPS Data** - Most vendor APIs don't expose application framerate
- ⚠️ Vendor-specific implementations needed

**Store Compatibility:** ✅ **SAFE**

**Verdict:** Not suitable for framerate detection, but continue using for GPU metrics.

---

### Option 4: DirectX Hooking (EasyHook + SharpDX)

**Overview:** Inject into game process and hook D3D Present() calls.

**Pros:**
- ✅ Accurate frame timing
- ✅ C# compatible

**Cons:**
- ❌ **Microsoft Store INCOMPATIBLE** - Process injection prohibited
- ❌ Anti-cheat issues
- ❌ Security concerns

**Store Compatibility:** ❌ **NOT SAFE**

**Verdict:** NOT RECOMMENDED - Violates Store policies

---

### Option 5: Overlay.NET Library

**Overview:** C# library for creating overlays.

**Pros:**
- ✅ C# native
- ✅ Easy overlay rendering

**Cons:**
- ❌ No built-in FPS detection
- ⚠️ Still needs separate data source

**Store Compatibility:** ⚠️ **DEPENDS ON IMPLEMENTATION**

**Verdict:** Good for overlay rendering, but doesn't solve FPS detection problem.

---

## Recommended Solution

### Primary: PresentMon Subprocess Integration

**Architecture:**
```
┌─────────────────────────────────────────────────────┐
│ XboxGamingBarHelper (.NET 8)                        │
├─────────────────────────────────────────────────────┤
│                                                     │
│  ┌────────────────────────────────────────────┐    │
│  │ IFramerateProvider (Interface)             │    │
│  └────────────────────────────────────────────┘    │
│            ▲                    ▲                   │
│            │                    │                   │
│  ┌─────────┴────────┐  ┌────────┴──────────────┐  │
│  │ PresentMon       │  │ ETWFrameRate          │  │
│  │ Provider         │  │ Provider (Fallback)   │  │
│  │ - Launch process │  │ - Direct ETW API      │  │
│  │ - Parse CSV      │  │ - TraceEvent NuGet    │  │
│  └──────────────────┘  └───────────────────────┘  │
│            │                                        │
│            ▼                                        │
│  ┌────────────────────────────────────────────┐    │
│  │ FramerateManager                           │    │
│  │ - Provider selection logic                 │    │
│  │ - Expose FPS metrics (Properties)          │    │
│  └────────────────────────────────────────────┘    │
│            │                                        │
│            ▼                                        │
│  ┌────────────────────────────────────────────┐    │
│  │ OnScreenDisplayManager                     │    │
│  │ - OSDItemFPS (modified to use new source)  │    │
│  │ - OSDItemFrametimeStats (modified)         │    │
│  └────────────────────────────────────────────┘    │
│                                                     │
└─────────────────────────────────────────────────────┘
                  │
                  ▼
        ┌──────────────────┐
        │ PresentMon.exe   │
        │ (Bundled)        │
        └──────────────────┘
```

---

## Migration Strategy

### Key Principles
1. **Incremental Changes** - Small, testable steps
2. **Backward Compatibility** - Keep RTSS working during development
3. **Safe Rollback** - Each phase can be reverted independently
4. **Conditional Compilation** - Use `#if STORE` to separate implementations

### Timeline Estimate
- **Phase 1:** Research & Planning (2-4 hours)
- **Phase 2:** Abstraction Layer (4-6 hours)
- **Phase 3:** PresentMon Integration (8-12 hours)
- **Phase 4:** Testing & Validation (4-6 hours)
- **Phase 5:** Production Deployment (2-3 hours)

**Total:** ~20-31 hours of development work

---

## Phase 1: Research & Planning

### Objectives
- ✅ Download and test PresentMon manually
- ✅ Verify Store compatibility and licensing
- ✅ Test PresentMon with your test games
- ✅ Document PresentMon command-line options

### Step 1.1: Download PresentMon

**Actions:**
1. Visit https://github.com/GameTechDev/PresentMon/releases
2. Download latest release (e.g., `PresentMon-2.x.x-x64.zip`)
3. Extract to a test folder (e.g., `C:\Temp\PresentMon\`)

**Expected Files:**
- `PresentMon.exe` - Console application
- `PresentMonService.exe` - Background service
- `PresentMonSDK.dll` - C++ SDK library
- Documentation files

### Step 1.2: Manual Testing

**Test 1: Basic FPS Capture**
```powershell
# Open PowerShell
cd C:\Temp\PresentMon

# Capture all running applications for 10 seconds
.\PresentMon.exe -timed 10 -output_stdout

# Expected: Console output showing FPS for all D3D/Vulkan apps
```

**Test 2: Specific Application Capture**
```powershell
# Launch a test game (e.g., any D3D game)
# Then run:
.\PresentMon.exe -process_name <game.exe> -output_stdout -timed 10

# Expected: Real-time FPS data in console
```

**Test 3: CSV Output**
```powershell
# Capture to CSV file
.\PresentMon.exe -process_name <game.exe> -output_file fps_test.csv -timed 10

# Check the CSV file structure
notepad fps_test.csv
```

**📊 Document CSV Format:**
```csv
Application,ProcessID,SwapChainAddress,Runtime,SyncInterval,PresentFlags,PresentMode,
TimeInSeconds,MsBetweenPresents,MsBetweenDisplayChange,MsInPresentAPI,MsUntilRenderComplete,
MsUntilDisplayed,Dropped,QPCTime
```

Key columns for us:
- `MsBetweenPresents` - Frame time (convert to FPS: 1000/value)
- `Dropped` - Dropped frames
- `Application` - Process name

### Step 1.3: Verify License Compatibility

**Actions:**
1. Review LICENSE file in PresentMon release
2. Confirm it's MIT License (allows commercial use and redistribution)
3. Document attribution requirements

**MIT License Requirements:**
- ✅ Can redistribute with your app
- ✅ Can use commercially
- ✅ Must include license notice in documentation
- ✅ No warranty provided

**✅ VERIFIED: Safe for Microsoft Store**

### Step 1.4: Check User Permissions

**Test:**
```powershell
# Check if current user is in Performance Log Users group
whoami /groups | findstr "Performance Log Users"

# If not present, PresentMon will require admin to add user
```

**Handling:**
- Option A: Request admin once to add user to group (persistent)
- Option B: Run PresentMon with admin (per-session)
- Option C: Document as system requirement

### Step 1.5: Performance Impact Testing

**Test:**
```powershell
# Run a benchmark game
# Measure FPS without PresentMon
# Measure FPS with PresentMon running
# Calculate overhead percentage
```

**Expected:** <1% performance impact (PresentMon is optimized)

### ✅ Phase 1 Checklist

- [ ] PresentMon downloaded and extracted
- [ ] Basic FPS capture tested and working
- [ ] CSV output format documented
- [ ] License verified (MIT - Store compatible)
- [ ] Performance impact measured (<1%)
- [ ] User permission requirements documented
- [ ] Test results documented in this file (update section below)

**📝 Test Results:**
```
Date Tested: _______________
PresentMon Version: _______________
Test Game: _______________
FPS Without PresentMon: _______________
FPS With PresentMon: _______________
Performance Overhead: _______________%
Issues Encountered: _______________
```

---

## Phase 2: Create Abstraction Layer

### Objectives
- ✅ Create interface for framerate providers
- ✅ Refactor existing RTSS code to use interface
- ✅ Ensure backward compatibility
- ✅ Prepare for multi-provider support

### Step 2.1: Create IFramerateProvider Interface

**File:** `XboxGamingBarHelper\Framerate\IFramerateProvider.cs`

**Actions:**
```csharp
using System;

namespace XboxGamingBarHelper.Framerate
{
    /// <summary>
    /// Interface for framerate detection providers (RTSS, PresentMon, ETW, etc.)
    /// </summary>
    public interface IFramerateProvider : IDisposable
    {
        /// <summary>
        /// Name of the provider (e.g., "RTSS", "PresentMon")
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Whether the provider is installed and available
        /// </summary>
        bool IsInstalled { get; }

        /// <summary>
        /// Whether the provider is currently running/active
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Current frames per second
        /// Returns -1 if not available
        /// </summary>
        float CurrentFPS { get; }

        /// <summary>
        /// Minimum frame time in milliseconds
        /// Returns -1 if not available
        /// </summary>
        float FrameTimeMin { get; }

        /// <summary>
        /// Maximum frame time in milliseconds
        /// Returns -1 if not available
        /// </summary>
        float FrameTimeMax { get; }

        /// <summary>
        /// Average frame time in milliseconds
        /// Returns -1 if not available
        /// </summary>
        float FrameTimeAvg { get; }

        /// <summary>
        /// Initialize the provider
        /// </summary>
        /// <returns>True if initialization succeeded</returns>
        bool Initialize();

        /// <summary>
        /// Start capturing framerate data
        /// </summary>
        /// <returns>True if start succeeded</returns>
        bool Start();

        /// <summary>
        /// Stop capturing framerate data
        /// </summary>
        void Stop();

        /// <summary>
        /// Update provider state (called every frame/update loop)
        /// </summary>
        void Update();
    }
}
```

**Testing:**
1. Create the file
2. Build solution
3. Verify no compilation errors

**✅ Success Criteria:** Project compiles successfully

### Step 2.2: Create Base FramerateProvider Class

**File:** `XboxGamingBarHelper\Framerate\FramerateProviderBase.cs`

**Actions:**
```csharp
using System;
using NLog;

namespace XboxGamingBarHelper.Framerate
{
    /// <summary>
    /// Base class for framerate providers with common functionality
    /// </summary>
    public abstract class FramerateProviderBase : IFramerateProvider
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        protected float currentFPS = -1f;
        protected float frameTimeMin = -1f;
        protected float frameTimeMax = -1f;
        protected float frameTimeAvg = -1f;

        public abstract string ProviderName { get; }
        public abstract bool IsInstalled { get; }
        public abstract bool IsRunning { get; }

        public float CurrentFPS => currentFPS;
        public float FrameTimeMin => frameTimeMin;
        public float FrameTimeMax => frameTimeMax;
        public float FrameTimeAvg => frameTimeAvg;

        public abstract bool Initialize();
        public abstract bool Start();
        public abstract void Stop();
        public abstract void Update();

        public virtual void Dispose()
        {
            Stop();
        }

        protected void ResetMetrics()
        {
            currentFPS = -1f;
            frameTimeMin = -1f;
            frameTimeMax = -1f;
            frameTimeAvg = -1f;
        }
    }
}
```

**Testing:**
1. Create the file
2. Build solution
3. Verify no compilation errors

**✅ Success Criteria:** Project compiles successfully

### Step 2.3: Create RTSSFramerateProvider

**File:** `XboxGamingBarHelper\Framerate\RTSSFramerateProvider.cs`

**Actions:**
Extract framerate-specific logic from `RTSSManager` into a new provider class:

```csharp
using System;
using System.Linq;
using RTSSSharedMemoryNET;
using Shared.Utilities;

namespace XboxGamingBarHelper.Framerate
{
    /// <summary>
    /// RTSS-based framerate provider
    /// </summary>
    public class RTSSFramerateProvider : FramerateProviderBase
    {
        public override string ProviderName => "RivaTuner Statistics Server";

        public override bool IsInstalled => RTSSHelper.IsInstalled(out _);

        public override bool IsRunning => RTSSHelper.IsRunning();

        public override bool Initialize()
        {
            Logger.Info("Initializing RTSS Framerate Provider");
            return IsInstalled;
        }

        public override bool Start()
        {
            if (!IsInstalled)
            {
                Logger.Warn("Cannot start RTSS provider - not installed");
                return false;
            }

            Logger.Info("Starting RTSS Framerate Provider");
            return true;
        }

        public override void Stop()
        {
            Logger.Info("Stopping RTSS Framerate Provider");
            ResetMetrics();
        }

        public override void Update()
        {
            if (!IsRunning)
            {
                ResetMetrics();
                return;
            }

            try
            {
                var appEntries = OSD.GetAppEntries();
                if (appEntries != null && appEntries.Length > 0)
                {
                    // Find the app that has stats or use the first one with a PID
                    AppEntry activeApp = appEntries.FirstOrDefault(a => a.StatFrameTimeCount > 0)
                                       ?? appEntries.FirstOrDefault(a => a.ProcessId > 0);

                    if (activeApp != null)
                    {
                        // Update frametime stats (in milliseconds)
                        frameTimeMin = activeApp.StatFrameTimeMin / 1000.0f;
                        frameTimeMax = activeApp.StatFrameTimeMax / 1000.0f;
                        frameTimeAvg = activeApp.StatFrameTimeAvg / 1000.0f;

                        // Calculate FPS from average frame time
                        if (frameTimeAvg > 0)
                        {
                            currentFPS = 1000.0f / frameTimeAvg;
                        }
                        else
                        {
                            currentFPS = -1f;
                        }
                    }
                    else
                    {
                        ResetMetrics();
                    }
                }
                else
                {
                    ResetMetrics();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error updating RTSS framerate data");
                ResetMetrics();
            }
        }
    }
}
```

**Testing:**
1. Create the file
2. Build solution
3. Verify it compiles
4. **DO NOT integrate yet** - this is just preparation

**✅ Success Criteria:** New RTSS provider compiles without errors

### Step 2.4: Create FramerateManager

**File:** `XboxGamingBarHelper\Framerate\FramerateManager.cs`

**Actions:**
```csharp
using System;
using System.Collections.Generic;
using NLog;

namespace XboxGamingBarHelper.Framerate
{
    /// <summary>
    /// Manages framerate providers and exposes unified framerate data
    /// </summary>
    public class FramerateManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly List<IFramerateProvider> availableProviders;
        private IFramerateProvider activeProvider;

        public FramerateManager()
        {
            availableProviders = new List<IFramerateProvider>();
        }

        /// <summary>
        /// Register a framerate provider
        /// </summary>
        public void RegisterProvider(IFramerateProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            availableProviders.Add(provider);
            Logger.Info($"Registered framerate provider: {provider.ProviderName}");
        }

        /// <summary>
        /// Initialize and select the best available provider
        /// </summary>
        public bool Initialize()
        {
            Logger.Info("Initializing FramerateManager...");

            // Try each provider in order of registration
            foreach (var provider in availableProviders)
            {
                Logger.Info($"Attempting to initialize provider: {provider.ProviderName}");
                
                if (provider.Initialize() && provider.IsInstalled)
                {
                    activeProvider = provider;
                    Logger.Info($"Selected active provider: {provider.ProviderName}");
                    return true;
                }
                else
                {
                    Logger.Info($"Provider {provider.ProviderName} not available");
                }
            }

            Logger.Warn("No framerate provider available");
            return false;
        }

        /// <summary>
        /// Start framerate capture
        /// </summary>
        public bool Start()
        {
            if (activeProvider == null)
            {
                Logger.Warn("Cannot start - no active provider");
                return false;
            }

            return activeProvider.Start();
        }

        /// <summary>
        /// Stop framerate capture
        /// </summary>
        public void Stop()
        {
            activeProvider?.Stop();
        }

        /// <summary>
        /// Update framerate data (call every frame)
        /// </summary>
        public void Update()
        {
            activeProvider?.Update();
        }

        // Expose metrics from active provider
        public float CurrentFPS => activeProvider?.CurrentFPS ?? -1f;
        public float FrameTimeMin => activeProvider?.FrameTimeMin ?? -1f;
        public float FrameTimeMax => activeProvider?.FrameTimeMax ?? -1f;
        public float FrameTimeAvg => activeProvider?.FrameTimeAvg ?? -1f;
        public string ActiveProviderName => activeProvider?.ProviderName ?? "None";
        public bool IsRunning => activeProvider?.IsRunning ?? false;

        public void Dispose()
        {
            foreach (var provider in availableProviders)
            {
                provider?.Dispose();
            }
            availableProviders.Clear();
            activeProvider = null;
        }
    }
}
```

**Testing:**
1. Create the file
2. Build solution
3. Verify compilation

**✅ Success Criteria:** Solution compiles successfully

### Step 2.5: Update Project File

**File:** `XboxGamingBarHelper\XboxGamingBarHelper.csproj`

**Actions:**
Add new files to appropriate item group:
```xml
<ItemGroup>
  <Compile Include="Framerate\IFramerateProvider.cs" />
  <Compile Include="Framerate\FramerateProviderBase.cs" />
  <Compile Include="Framerate\RTSSFramerateProvider.cs" />
  <Compile Include="Framerate\FramerateManager.cs" />
</ItemGroup>
```

**Testing:**
1. Build solution
2. Verify all new files are included in build

### ✅ Phase 2 Checklist

- [ ] `IFramerateProvider.cs` created and compiles
- [ ] `FramerateProviderBase.cs` created and compiles
- [ ] `RTSSFramerateProvider.cs` created and compiles
- [ ] `FramerateManager.cs` created and compiles
- [ ] Project file updated with new files
- [ ] Full solution builds without errors
- [ ] Git commit created: "Add framerate provider abstraction layer"

**🔄 Rollback:** If issues occur, delete the `Framerate\` folder and revert project file changes.

---

## Phase 3: Implement PresentMon Integration

### Objectives
- ✅ Create PresentMon framerate provider
- ✅ Bundle PresentMon binaries
- ✅ Implement subprocess management
- ✅ Parse PresentMon output

### Step 3.1: Add PresentMon Binaries to Project

**Actions:**
1. Create folder: `XboxGamingBarHelper\PresentMon\`
2. Copy from downloaded release:
   - `PresentMon.exe` → `XboxGamingBarHelper\PresentMon\PresentMon.exe`
3. Add LICENSE file: `XboxGamingBarHelper\PresentMon\LICENSE.txt`

**Update Project File:**
```xml
<ItemGroup>
  <None Include="PresentMon\PresentMon.exe">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
  <None Include="PresentMon\LICENSE.txt">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

**Testing:**
1. Build solution
2. Check output folder: `bin\Debug\net8.0-windows10.0.22000.0\PresentMon\`
3. Verify `PresentMon.exe` is present

**✅ Success Criteria:** PresentMon.exe copied to output directory

### Step 3.2: Create PresentMonFramerateProvider

**File:** `XboxGamingBarHelper\Framerate\PresentMonFramerateProvider.cs`

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XboxGamingBarHelper.Framerate
{
    /// <summary>
    /// PresentMon-based framerate provider
    /// </summary>
    public class PresentMonFramerateProvider : FramerateProviderBase
    {
        private Process presentMonProcess;
        private CancellationTokenSource cancellationTokenSource;
        private Task outputReaderTask;
        
        private readonly string presentMonPath;
        private readonly StringBuilder outputBuffer;
        private readonly object lockObject = new object();

        // PresentMon output parsing state
        private int frameCount = 0;
        private float sumFrameTime = 0f;
        private float minFrameTime = float.MaxValue;
        private float maxFrameTime = float.MinValue;
        private DateTime lastResetTime = DateTime.Now;
        private const int RESET_INTERVAL_MS = 1000; // Reset stats every second

        public override string ProviderName => "Intel PresentMon";

        public override bool IsInstalled
        {
            get
            {
                return File.Exists(presentMonPath);
            }
        }

        public override bool IsRunning => presentMonProcess != null && !presentMonProcess.HasExited;

        public PresentMonFramerateProvider()
        {
            // Construct path to bundled PresentMon.exe
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            presentMonPath = Path.Combine(baseDir, "PresentMon", "PresentMon.exe");
            outputBuffer = new StringBuilder();
        }

        public override bool Initialize()
        {
            Logger.Info("Initializing PresentMon Framerate Provider");
            
            if (!IsInstalled)
            {
                Logger.Error($"PresentMon not found at: {presentMonPath}");
                return false;
            }

            Logger.Info($"PresentMon found at: {presentMonPath}");
            return true;
        }

        public override bool Start()
        {
            if (!IsInstalled)
            {
                Logger.Error("Cannot start PresentMon - not installed");
                return false;
            }

            if (IsRunning)
            {
                Logger.Warn("PresentMon already running");
                return true;
            }

            try
            {
                Logger.Info("Starting PresentMon process");

                cancellationTokenSource = new CancellationTokenSource();

                // Configure PresentMon process
                var startInfo = new ProcessStartInfo
                {
                    FileName = presentMonPath,
                    // Arguments: capture all processes, output to stdout, no CSV headers after first line
                    Arguments = "-output_stdout -no_top -dont_restart_as_admin -terminate_on_proc_exit",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                presentMonProcess = new Process { StartInfo = startInfo };

                // Start the process
                presentMonProcess.Start();

                // Start reading output asynchronously
                outputReaderTask = Task.Run(() => ReadOutputAsync(cancellationTokenSource.Token));

                Logger.Info($"PresentMon process started (PID: {presentMonProcess.Id})");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to start PresentMon process");
                return false;
            }
        }

        public override void Stop()
        {
            Logger.Info("Stopping PresentMon provider");

            try
            {
                // Signal cancellation
                cancellationTokenSource?.Cancel();

                // Kill the process if still running
                if (presentMonProcess != null && !presentMonProcess.HasExited)
                {
                    presentMonProcess.Kill();
                    presentMonProcess.WaitForExit(2000);
                }

                // Wait for output reader task
                outputReaderTask?.Wait(1000);

                // Cleanup
                presentMonProcess?.Dispose();
                presentMonProcess = null;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;

                ResetMetrics();
                Logger.Info("PresentMon provider stopped");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error stopping PresentMon provider");
            }
        }

        public override void Update()
        {
            if (!IsRunning)
            {
                ResetMetrics();
                return;
            }

            // Check if we need to reset stats (every second)
            if ((DateTime.Now - lastResetTime).TotalMilliseconds >= RESET_INTERVAL_MS)
            {
                lock (lockObject)
                {
                    // Calculate metrics from accumulated data
                    if (frameCount > 0)
                    {
                        frameTimeAvg = sumFrameTime / frameCount;
                        frameTimeMin = minFrameTime;
                        frameTimeMax = maxFrameTime;
                        currentFPS = frameCount; // Frames in the last second = FPS
                    }
                    else
                    {
                        ResetMetrics();
                    }

                    // Reset accumulation
                    frameCount = 0;
                    sumFrameTime = 0f;
                    minFrameTime = float.MaxValue;
                    maxFrameTime = float.MinValue;
                    lastResetTime = DateTime.Now;
                }
            }
        }

        private async Task ReadOutputAsync(CancellationToken cancellationToken)
        {
            try
            {
                Logger.Info("PresentMon output reader started");

                using (var reader = presentMonProcess.StandardOutput)
                {
                    string line;
                    bool isFirstLine = true;

                    while (!cancellationToken.IsCancellationRequested && 
                           (line = await reader.ReadLineAsync()) != null)
                    {
                        // Skip CSV header
                        if (isFirstLine)
                        {
                            isFirstLine = false;
                            Logger.Debug($"PresentMon CSV Header: {line}");
                            continue;
                        }

                        ParsePresentMonLine(line);
                    }
                }

                Logger.Info("PresentMon output reader finished");
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    Logger.Error(ex, "Error reading PresentMon output");
                }
            }
        }

        private void ParsePresentMonLine(string line)
        {
            try
            {
                // PresentMon CSV format (simplified):
                // Application,ProcessID,SwapChainAddress,Runtime,SyncInterval,PresentFlags,
                // PresentMode,TimeInSeconds,MsBetweenPresents,...
                
                var parts = line.Split(',');
                if (parts.Length < 9)
                    return;

                // Extract MsBetweenPresents (index 8)
                string msTestString = parts[8];
                if (float.TryParse(msTestString, out float msBetweenPresents))
                {
                    if (msBetweenPresents > 0 && msBetweenPresents < 1000) // Sanity check
                    {
                        lock (lockObject)
                        {
                            frameCount++;
                            sumFrameTime += msBetweenPresents;
                            
                            if (msBetweenPresents < minFrameTime)
                                minFrameTime = msBetweenPresents;
                            
                            if (msBetweenPresents > maxFrameTime)
                                maxFrameTime = msBetweenPresents;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"Error parsing PresentMon line: {line}");
            }
        }

        public override void Dispose()
        {
            Stop();
            base.Dispose();
        }
    }
}
```

**Testing:**
1. Create the file
2. Build solution
3. Verify compilation

**✅ Success Criteria:** New provider compiles without errors

### Step 3.3: Update Program.cs to Use FramerateManager

**File:** `XboxGamingBarHelper\Program.cs`

**Actions (for testing only - don't remove RTSS yet):**

Add field:
```csharp
private static FramerateManager framerateManager;
```

In `MainLoopAsync()`, after hardware manager initialization:
```csharp
Logger.Info("Initialize Framerate Manager.");
framerateManager = new FramerateManager();

#if !STORE
// Register RTSS provider for Debug/Release builds
framerateManager.RegisterProvider(new RTSSFramerateProvider());
#endif

// Register PresentMon provider (all builds)
framerateManager.RegisterProvider(new PresentMonFramerateProvider());

// Initialize and start
if (framerateManager.Initialize())
{
    framerateManager.Start();
    Logger.Info($"Framerate manager started with provider: {framerateManager.ActiveProviderName}");
}
else
{
    Logger.Warn("No framerate provider available");
}
```

Add to update loop:
```csharp
framerateManager?.Update();
```

**Testing:**
1. Build in Debug configuration
2. Run XboxGamingBarHelper
3. Check logs for:
   - "Registered framerate provider: RivaTuner Statistics Server"
   - "Registered framerate provider: Intel PresentMon"
   - "Selected active provider: [provider name]"
4. Verify no crashes

**✅ Success Criteria:** 
- Application starts without errors
- Logs show provider registration
- Active provider selected

### Step 3.4: Manual Testing of PresentMon Provider

**Test Plan:**

**Test 1: PresentMon Detection**
1. Build XboxGamingBarHelper
2. Check `bin\Debug\...\PresentMon\PresentMon.exe` exists
3. Run helper app
4. Check logs for "PresentMon found at: [path]"

**Test 2: Process Launch**
1. Run helper app
2. Open Task Manager
3. Verify `PresentMon.exe` process is running
4. Close helper app
5. Verify `PresentMon.exe` process terminated

**Test 3: Framerate Data**
1. Launch a test game (any D3D game)
2. Run helper app
3. Add temporary debug logs in `Update()`:
   ```csharp
   Logger.Debug($"FPS: {framerateManager.CurrentFPS}, Avg Frame Time: {framerateManager.FrameTimeAvg}ms");
   ```
4. Check logs for actual FPS values
5. Compare with in-game FPS counter or RTSS

**Expected Results:**
- FPS values should be reasonable (30-240 range for most games)
- Frame times should correlate with FPS
- Values should update approximately every second

### ✅ Phase 3 Checklist

- [ ] PresentMon binaries added to project
- [ ] PresentMon copied to output directory
- [ ] `PresentMonFramerateProvider.cs` created
- [ ] FramerateManager integrated in Program.cs
- [ ] Solution builds without errors
- [ ] Manual Test 1 passed (detection)
- [ ] Manual Test 2 passed (process launch)
- [ ] Manual Test 3 passed (framerate data)
- [ ] Git commit: "Add PresentMon framerate provider"

**🔄 Rollback:** Revert Program.cs changes, keep provider classes for later.

---

## Phase 4: Testing & Validation

### Objectives
- ✅ Integrate framerate data into OSD items
- ✅ Test with multiple games
- ✅ Validate Store build compatibility
- ✅ Performance testing

### Step 4.1: Update OSDItemFPS

**File:** `XboxGamingBarHelper\RTSS\OSDItems\OSDItemFPS.cs`

**Current Implementation Review:**
Look at your existing OSDItemFPS to understand how it gets FPS data from RTSS.

**Actions:**
Modify to use FramerateManager instead:

```csharp
// Add to class
private readonly FramerateManager framerateManager;

// Update constructor
public OSDItemFPS(FramerateManager framerateManager)
{
    this.framerateManager = framerateManager;
}

// Update GetOSDString method
public override string GetOSDString(int onScreenDisplayLevel)
{
    if (onScreenDisplayLevel < 4)
        return string.Empty;

    float fps = framerateManager?.CurrentFPS ?? -1f;
    
    if (fps < 0)
        return string.Empty;

    return $"<C=00FF00>FPS<C>: {fps:F1}";
}
```

**Testing:**
1. Build solution
2. Run with a game
3. Verify FPS shows in overlay
4. Compare with previous RTSS values

### Step 4.2: Update OSDItemFrametimeStats

**File:** `XboxGamingBarHelper\RTSS\OSDItems\OSDItemFrametimeStats.cs`

**Actions:**
```csharp
// Add to class
private readonly FramerateManager framerateManager;

// Update constructor
public OSDItemFrametimeStats(FramerateManager framerateManager)
{
    this.framerateManager = framerateManager;
}

// Update GetOSDString method
public override string GetOSDString(int onScreenDisplayLevel)
{
    if (onScreenDisplayLevel < 4)
        return string.Empty;

    float min = framerateManager?.FrameTimeMin ?? -1f;
    float max = framerateManager?.FrameTimeMax ?? -1f;
    float avg = framerateManager?.FrameTimeAvg ?? -1f;

    if (min < 0 || max < 0 || avg < 0)
        return string.Empty;

    return $"<C=FFFF00>Frame Time<C>: {min:F1}ms / {avg:F1}ms / {max:F1}ms";
}
```

**Testing:**
1. Build and run
2. Verify frametime stats appear in overlay
3. Validate values are reasonable

### Step 4.3: Update RTSSManager

**File:** `XboxGamingBarHelper\RTSS\RTSSManager.cs`

**Actions:**
Update constructor to pass FramerateManager to OSD items:

```csharp
public RTSSManager(HardwareManager hardwareManager, FramerateManager framerateManager, AppServiceConnection connection)
    : base(connection)
{
    var osdItemsList = new List<OSDItem>()
    {
        // ... other items ...
        new OSDItemFPS(framerateManager),
    };

    frametimeStatsItem = new OSDItemFrametimeStats(framerateManager);
    osdItemsList.Add(frametimeStatsItem);
    
    // ... rest of code ...
}
```

Remove the old frametime data fetching code from RTSSManager.Update() since FramerateManager handles it now.

**Testing:**
1. Build solution
2. Run with RTSS mode
3. Verify overlay still works
4. Check FPS and frametime stats display

### Step 4.4: Comprehensive Game Testing

**Test Matrix:**

| Game | API | RTSS Works | PresentMon Works | Notes |
|------|-----|------------|------------------|-------|
| [Game 1] | D3D11 | ☐ Yes ☐ No | ☐ Yes ☐ No | |
| [Game 2] | D3D12 | ☐ Yes ☐ No | ☐ Yes ☐ No | |
| [Game 3] | Vulkan | ☐ Yes ☐ No | ☐ Yes ☐ No | |
| [Game 4] | OpenGL | ☐ Yes ☐ No | ☐ Yes ☐ No | |

**Test Procedure:**
1. Launch game
2. Launch Xbox Gaming Bar Helper
3. Enable overlay (level 4+)
4. Verify FPS counter appears
5. Verify frametime stats appear
6. Play for 2-3 minutes
7. Check for crashes or freezes
8. Compare FPS values with in-game counter

**Performance Test:**
1. Run benchmark game
2. Measure baseline FPS (no Xbox Gaming Bar)
3. Measure FPS with Xbox Gaming Bar + RTSS
4. Measure FPS with Xbox Gaming Bar + PresentMon
5. Calculate overhead percentage

**Expected:** <1-2% overhead

### Step 4.5: Store Build Validation

**Actions:**
1. Switch to Store configuration
2. Build solution
3. Verify RTSS code is excluded (#if !STORE)
4. Verify PresentMon is included
5. Run full test suite with Store build

**Verification:**
```powershell
# Check that RTSS DLL is NOT in Store build output
Test-Path "bin\x64\Store\...\RTSSSharedMemoryNET.dll"
# Should return: False

# Check that PresentMon IS in Store build output  
Test-Path "bin\x64\Store\...\PresentMon\PresentMon.exe"
# Should return: True
```

### ✅ Phase 4 Checklist

- [ ] OSDItemFPS updated and tested
- [ ] OSDItemFrametimeStats updated and tested
- [ ] RTSSManager refactored for FramerateManager
- [ ] Game Test Matrix completed (minimum 3 games)
- [ ] Performance overhead measured (<2%)
- [ ] Store build verified (no RTSS, has PresentMon)
- [ ] All tests passing
- [ ] Git commit: "Integrate FramerateManager with OSD system"

---

## Phase 5: Production Deployment

### Objectives
- ✅ Update documentation
- ✅ Update installer/package
- ✅ Create user migration guide
- ✅ Deploy to production

### Step 5.1: Update README and Documentation

**File:** `README.md` (if exists)

**Actions:**
Add section about framerate detection:

```markdown
## Framerate Detection

Xbox Gaming Bar supports multiple framerate detection methods:

### Intel PresentMon (Default)
- **Pros:** Store-compatible, works with all graphics APIs
- **Requirements:** User must be in "Performance Log Users" group
- **Setup:** Automatic - PresentMon is bundled with the application

### RivaTuner Statistics Server (Debug/Release only)
- **Pros:** Feature-rich, includes graphs
- **Cons:** Not available in Store builds
- **Setup:** Install RTSS separately from https://www.guru3d.com/

### Adding User to Performance Log Users Group

If PresentMon fails to start, run this PowerShell command as Administrator:

&#96;&#96;&#96;powershell
net localgroup "Performance Log Users" "$env:USERNAME" /add
&#96;&#96;&#96;

Then restart your computer.
```

### Step 5.2: Add First-Run Permission Check

**File:** `XboxGamingBarHelper\Framerate\PresentMonFramerateProvider.cs`

**Actions:**
Add permission checking and user guidance:

```csharp
private bool CheckPermissions()
{
    // Check if user is in Performance Log Users group
    var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
    var principal = new System.Security.Principal.WindowsPrincipal(identity);
    
    // SID for Performance Log Users group
    var perfLogUsers = new System.Security.Principal.SecurityIdentifier(
        System.Security.Principal.WellKnownSidType.BuiltinPerformanceLoggingUsersSid, null);
    
    if (!principal.IsInRole(perfLogUsers))
    {
        Logger.Warn("User is not in Performance Log Users group");
        // TODO: Show notification to user via AppServiceConnection
        return false;
    }
    
    return true;
}

public override bool Start()
{
    if (!CheckPermissions())
    {
        Logger.Error("Insufficient permissions for PresentMon");
        // Don't fail completely - fallback to other provider
        return false;
    }
    
    // ... rest of Start() method ...
}
```

### Step 5.3: Update Package Configuration

**File:** `XboxGamingBarPackage\Package.appxmanifest`

**Actions:**
Verify PresentMon.exe is declared as restricted capability if needed (usually not required):

```xml
<!-- No special capabilities needed for PresentMon -->
<!-- It uses standard Windows ETW APIs -->
```

**File:** `XboxGamingBarPackage\XboxGamingBarPackage.wapproj`

**Actions:**
Ensure PresentMon files are included in package:

```xml
<ItemGroup>
  <!-- PresentMon binaries from Helper project will be auto-included -->
  <!-- Verify in package output -->
</ItemGroup>
```

### Step 5.4: Create Installer Notes

**File:** `INSTALL.md` or similar

**Actions:**
```markdown
## Installation Notes

### System Requirements
- Windows 10 version 1809 or later
- .NET 8 Runtime
- User account with Performance Log Users permission (for framerate detection)

### Post-Installation Setup

#### Enable Framerate Detection
1. Run PowerShell as Administrator
2. Execute: `net localgroup "Performance Log Users" "$env:USERNAME" /add`
3. Restart your computer
4. Launch Xbox Gaming Bar

### Troubleshooting

**FPS counter not showing:**
- Verify you're in Performance Log Users group
- Check that PresentMon.exe is in the install directory
- Check logs at: `%LOCALAPPDATA%\XboxGamingBar\logs\`

**Performance impact:**
- PresentMon typically adds <1% CPU overhead
- If experiencing issues, disable overlay in settings
```

### Step 5.5: Final Testing Checklist

**Debug Build:**
- [ ] RTSS provider works (if RTSS installed)
- [ ] PresentMon provider works
- [ ] Provider fallback works (if one fails)
- [ ] Logs clearly show active provider

**Release Build:**
- [ ] RTSS provider works
- [ ] PresentMon provider works
- [ ] Build size acceptable (+5-10MB for PresentMon)

**Store Build:**
- [ ] RTSS completely excluded
- [ ] PresentMon works
- [ ] No Store policy violations
- [ ] WACK (Windows App Cert Kit) passes
- [ ] Upload test successful

### Step 5.6: Create Release

**Git Tags:**
```bash
git tag -a v1.0.0-presentmon -m "Add PresentMon support for Store builds"
git push origin v1.0.0-presentmon
```

**Release Notes:**
```markdown
## Version 1.0.0 - PresentMon Support

### New Features
- ✨ Added Intel PresentMon framerate detection
- ✨ Multi-provider framerate system (RTSS/PresentMon)
- ✨ Microsoft Store compatible builds

### Changes
- 🔄 Refactored framerate detection into provider pattern
- 🔄 Store builds now use PresentMon instead of RTSS
- 📝 Updated documentation with setup instructions

### Requirements
- Windows 10 1809+
- User must be in "Performance Log Users" group for PresentMon

### Migration
- Existing users: No action required, RTSS still works in standalone builds
- Store users: Follow setup guide for Performance Log Users permission

### Known Issues
- First run may require restart after permission changes
```

### ✅ Phase 5 Checklist

- [ ] Documentation updated (README, INSTALL)
- [ ] Permission checking implemented
- [ ] Package configuration verified
- [ ] Final testing completed (all builds)
- [ ] WACK certification passed (Store build)
- [ ] Release notes created
- [ ] Git tag created
- [ ] Deployed to production

---

## Rollback Plan

### If Critical Issues Found After Deployment

**Severity 1: App Crashes**
1. Immediately revert to previous version
2. Remove PresentMon provider registration
3. Hotfix: Add try-catch around FramerateManager calls

**Severity 2: FPS Data Incorrect**
1. Add configuration option to disable framerate detection
2. Log detailed metrics for investigation
3. Issue patch with fixes

**Severity 3: Performance Issues**
1. Add configuration for PresentMon update interval
2. Add option to disable PresentMon
3. Investigate and optimize in next release

### Rolling Back Code Changes

**Phase 5 → Phase 4:**
```bash
git revert [phase-5-commit-hash]
```

**Phase 4 → Phase 3:**
- Remove FramerateManager from OSD items
- Restore RTSS direct integration

**Phase 3 → Phase 2:**
- Remove PresentMon provider
- Remove PresentMon binaries from project

**Phase 2 → Phase 1:**
```bash
git checkout [commit-before-abstraction]
```

**Complete Rollback:**
```bash
git revert --no-commit [first-phase-2-commit]..HEAD
git commit -m "Revert PresentMon migration"
```

---

## Appendix

### A. PresentMon Command-Line Reference

```bash
# Basic usage
PresentMon.exe [options]

# Key options:
-output_stdout         # Output to console instead of file
-output_file <path>    # Output to CSV file
-process_name <name>   # Monitor specific process
-process_id <pid>      # Monitor specific PID
-timed <seconds>       # Run for specified duration
-no_top                # Disable console updates
-dont_restart_as_admin # Don't try to elevate
-terminate_on_proc_exit # Exit when target process exits

# Example: Monitor specific game
PresentMon.exe -process_name game.exe -output_stdout -no_top
```

### B. CSV Output Format

```csv
Application,ProcessID,SwapChainAddress,Runtime,SyncInterval,PresentFlags,PresentMode,
TimeInSeconds,MsBetweenPresents,MsBetweenDisplayChange,MsInPresentAPI,
MsUntilRenderComplete,MsUntilDisplayed,Dropped,QPCTime

MyGame.exe,12345,0x123456789ABC,DXGI,1,0,Hardware: Legacy Flip,
1.234,16.67,16.67,0.5,10.2,16.5,0,123456789
```

**Key Columns:**
- `MsBetweenPresents` - Time between frames (convert to FPS: 1000/value)
- `Dropped` - Dropped frame count
- `Runtime` - Graphics API (DXGI, D3D9, etc.)

### C. ETW Alternative Implementation

If PresentMon doesn't work, here's a basic ETW implementation:

**File:** `XboxGamingBarHelper\Framerate\ETWFramerateProvider.cs`

```csharp
using System;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Session;

namespace XboxGamingBarHelper.Framerate
{
    /// <summary>
    /// Direct ETW-based framerate provider (fallback option)
    /// </summary>
    public class ETWFramerateProvider : FramerateProviderBase
    {
        private TraceEventSession session;
        
        public override string ProviderName => "Windows ETW";
        
        public override bool IsInstalled => true; // ETW is built-in
        
        // Implementation requires Microsoft.Diagnostics.Tracing.TraceEvent NuGet
        // This is a simplified stub - full implementation requires ETW expertise
        
        public override bool Initialize()
        {
            // TODO: Set up ETW session to listen to DXGI provider
            return true;
        }
        
        public override bool Start()
        {
            // TODO: Start ETW trace
            return true;
        }
        
        public override void Stop()
        {
            // TODO: Stop ETW trace
        }
        
        public override void Update()
        {
            // TODO: Process ETW events
        }
    }
}
```

**NuGet Package:**
```xml
<PackageReference Include="Microsoft.Diagnostics.Tracing.TraceEvent" Version="3.0.7" />
```

### D. Troubleshooting Guide

**Problem: PresentMon.exe not found**
- Check: `bin\Debug\...\PresentMon\PresentMon.exe` exists
- Solution: Verify `CopyToOutputDirectory` is set correctly in .csproj

**Problem: PresentMon fails to start**
- Check: User in Performance Log Users group
- Solution: Run `net localgroup "Performance Log Users" "$env:USERNAME" /add` as admin

**Problem: No FPS data**
- Check: Game is using supported API (D3D/Vulkan/OpenGL)
- Check: PresentMon process is running (Task Manager)
- Solution: Add debug logging to ParsePresentMonLine method

**Problem: Incorrect FPS values**
- Check: CSV parsing is correct
- Check: Column indices match PresentMon version
- Solution: Log raw CSV line to verify format

**Problem: High CPU usage**
- Check: Update interval (currently 500ms in Program.cs)
- Solution: Increase update interval or implement throttling

### E. Performance Optimization Tips

1. **Reduce Update Frequency:**
   ```csharp
   // In Program.cs update loop
   await Task.Delay(1000); // Instead of 500ms
   ```

2. **Batch Process CSV output:**
   Parse multiple lines at once instead of line-by-line

3. **Use Ring Buffer:**
   Keep last N frame times instead of all data

4. **Lazy Initialization:**
   Only start PresentMon when overlay is enabled

### F. Future Enhancements

**Potential additions for v2.0:**
1. **ETW Direct Integration** - Eliminate PresentMon dependency
2. **Per-Application Filtering** - Only monitor active game
3. **Advanced Metrics** - 1% lows, 0.1% lows, percentiles
4. **Graph Support** - Frame time graphs (like RTSS)
5. **Remote Monitoring** - Network-based FPS sharing
6. **GPU Vendor Extensions** - NVIDIA/AMD specific enhancements

### G. License and Attribution

**PresentMon:**
- License: MIT
- Copyright: Intel Corporation
- Attribution: Include LICENSE.txt in distribution

**Required Attribution Text:**
```
This application uses Intel PresentMon for framerate detection.
PresentMon is Copyright (c) Intel Corporation
Licensed under the MIT License
```

**Add to About dialog/Settings:**
```csharp
"Performance monitoring powered by Intel PresentMon"
```

---

## Document Updates

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2026-01-26 | 1.0 | Initial creation | Assistant |

---

## Conclusion

This guide provides a comprehensive, phased approach to migrating from RTSS to PresentMon for Microsoft Store compatibility. Each phase includes:

- ✅ Clear objectives
- ✅ Step-by-step instructions
- ✅ Testing procedures
- ✅ Rollback options
- ✅ Success criteria

Follow the phases sequentially, complete all testing, and commit after each successful phase. This ensures a safe, reversible migration path.

**Estimated Total Time:** 20-31 hours  
**Risk Level:** Low (incremental, reversible changes)  
**Store Compatibility:** High (fully compliant)

Good luck with the migration! 🚀
