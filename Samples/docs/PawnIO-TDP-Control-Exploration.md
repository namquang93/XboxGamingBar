# Exploring PawnIO as a Replacement for RyzenAdj for TDP Control

**Date:** 2026-01-26  
**Author:** XboxGamingBar Development Team  
**Status:** Exploration / Research

---

## Executive Summary

This document explores the feasibility of replacing **RyzenAdj** with **PawnIO** for TDP (Thermal Design Power) control in the XboxGamingBarHelper application. The primary motivation is to avoid requiring administrator rights while maintaining the ability to adjust CPU power limits.

### Key Findings

| Aspect | RyzenAdj (Current) | PawnIO (Proposed) |
|--------|-------------------|-------------------|
| **Admin Rights** | Required | Still Required* |
| **Driver Dependency** | WinRing0 (deprecated, flagged by AV) | PawnIO (signed, secure) |
| **Security** | Raw memory access vulnerability | Script-controlled, sandboxed access |
| **Microsoft Store Compatible** | No | Potentially No** |
| **TDP Control Support** | Native | Requires RyzenSMU module |

*\*Driver installation requires admin rights, but subsequent user-mode access may not.*  
*\*\*Kernel-mode driver dependency likely violates Store policies.*

---

## Table of Contents

1. [Current Implementation (RyzenAdj)](#1-current-implementation-ryzenadj)
2. [What is PawnIO?](#2-what-is-pawnio)
3. [PawnIO Architecture](#3-pawnio-architecture)
4. [RyzenSMU Module](#4-ryzensmu-module)
5. [Admin Rights Analysis](#5-admin-rights-analysis)
6. [Implementation Approach](#6-implementation-approach)
7. [Pros and Cons](#7-pros-and-cons)
8. [Microsoft Store Considerations](#8-microsoft-store-considerations)
9. [Recommendations](#9-recommendations)
10. [References](#10-references)

---

## 1. Current Implementation (RyzenAdj)

### Overview

The current TDP control in XboxGamingBarHelper uses **RyzenAdj** via `libryzenadj.dll`. This library communicates with the AMD Ryzen processor's System Management Unit (SMU) to adjust power limits.

### Current Code Location

- **Wrapper:** `XboxGamingBarHelper/Hardware/RyzenAdj.cs`
- **Usage:** `XboxGamingBarHelper/Hardware/HardwareManager.cs`

### Key Functions Used

```csharp
// Initialization
IntPtr ryzenAdjHandle = RyzenAdj.init_ryzenadj();

// Reading TDP
RyzenAdj.refresh_table(ryzenAdjHandle);
float currentTDP = RyzenAdj.get_stapm_limit(ryzenAdjHandle);

// Setting TDP
RyzenAdj.set_fast_limit(ryzenAdjHandle, (uint)((tdp + 10) * 1000));
RyzenAdj.set_slow_limit(ryzenAdjHandle, (uint)((tdp + 5) * 1000));
RyzenAdj.set_stapm_limit(ryzenAdjHandle, (uint)(tdp * 1000));
```

### Dependencies

- `libryzenadj.dll` - RyzenAdj library
- `WinRing0x64.dll` / `WinRing0x64.sys` - Kernel driver for hardware access
- **Administrator rights required** for driver loading and SMU access

### Current Problems

1. **Admin Rights:** Application must run elevated to access hardware
2. **WinRing0 Security Issues:** 
   - Flagged by Windows Defender and antivirus software
   - Microsoft has banned WinRing0 in some contexts
   - Known security vulnerabilities (raw memory access)
3. **Store Incompatibility:** Disabled in Store builds (`#if !STORE`)

---

## 2. What is PawnIO?

**PawnIO** is a modern, signed Windows kernel-mode driver developed by [namazso](https://github.com/namazso) as a secure alternative to WinRing0.

### Key Features

- **Signed Driver:** Digitally signed, avoiding AV false positives
- **Script-Based Access:** Uses Pawn scripting language for controlled hardware access
- **Modular Design:** Separate modules for different hardware types
- **Security-Focused:** No raw memory access; all operations are script-controlled

### Official Resources

- **Website:** [pawnio.eu](https://pawnio.eu)
- **Driver Source:** [github.com/namazso/PawnIO](https://github.com/namazso/PawnIO)
- **Wrapper Library:** [github.com/namazso/PawnIOLib](https://github.com/namazso/PawnIOLib)
- **Modules:** [github.com/namazso/PawnIO.Modules](https://github.com/namazso/PawnIO.Modules)

---

## 3. PawnIO Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    User Application                          │
│                  (XboxGamingBarHelper)                       │
└─────────────────────┬───────────────────────────────────────┘
                      │ LoadLibrary
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                    PawnIOLib.dll                             │
│                 (User-mode wrapper)                          │
│  Functions: pawnio_open, pawnio_load, pawnio_execute        │
└─────────────────────┬───────────────────────────────────────┘
                      │ DeviceIoControl
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                    PawnIO.sys                                │
│                  (Kernel-mode driver)                        │
│            Executes Pawn scripts in Ring 0                   │
└─────────────────────┬───────────────────────────────────────┘
                      │ Module Script
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                   RyzenSMU.p Module                          │
│          (Pawn script for AMD SMU communication)             │
│       Provides controlled access to SMU registers            │
└─────────────────────────────────────────────────────────────┘
```

### Workflow

1. **Find PawnIO Installation:** Read registry key `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO`
2. **Load PawnIOLib:** `LoadLibrary(InstallLocation + "PawnIOLib")`
3. **Resolve Functions:** Use `GetProcAddress` to get function pointers
4. **Load Module:** Open handle and load the RyzenSMU module blob
5. **Execute IOCTLs:** Call module functions for SMU communication

---

## 4. RyzenSMU Module

The **RyzenSMU** module is an official PawnIO module that provides access to AMD Ryzen's System Management Unit.

### Available in PawnIO Modules

- Located in [PawnIO.Modules releases](https://github.com/namazso/PawnIO.Modules/releases)
- File: `RyzenSMU.p` (compiled to `.amx` blob)

### Capabilities (Based on ryzen_smu Linux Driver)

The SMU can control:

| SMU Function | Description |
|--------------|-------------|
| STAPM Limit | Sustained power limit (main TDP) |
| Fast Limit (PPT) | Peak Package Power |
| Slow Limit (PPT) | Sustained Package Power |
| Temperature Limits | Max CPU temperature |
| Clock Controls | Min/Max frequencies |

### SMU Message Format

Communication with the SMU involves sending messages to specific registers:

```
SMU Message = [Message ID] + [Arguments]
Response = [Status] + [Return Values]
```

### Current Module Status

Based on recent PawnIO.Modules releases (0.2.2):
- RyzenSMU module is actively maintained
- Supports modern platforms including StrixPoint (Z2 Extreme)
- Includes AMD CSTATE MSR support

---

## 5. Admin Rights Analysis

### The Admin Rights Question

**Question:** Can PawnIO eliminate the need for admin rights for TDP control?

**Answer:** Partially, but with significant caveats.

### Breakdown

| Operation | Admin Required? | Notes |
|-----------|----------------|-------|
| PawnIO Driver Installation | **Yes** | One-time setup by user |
| PawnIO Service Start | **Yes** | At system boot |
| Opening PawnIO Handle | **No*** | After driver is running |
| Loading Module Blob | **No*** | Into existing handle |
| Executing SMU Commands | **No*** | Through loaded module |

*\*Depends on driver configuration and permissions.*

### Key Insight

Once PawnIO is **installed and running as a system service**, user-mode applications can potentially communicate with it **without elevation**, depending on the driver's security descriptor.

### Current Behavior (FanControl Example)

Applications like FanControl use PawnIO without requiring the application itself to run as administrator. However:
1. The **PawnIO driver must be installed** (requires admin once)
2. The driver runs as a **system service** with kernel privileges
3. User applications communicate via **DeviceIoControl**

---

## 6. Implementation Approach

If moving forward with PawnIO, here's the proposed implementation:

### 6.1 Prerequisites Check

```csharp
public static class PawnIOHelper
{
    private const string PawnIORegistryKey = 
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO";
    
    public static bool IsPawnIOInstalled()
    {
        using var key = Registry.LocalMachine.OpenSubKey(PawnIORegistryKey);
        return key != null;
    }
    
    public static string GetInstallLocation()
    {
        using var key = Registry.LocalMachine.OpenSubKey(PawnIORegistryKey);
        return key?.GetValue("InstallLocation") as string 
               ?? @"C:\Program Files\PawnIO";
    }
}
```

### 6.2 PawnIOLib P/Invoke Declarations

```csharp
internal static class PawnIOLib
{
    private static IntPtr _libHandle;
    
    // Function delegates
    public delegate int PawnIOOpen(out IntPtr handle);
    public delegate int PawnIOClose(IntPtr handle);
    public delegate int PawnIOLoad(IntPtr handle, byte[] blob, int size);
    public delegate int PawnIOExecute(
        IntPtr handle, 
        string ioctl, 
        byte[] input, 
        int inputSize,
        byte[] output, 
        int outputSize, 
        out int returnSize);
    
    public static PawnIOOpen Open;
    public static PawnIOClose Close;
    public static PawnIOLoad Load;
    public static PawnIOExecute Execute;
    
    public static bool Initialize(string installPath)
    {
        _libHandle = NativeLibrary.Load(Path.Combine(installPath, "PawnIOLib.dll"));
        if (_libHandle == IntPtr.Zero) return false;
        
        Open = Marshal.GetDelegateForFunctionPointer<PawnIOOpen>(
            NativeLibrary.GetExport(_libHandle, "pawnio_open"));
        // ... resolve other functions
        
        return true;
    }
}
```

### 6.3 RyzenSMU TDP Controller

```csharp
public class PawnIOTDPController : IDisposable
{
    private IntPtr _handle;
    private readonly byte[] _ryzenSmuModule;
    
    public PawnIOTDPController()
    {
        // Load embedded RyzenSMU.amx module blob
        _ryzenSmuModule = LoadEmbeddedResource("RyzenSMU.amx");
    }
    
    public bool Initialize()
    {
        if (!PawnIOHelper.IsPawnIOInstalled())
            return false;
            
        var installPath = PawnIOHelper.GetInstallLocation();
        if (!PawnIOLib.Initialize(installPath))
            return false;
            
        if (PawnIOLib.Open(out _handle) != 0)
            return false;
            
        if (PawnIOLib.Load(_handle, _ryzenSmuModule, _ryzenSmuModule.Length) != 0)
            return false;
            
        return true;
    }
    
    public int GetSTAPMLimit()
    {
        // Call RyzenSMU ioctl to read STAPM limit
        // Implementation depends on RyzenSMU module interface
    }
    
    public void SetSTAPMLimit(int watts)
    {
        // Call RyzenSMU ioctl to set STAPM limit
        // Implementation depends on RyzenSMU module interface
    }
    
    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
            PawnIOLib.Close(_handle);
    }
}
```

### 6.4 Integration with HardwareManager

```csharp
internal class HardwareManager : Manager
{
    private readonly ITDPController _tdpController;
    
    internal HardwareManager(AppServiceConnection connection) : base(connection)
    {
        // Try PawnIO first, fall back to RyzenAdj
        #if !STORE
        if (PawnIOTDPController.IsPawnIOAvailable())
        {
            _tdpController = new PawnIOTDPController();
            Logger.Info("Using PawnIO for TDP control");
        }
        else
        {
            _tdpController = new RyzenAdjTDPController();
            Logger.Info("Using RyzenAdj for TDP control");
        }
        #endif
    }
}
```

---

## 7. Pros and Cons

### Pros ✅

| Advantage | Description |
|-----------|-------------|
| **Signed Driver** | No antivirus/Windows Defender issues |
| **Modern & Maintained** | Active development, community support |
| **Secure Design** | No raw memory access vulnerabilities |
| **Script-Based** | Easy to extend for new CPU generations |
| **Shared Installation** | User may already have PawnIO for FanControl |
| **Potentially No Elevation** | After driver installation, app may run unelevated |

### Cons ❌

| Disadvantage | Description |
|--------------|-------------|
| **External Dependency** | Requires user to install PawnIO separately |
| **More Complex Integration** | Need to embed module, handle dynamic loading |
| **Uncertain SMU Interface** | RyzenSMU module API not well documented |
| **Still Kernel Access** | Fundamentally requires kernel driver |
| **Store Incompatible** | Likely violates Microsoft Store policies |
| **Licensing Considerations** | GPL/LGPL licensing may affect distribution |

---

## 8. Microsoft Store Considerations

### Store Policy Analysis

The Microsoft Store has strict policies regarding:

1. **Kernel-Mode Drivers:** Not allowed in Store apps
2. **Hardware-Level Access:** Restricted to approved APIs
3. **External Dependencies:** Must be bundled or store-distributed

### Impact on Store Build

| Scenario | PawnIO | RyzenAdj |
|----------|--------|----------|
| Store Build | ❌ Not compatible | ❌ Not compatible |
| Sideload Build | ✅ Works with PawnIO installed | ✅ Works with admin |
| GitHub Release | ✅ Can bundle instructions | ✅ Works as-is |

### Conclusion for Store

**Neither RyzenAdj nor PawnIO is suitable for the Microsoft Store version.** TDP control will remain a sideload-only feature.

---

## 9. Recommendations

### Short-Term (Keep RyzenAdj)

For the immediate future, **continue using RyzenAdj** because:
- It works and is well-tested
- The integration is already complete
- Switching provides marginal benefit for the effort

### Medium-Term (Consider PawnIO)

Consider switching to PawnIO if:
- WinRing0 becomes completely blocked by Windows
- PawnIO becomes more widely adopted (as with FanControl)
- Better documentation for RyzenSMU module emerges

### Long-Term (Monitor AMD Official Solutions)

Watch for:
- AMD official TDP control APIs
- Windows power management improvements
- Microsoft relaxing hardware access policies

### Recommendation Matrix

| Priority | Action |
|----------|--------|
| 1 | Keep current RyzenAdj implementation working |
| 2 | Add graceful fallback when TDP control unavailable |
| 3 | Create abstraction layer (`ITDPController`) for future flexibility |
| 4 | Monitor PawnIO development and adoption |
| 5 | Consider user documentation for PawnIO as optional enhancement |

---

## 10. References

### RyzenAdj
- GitHub: https://github.com/FlyGoat/RyzenAdj
- Documentation: https://github.com/FlyGoat/RyzenAdj#readme

### PawnIO
- Official Website: https://pawnio.eu
- Driver Source: https://github.com/namazso/PawnIO
- Library Source: https://github.com/namazso/PawnIOLib
- Modules: https://github.com/namazso/PawnIO.Modules
- Wiki: https://github.com/namazso/PawnIO.Modules/wiki

### AMD SMU
- ryzen_smu Linux Driver: https://github.com/leogx9r/ryzen_smu
- SMU Documentation: https://gitlab.com/leogx9r/ryzen_smu

### Related Projects Using PawnIO
- FanControl: https://github.com/Rem0o/FanControl.Releases
- SMUDebugTool: https://github.com/irusanov/SMUDebugTool

---

## Appendix A: Files to Modify for Migration

If proceeding with PawnIO migration:

| File | Changes |
|------|---------|
| `Hardware/RyzenAdj.cs` | Keep as fallback implementation |
| `Hardware/HardwareManager.cs` | Add abstraction layer |
| **New:** `Hardware/ITDPController.cs` | Interface for TDP control |
| **New:** `Hardware/PawnIOTDPController.cs` | PawnIO implementation |
| **New:** `Hardware/RyzenAdjTDPController.cs` | Refactored RyzenAdj implementation |
| `XboxGamingBarHelper.csproj` | Add embedded RyzenSMU module resource |

---

## Appendix B: User Experience Impact

### Current Flow (RyzenAdj)
1. User downloads XboxGamingBar
2. User runs as Administrator
3. TDP control works immediately

### Proposed Flow (PawnIO)
1. User downloads XboxGamingBar
2. User downloads and installs PawnIO from pawnio.eu
3. User runs XboxGamingBar (possibly without admin)
4. TDP control works if PawnIO is detected

### Hybrid Approach (Recommended)
1. User downloads XboxGamingBar
2. App checks for PawnIO → uses if available
3. App falls back to RyzenAdj → prompts for admin if needed
4. TDP control works in either case

---

## Appendix C: Step-by-Step Implementation Guide

This section provides a precise, implementation-ready guide for integrating PawnIO with TDP control, based on official documentation and real-world implementations from **SMUDebugTool** and **ZenStates-Core**.

---

### Step 1: Prerequisites and Dependencies

#### 1.1 Install PawnIO on Development Machine

```powershell
# Download and run the PawnIO installer
# Official download: https://github.com/namazso/PawnIO.Setup/releases/latest/download/PawnIO_setup.exe

# After installation, verify:
# - PawnIO.sys driver is loaded (check Device Manager or Services)
# - PawnIOLib.dll exists in C:\Program Files\PawnIO\
```

#### 1.2 Download PawnIO Modules

```powershell
# Download the latest RyzenSMU module from:
# https://github.com/namazso/PawnIO.Modules/releases

# Files needed:
# - RyzenSMU.p (compiled module blob)
# - Include in your project as embedded resource
```

#### 1.3 Reference ZenStates-Core (Recommended)

Instead of implementing PawnIO integration from scratch, use the **ZenStates-Core** library which already has PawnIO support:

```powershell
# NuGet Package (if available) or direct reference:
# https://github.com/irusanov/ZenStates-Core/releases

# Add to your project:
# - ZenStates.Core.dll
```

---

### Step 2: Understanding SMU Message IDs for TDP Control

The AMD System Management Unit (SMU) uses specific message IDs to control power limits. These are the key IDs for TDP adjustment:

#### 2.1 SMU Message IDs (MP1)

| Message ID | Hex Value | Description | Argument |
|------------|-----------|-------------|----------|
| `SetSustainedPowerLimit` | `0x1A` | STAPM Limit (main TDP) | Power in mW |
| `SetFastPPTLimit` | `0x1B` | Fast PPT (burst power) | Power in mW |
| `SetSlowPPTLimit` | `0x1C` | Slow PPT (sustained) | Power in mW |
| `SetSlowPPTTimeConstant` | `0x1D` | Slow PPT time window | Time in ms |
| `SetStapmTimeConstant` | `0x1E` | STAPM time window | Time in ms |
| `SetTctlMax` | `0x1F` | Max temperature | Temp in °C |
| `SetVrmCurrentLimit` | `0x20` | VRM current limit | Current in mA |
| `SetVrmSocCurrentLimit` | `0x21` | SoC VRM current limit | Current in mA |
| `TransferTableSmu2Dram` | `0x16` | Read PM table to DRAM | N/A |
| `TransferTableDram2Smu` | `0x17` | Write PM table from DRAM | N/A |

#### 2.2 SMU Response Codes

| Response | Value | Meaning |
|----------|-------|---------|
| `OK` | `0x01` | Command successful |
| `Failed` | `0x00` | Command failed |
| `UnknownCmd` | `0xFE` | Unknown command |
| `CmdRejected` | `0xFD` | Command rejected |
| `Busy` | `0xFC` | SMU busy |

---

### Step 3: Using ZenStates-Core Library (Recommended Approach)

ZenStates-Core is a well-maintained C# library that already integrates with PawnIO. This is the easiest implementation path.

#### 3.1 Add ZenStates-Core Reference

```xml
<!-- In XboxGamingBarHelper.csproj -->
<ItemGroup Condition="'$(Configuration)' != 'Store'">
  <Reference Include="ZenStates.Core">
    <HintPath>libs\ZenStates.Core.dll</HintPath>
  </Reference>
</ItemGroup>
```

#### 3.2 Initialize ZenStates CPU Module

```csharp
using ZenStates.Core;

public class ZenStatesTDPController : ITDPController, IDisposable
{
    private readonly Cpu _cpu;
    private bool _initialized;

    public ZenStatesTDPController()
    {
        try
        {
            // ZenStates.Core automatically detects and uses PawnIO if available
            // Falls back to WinRing0 if PawnIO is not installed
            _cpu = new Cpu();
            _initialized = _cpu.Status == Cpu.Status.OK;
            
            if (_initialized)
            {
                Logger.Info($"ZenStates initialized: {_cpu.info.codeName}");
                Logger.Info($"SMU Version: {_cpu.smu.Version}");
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"ZenStates initialization failed: {ex.Message}");
            _initialized = false;
        }
    }

    public bool IsSupported => _initialized;
    
    public void Dispose()
    {
        _cpu?.Dispose();
    }
}
```

#### 3.3 Send SMU Commands for TDP Control

```csharp
public class ZenStatesTDPController : ITDPController, IDisposable
{
    // ... initialization code from above ...

    /// <summary>
    /// Set the STAPM (sustained) power limit
    /// </summary>
    /// <param name="watts">TDP in watts</param>
    /// <returns>True if successful</returns>
    public bool SetStapmLimit(uint watts)
    {
        if (!_initialized) return false;

        // Convert watts to milliwatts (SMU expects mW)
        uint milliwatts = watts * 1000;
        
        // SMU command arguments (up to 6 arguments)
        uint[] args = new uint[6];
        args[0] = milliwatts;

        // Send the SetSustainedPowerLimit command (0x1A)
        var result = _cpu.smu.SendSmuCommand(
            _cpu.smu.Addresses.Rsmu,  // RSmu mailbox
            0x1A,                      // SetSustainedPowerLimit
            ref args
        );

        if (result == SMU.Status.OK)
        {
            Logger.Info($"STAPM limit set to {watts}W");
            return true;
        }
        else
        {
            Logger.Error($"Failed to set STAPM limit: {result}");
            return false;
        }
    }

    /// <summary>
    /// Set the Fast PPT (boost) power limit
    /// </summary>
    public bool SetFastPPTLimit(uint watts)
    {
        if (!_initialized) return false;

        uint milliwatts = watts * 1000;
        uint[] args = new uint[6];
        args[0] = milliwatts;

        var result = _cpu.smu.SendSmuCommand(
            _cpu.smu.Addresses.Rsmu,
            0x1B,  // SetFastPPTLimit
            ref args
        );

        return result == SMU.Status.OK;
    }

    /// <summary>
    /// Set the Slow PPT (sustained) power limit
    /// </summary>
    public bool SetSlowPPTLimit(uint watts)
    {
        if (!_initialized) return false;

        uint milliwatts = watts * 1000;
        uint[] args = new uint[6];
        args[0] = milliwatts;

        var result = _cpu.smu.SendSmuCommand(
            _cpu.smu.Addresses.Rsmu,
            0x1C,  // SetSlowPPTLimit
            ref args
        );

        return result == SMU.Status.OK;
    }

    /// <summary>
    /// Combined TDP setting (matches current RyzenAdj behavior)
    /// </summary>
    public bool SetTDP(int tdpWatts)
    {
        if (!_initialized) return false;

        // Apply same offsets as current RyzenAdj implementation:
        // Fast Limit = TDP + 10W
        // Slow Limit = TDP + 5W  
        // STAPM Limit = TDP
        
        bool success = true;
        success &= SetFastPPTLimit((uint)(tdpWatts + 10));
        success &= SetSlowPPTLimit((uint)(tdpWatts + 5));
        success &= SetStapmLimit((uint)tdpWatts);

        return success;
    }

    /// <summary>
    /// Get current STAPM limit by reading PM table
    /// </summary>
    public int GetCurrentTDP()
    {
        if (!_initialized) return -1;

        // Request PM table transfer from SMU to DRAM
        uint[] args = new uint[6];
        var result = _cpu.smu.SendSmuCommand(
            _cpu.smu.Addresses.Rsmu,
            0x16,  // TransferTableSmu2Dram
            ref args
        );

        if (result == SMU.Status.OK)
        {
            // Read STAPM limit from PM table
            // Note: PM table structure varies by CPU generation
            // This is a simplified example
            return (int)(_cpu.powerTable?.StapmLimit ?? 25);
        }

        return 25; // Default fallback
    }
}
```

---

### Step 4: Direct PawnIO Implementation (Alternative)

If you prefer not to use ZenStates-Core, here's the direct PawnIO implementation:

#### 4.1 PawnIOLib P/Invoke Definitions

```csharp
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace XboxGamingBarHelper.Hardware
{
    /// <summary>
    /// Native PawnIO library wrapper
    /// Based on PawnIOLib.h from official PawnIO installation
    /// </summary>
    internal static class PawnIONative
    {
        // PawnIO result codes
        public const int PAWNIO_OK = 0;
        public const int PAWNIO_ERROR_NOT_FOUND = 1;
        public const int PAWNIO_ERROR_ACCESS_DENIED = 2;
        public const int PAWNIO_ERROR_INVALID_PARAMETER = 3;
        
        private static IntPtr _libHandle = IntPtr.Zero;
        
        // Function pointer delegates
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int PawnIOOpenDelegate(out IntPtr handle);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int PawnIOCloseDelegate(IntPtr handle);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int PawnIOLoadDelegate(
            IntPtr handle, 
            byte[] blob, 
            UIntPtr size);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int PawnIOExecuteDelegate(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPStr)] string ioctlName,
            byte[] inputData,
            UIntPtr inputSize,
            byte[] outputData,
            UIntPtr outputSize,
            out UIntPtr returnedSize);

        // Resolved function pointers
        public static PawnIOOpenDelegate Open { get; private set; }
        public static PawnIOCloseDelegate Close { get; private set; }
        public static PawnIOLoadDelegate Load { get; private set; }
        public static PawnIOExecuteDelegate Execute { get; private set; }

        /// <summary>
        /// Check if PawnIO is installed
        /// </summary>
        public static bool IsInstalled()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO");
                return key != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get PawnIO installation path
        /// </summary>
        public static string GetInstallPath()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PawnIO");
                return key?.GetValue("InstallLocation") as string 
                       ?? @"C:\Program Files\PawnIO";
            }
            catch
            {
                return @"C:\Program Files\PawnIO";
            }
        }

        /// <summary>
        /// Initialize PawnIO library
        /// </summary>
        public static bool Initialize()
        {
            if (!IsInstalled())
                return false;

            string installPath = GetInstallPath();
            string libPath = System.IO.Path.Combine(installPath, "PawnIOLib.dll");

            try
            {
                _libHandle = NativeLibrary.Load(libPath);
                if (_libHandle == IntPtr.Zero)
                    return false;

                // Resolve function pointers
                Open = Marshal.GetDelegateForFunctionPointer<PawnIOOpenDelegate>(
                    NativeLibrary.GetExport(_libHandle, "pawnio_open"));
                    
                Close = Marshal.GetDelegateForFunctionPointer<PawnIOCloseDelegate>(
                    NativeLibrary.GetExport(_libHandle, "pawnio_close"));
                    
                Load = Marshal.GetDelegateForFunctionPointer<PawnIOLoadDelegate>(
                    NativeLibrary.GetExport(_libHandle, "pawnio_load"));
                    
                Execute = Marshal.GetDelegateForFunctionPointer<PawnIOExecuteDelegate>(
                    NativeLibrary.GetExport(_libHandle, "pawnio_execute"));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load PawnIO: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public static void Shutdown()
        {
            if (_libHandle != IntPtr.Zero)
            {
                NativeLibrary.Free(_libHandle);
                _libHandle = IntPtr.Zero;
            }
        }
    }
}
```

#### 4.2 RyzenSMU Module Wrapper

```csharp
using System;
using System.IO;
using System.Reflection;

namespace XboxGamingBarHelper.Hardware
{
    /// <summary>
    /// PawnIO RyzenSMU module wrapper for TDP control
    /// </summary>
    public class PawnIOTDPController : ITDPController, IDisposable
    {
        private IntPtr _handle = IntPtr.Zero;
        private bool _initialized = false;
        private byte[] _ryzenSmuModule;

        public bool IsSupported => _initialized;

        public PawnIOTDPController()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize PawnIO and load RyzenSMU module
        /// </summary>
        private bool Initialize()
        {
            try
            {
                // Step 1: Initialize PawnIO library
                if (!PawnIONative.Initialize())
                {
                    Logger.Error("PawnIO not installed or failed to load");
                    return false;
                }

                // Step 2: Open PawnIO handle
                int result = PawnIONative.Open(out _handle);
                if (result != PawnIONative.PAWNIO_OK || _handle == IntPtr.Zero)
                {
                    Logger.Error($"Failed to open PawnIO handle: {result}");
                    return false;
                }

                // Step 3: Load RyzenSMU module blob
                _ryzenSmuModule = LoadEmbeddedModule("RyzenSMU.amx");
                if (_ryzenSmuModule == null || _ryzenSmuModule.Length == 0)
                {
                    Logger.Error("Failed to load RyzenSMU module");
                    return false;
                }

                result = PawnIONative.Load(
                    _handle, 
                    _ryzenSmuModule, 
                    (UIntPtr)_ryzenSmuModule.Length);
                    
                if (result != PawnIONative.PAWNIO_OK)
                {
                    Logger.Error($"Failed to load RyzenSMU module: {result}");
                    return false;
                }

                _initialized = true;
                Logger.Info("PawnIO TDP controller initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"PawnIO initialization error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load embedded RyzenSMU.amx module from resources
        /// </summary>
        private byte[] LoadEmbeddedModule(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fullResourceName = $"{assembly.GetName().Name}.Resources.{resourceName}";
            
            using var stream = assembly.GetManifestResourceStream(fullResourceName);
            if (stream == null)
            {
                // Try loading from file as fallback
                string modulePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, 
                    "Modules", 
                    resourceName);
                    
                if (File.Exists(modulePath))
                    return File.ReadAllBytes(modulePath);
                    
                return null;
            }

            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Execute an ioctl on the RyzenSMU module
        /// </summary>
        private bool ExecuteIoctl(string ioctlName, byte[] input, byte[] output = null)
        {
            if (!_initialized) return false;

            try
            {
                UIntPtr returnedSize;
                int result = PawnIONative.Execute(
                    _handle,
                    ioctlName,
                    input,
                    (UIntPtr)(input?.Length ?? 0),
                    output,
                    (UIntPtr)(output?.Length ?? 0),
                    out returnedSize);

                return result == PawnIONative.PAWNIO_OK;
            }
            catch (Exception ex)
            {
                Logger.Error($"IOCTL execution error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Set TDP using SMU messages
        /// Note: Exact ioctl names depend on RyzenSMU module implementation
        /// </summary>
        public bool SetTDP(int tdpWatts)
        {
            if (!_initialized) return false;

            // Prepare arguments for SMU message
            // Format: [MessageID:4bytes][Arg0:4bytes][Arg1:4bytes]...
            
            // Set Fast PPT (TDP + 10W)
            byte[] fastPPTArgs = BitConverter.GetBytes((uint)((tdpWatts + 10) * 1000));
            if (!ExecuteIoctl("ioctl_set_fast_ppt", fastPPTArgs))
                Logger.Warning("Failed to set Fast PPT");

            // Set Slow PPT (TDP + 5W)
            byte[] slowPPTArgs = BitConverter.GetBytes((uint)((tdpWatts + 5) * 1000));
            if (!ExecuteIoctl("ioctl_set_slow_ppt", slowPPTArgs))
                Logger.Warning("Failed to set Slow PPT");

            // Set STAPM (main TDP)
            byte[] stapmArgs = BitConverter.GetBytes((uint)(tdpWatts * 1000));
            if (!ExecuteIoctl("ioctl_set_stapm", stapmArgs))
            {
                Logger.Error("Failed to set STAPM limit");
                return false;
            }

            Logger.Info($"TDP set to {tdpWatts}W via PawnIO");
            return true;
        }

        /// <summary>
        /// Get current TDP
        /// </summary>
        public int GetCurrentTDP()
        {
            if (!_initialized) return 25; // Default fallback

            byte[] output = new byte[4];
            if (ExecuteIoctl("ioctl_get_stapm", null, output))
            {
                uint milliwatts = BitConverter.ToUInt32(output, 0);
                return (int)(milliwatts / 1000);
            }

            return 25;
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                PawnIONative.Close(_handle);
                _handle = IntPtr.Zero;
            }
            PawnIONative.Shutdown();
            _initialized = false;
        }
    }
}
```

---

### Step 5: Integration with HardwareManager

#### 5.1 Create ITDPController Interface

```csharp
// File: Hardware/ITDPController.cs
namespace XboxGamingBarHelper.Hardware
{
    /// <summary>
    /// Interface for TDP control implementations
    /// </summary>
    public interface ITDPController : IDisposable
    {
        /// <summary>
        /// Check if TDP control is supported on this system
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Get the current TDP in watts
        /// </summary>
        int GetCurrentTDP();

        /// <summary>
        /// Set the TDP in watts
        /// </summary>
        bool SetTDP(int watts);
    }
}
```

#### 5.2 Wrap Existing RyzenAdj Implementation

```csharp
// File: Hardware/RyzenAdjTDPController.cs
namespace XboxGamingBarHelper.Hardware
{
    /// <summary>
    /// RyzenAdj-based TDP controller (existing implementation)
    /// </summary>
    internal class RyzenAdjTDPController : ITDPController
    {
        private readonly IntPtr _handle;

        public bool IsSupported => _handle != IntPtr.Zero;

        public RyzenAdjTDPController()
        {
            _handle = RyzenAdj.init_ryzenadj();
        }

        public int GetCurrentTDP()
        {
            if (_handle == IntPtr.Zero) return 25;
            RyzenAdj.refresh_table(_handle);
            return (int)RyzenAdj.get_stapm_limit(_handle);
        }

        public bool SetTDP(int watts)
        {
            if (_handle == IntPtr.Zero) return false;

            RyzenAdj.set_fast_limit(_handle, (uint)((watts + 10) * 1000));
            RyzenAdj.set_slow_limit(_handle, (uint)((watts + 5) * 1000));
            RyzenAdj.set_stapm_limit(_handle, (uint)(watts * 1000));
            
            return true;
        }

        public void Dispose()
        {
            // RyzenAdj doesn't have explicit cleanup
        }
    }
}
```

#### 5.3 Update HardwareManager to Use Abstraction

```csharp
// In HardwareManager.cs
internal class HardwareManager : Manager
{
    private readonly ITDPController _tdpController;

    internal HardwareManager(AppServiceConnection connection) : base(connection)
    {
#if !STORE
        // Try PawnIO first (if user has it installed)
        if (PawnIONative.IsInstalled())
        {
            var pawnioController = new PawnIOTDPController();
            if (pawnioController.IsSupported)
            {
                _tdpController = pawnioController;
                Logger.Info("Using PawnIO for TDP control (no admin required)");
            }
        }

        // Fall back to RyzenAdj
        if (_tdpController == null)
        {
            var ryzenAdjController = new RyzenAdjTDPController();
            if (ryzenAdjController.IsSupported)
            {
                _tdpController = ryzenAdjController;
                Logger.Info("Using RyzenAdj for TDP control (admin required)");
            }
        }

        // Initialize TDP properties
        if (_tdpController?.IsSupported == true)
        {
            int initialTDP = _tdpController.GetCurrentTDP();
            tdpControlSupport = new TDPControlSupportProperty(true, this);
            tdp = new TDPProperty(initialTDP, null, this);
        }
        else
        {
            Logger.Warning("No TDP controller available");
            tdpControlSupport = new TDPControlSupportProperty(false, this);
        }
#else
        Logger.Info("TDP control is disabled in Store build");
        tdpControlSupport = new TDPControlSupportProperty(false, this);
#endif
    }

    public int GetTDP()
    {
        return _tdpController?.GetCurrentTDP() ?? 25;
    }

    public void SetTDP(int tdp)
    {
        _tdpController?.SetTDP(tdp);
    }
}
```

---

### Step 6: Embed RyzenSMU Module

#### 6.1 Download Module from PawnIO.Modules Releases

1. Go to https://github.com/namazso/PawnIO.Modules/releases
2. Download the latest release ZIP
3. Extract `RyzenSMU.amx` (compiled Pawn module)

#### 6.2 Add as Embedded Resource

```xml
<!-- In XboxGamingBarHelper.csproj -->
<ItemGroup Condition="'$(Configuration)' != 'Store'">
  <EmbeddedResource Include="Resources\RyzenSMU.amx">
    <LogicalName>XboxGamingBarHelper.Resources.RyzenSMU.amx</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

#### 6.3 Alternative: Include as Content File

```xml
<ItemGroup Condition="'$(Configuration)' != 'Store'">
  <Content Include="Modules\RyzenSMU.amx">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

---

### Step 7: Testing and Verification

#### 7.1 Test Checklist

- [ ] PawnIO driver installed and running
- [ ] RyzenSMU module loads successfully
- [ ] Can read current STAPM limit
- [ ] Can set STAPM limit
- [ ] Fast/Slow PPT limits apply correctly
- [ ] Values persist across refresh
- [ ] Graceful fallback to RyzenAdj when PawnIO unavailable

#### 7.2 Debug Logging

```csharp
// Add detailed logging during development
Logger.Debug($"PawnIO installed: {PawnIONative.IsInstalled()}");
Logger.Debug($"PawnIO path: {PawnIONative.GetInstallPath()}");
Logger.Debug($"Module loaded: {_ryzenSmuModule?.Length ?? 0} bytes");
Logger.Debug($"Handle: {_handle:X}");
```

---

### Step 8: User Documentation

#### 8.1 User Instructions (Add to README)

```markdown
## TDP Control Setup

### Option 1: Run as Administrator (Default)
The app uses RyzenAdj which requires administrator rights.
Right-click the app → Run as administrator.

### Option 2: Install PawnIO (No Admin Required)
1. Download PawnIO from https://pawnio.eu
2. Run the installer (requires admin once)
3. Restart your computer
4. Run XboxGamingBar normally - TDP control will work without admin!

Note: PawnIO is also used by FanControl and other hardware tools.
If you already have PawnIO installed, TDP control will work automatically.
```

---

### Troubleshooting Guide

| Issue | Solution |
|-------|----------|
| PawnIO not detected | Verify installation at `C:\Program Files\PawnIO` |
| Module load fails | Check that RyzenSMU.amx is included in build output |
| SMU commands fail | CPU may not be supported; check ZenStates compatibility |
| Access denied errors | PawnIO driver may not be running; restart system |
| Values don't persist | Some systems reset TDP on certain events; reapply as needed |

---

*Document created: 2026-01-26*  
*Last updated: 2026-01-26*
