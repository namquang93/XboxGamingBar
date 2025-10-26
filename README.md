# Xbox Gaming Bar

## What is it?

XBox Gaming Bar is a helper tool for gamers to control all gaming-related settings using the gamepad/game controller.
Xbox Gaming Bar is built as a Xbox Game Bar widget as the frontend, and a Win32 helper as the backend tool.
As of now, there are following functions:
1. Performance Overlay using RivaTuner Statistics Server OSD.
2. TDP Limit.
3. Per-game Profile.
4. CPU performance adjustments.
  a. Enable or disable CPU Boost.
  b. Set CPU Energy Performance Preference (EPP).
  c. Set CPU clock speed limit.

![alt text](Screenshots/v2_2.png)

## Installation

Please follow our [Wiki](https://github.com/namquang93/XboxGamingBar/wiki/Installation-Instruction) page for installation instruction.

## Language

Xbox Gaming Bar is 100% free and open source. It's built upon C#.
Libraries used:
- LibreHardwareMonitor for performance statistics overlay.
- RyzenAdj for AMD TDP control.