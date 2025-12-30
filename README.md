# Xbox Gaming Bar

## What is it?

XBox Gaming Bar is a helper tool for gamers to control all gaming-related settings using the gamepad/game controller.
Xbox Gaming Bar is built as an Xbox Game Bar widget as the frontend, and a Win32 helper as the backend tool.
As of now, there are the following functions:
- Performance Overlay using RivaTuner Statistics Server OSD.
- TDP Limit.
- Per-game Profile.
- CPU performance adjustments.
  - Enable or disable CPU Boost.
  - Set CPU Energy Performance Preference (EPP).
  - Set CPU clock speed limit.
- AMD Settings.
  - Radeon Super Resolution.
  - AMD Fluid Motion Frame.
  - Radeon Anti-Lag.
  - Radeon Boost.
  - Radeon Chill.

![alt text](Screenshots/v3_2.png)

## Installation

There are 2 different versions of the application. The Microsoft Store version is much easier to install, but missing some main features, including TDP control (because it requires administrator rights, which is not allowed by Microsoft). The Sideload version needs some additional setups, but has all features.

### Microsoft Store

[<img width="400" height="109" alt="StoreBadge-light" src="https://github.com/user-attachments/assets/43b740a7-ba12-44b5-acaa-d58affcaf973" />](https://apps.microsoft.com/detail/9njzds2l3trv)

### Sideload

Grab the latest release [here](https://github.com/namquang93/XboxGamingBar/releases/latest) and follow our [Wiki](https://github.com/namquang93/XboxGamingBar/wiki/Installation-Instruction) page for installation instructions.

## Report a bug

If you have an issue with the application, feel free to create an issue in the [Issues](https://github.com/namquang93/XboxGamingBar/issues) section. Please also attach the log to the issue so that I can fix it quickly. There is a wiki page on [how to get the log](https://github.com/namquang93/XboxGamingBar/wiki/Getting-The-Logs).

## Language

Xbox Gaming Bar is 100% free and open source. It's built upon C#.
Libraries used:
- LibreHardwareMonitor for performance statistics overlay.

- RyzenAdj for AMD TDP control.
