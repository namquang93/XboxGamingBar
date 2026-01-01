using NLog;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Settings;
using static XboxGamingBarHelper.Input.XInput;

namespace XboxGamingBarHelper.Input
{
    internal class InputManager : Manager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private CancellationTokenSource cancellationTokenSource;
        private Task pollingTask;
        
        // State tracking for Controller 0
        private XInputState previousState;
        private bool wasConnected = false;

        public AppServiceConnection Connection { get; set; }

        public InputManager(AppServiceConnection connection) : base(connection)
        {
            Connection = connection;
            cancellationTokenSource = new CancellationTokenSource();
            pollingTask = Task.Run(() => PollingLoop(cancellationTokenSource.Token));
        }

        private async Task PollingLoop(CancellationToken token)
        {
            Logger.Info("Started XInput polling loop for Controller 0.");
            
            while (!token.IsCancellationRequested)
            {
                XInputState currentState = new XInputState();
                // 0 = First Controller. In a robust app you might check 0-3.
                int result = XInputGetState(0, ref currentState);

                if (result == 0) // ERROR_SUCCESS
                {
                    if (!wasConnected)
                    {
                        Logger.Info("XInput Controller 0 Connected.");
                        wasConnected = true;
                        previousState = currentState;
                    }
                    else
                    {
                        // Check if packet number changed (state changed)
                        if (currentState.PacketNumber != previousState.PacketNumber)
                        {
                            var settings = SettingsManager.GetInstance();
                            if (settings != null && settings.LosslessScalingShortcut != null && settings.LosslessScalingShortcut.Value != null)
                            {
                                List<int> shortcutKeys = settings.LosslessScalingShortcut.Value;
                                
                                // Map the current button state to our internal list of ints (assuming they map to GamepadButtonFlags)
                                // We check if the bitmask in currentState.Gamepad.Buttons contains ALL keys in the shortcut list
                                
                                bool allKeysPressed = true;
                                int currentButtons = currentState.Gamepad.Buttons;
                                
                                // Assuming the shortcut integers in settings map directly to the XInput button bitmask values
                                foreach (int key in shortcutKeys)
                                {
                                    if ((currentButtons & key) != key)
                                    {
                                        allKeysPressed = false;
                                        break;
                                    }
                                }

                                if (allKeysPressed && shortcutKeys.Count > 0)
                                {
                                    // Debounce: Check if they were NOT all pressed in the previous state to avoid repeat triggering
                                    bool wasPressedBefore = true;
                                    int previousButtons = previousState.Gamepad.Buttons;

                                    foreach (int key in shortcutKeys)
                                    {
                                        if ((previousButtons & key) != key)
                                        {
                                            wasPressedBefore = false;
                                            break;
                                        }
                                    }

                                    if (!wasPressedBefore)
                                    {
                                        Logger.Info($"Lossless Scaling Shortcut triggered! Keys: {string.Join("+", shortcutKeys)}");
                                    }
                                }
                            }

                            // Log any changes for debugging
                            if (currentState.Gamepad.Buttons != previousState.Gamepad.Buttons)
                            {
                                Logger.Info($"Gamepad 0 Buttons Changed: {(GamepadButtonFlags)currentState.Gamepad.Buttons}");
                            }
                            
                            previousState = currentState;
                        }
                    }
                }
                else
                {
                    if (wasConnected)
                    {
                        Logger.Info("XInput Controller 0 Disconnected.");
                        wasConnected = false;
                    }
                }

                await Task.Delay(16, token); // ~60Hz polling
            }
        }
    }
}
