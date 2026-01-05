using NLog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using XboxGamingBarHelper.Core;
using XboxGamingBarHelper.Settings;
using static XboxGamingBarHelper.Input.XInput;
using Windows.UI.Input.Preview.Injection;

namespace XboxGamingBarHelper.Input
{
    internal class InputManager : Manager
    {
        private CancellationTokenSource cancellationTokenSource;
        private Task pollingTask;
        private InputInjector inputInjector;
        
        // State tracking for Controller 0
        private XInputState previousState;
        private bool wasConnected = false;

        // Static mapping for O(1) lookup
        // Note: Triggers (GamepadLeftTrigger, GamepadRightTrigger) are not included here 
        // because they are analog inputs in XInput and don't have corresponding bit flags.
        // They are handled explicitly in IsKeyPressed.
        private static readonly Dictionary<int, GamepadButtonFlags> ButtonMapping = new Dictionary<int, GamepadButtonFlags>
        {
            { (int)global::Windows.System.VirtualKey.GamepadA, GamepadButtonFlags.A },
            { (int)global::Windows.System.VirtualKey.GamepadB, GamepadButtonFlags.B },
            { (int)global::Windows.System.VirtualKey.GamepadX, GamepadButtonFlags.X },
            { (int)global::Windows.System.VirtualKey.GamepadY, GamepadButtonFlags.Y },
            { (int)global::Windows.System.VirtualKey.GamepadRightShoulder, GamepadButtonFlags.RightShoulder },
            { (int)global::Windows.System.VirtualKey.GamepadLeftShoulder, GamepadButtonFlags.LeftShoulder },
            { (int)global::Windows.System.VirtualKey.GamepadMenu, GamepadButtonFlags.Start },
            { (int)global::Windows.System.VirtualKey.GamepadView, GamepadButtonFlags.Back },
            { (int)global::Windows.System.VirtualKey.GamepadDPadUp, GamepadButtonFlags.DPadUp },
            { (int)global::Windows.System.VirtualKey.GamepadDPadDown, GamepadButtonFlags.DPadDown },
            { (int)global::Windows.System.VirtualKey.GamepadDPadLeft, GamepadButtonFlags.DPadLeft },
            { (int)global::Windows.System.VirtualKey.GamepadDPadRight, GamepadButtonFlags.DPadRight },
            { (int)global::Windows.System.VirtualKey.GamepadLeftThumbstickButton, GamepadButtonFlags.LeftThumb },
            { (int)global::Windows.System.VirtualKey.GamepadRightThumbstickButton, GamepadButtonFlags.RightThumb }
        };

        public InputManager(AppServiceConnection connection) : base(connection)
        {
            Connection = connection;
            inputInjector = InputInjector.TryCreate();
            cancellationTokenSource = new CancellationTokenSource();
            pollingTask = Task.Run(() => PollingLoop(cancellationTokenSource.Token));
        }

        private async Task PollingLoop(CancellationToken token)
        {
            Logger.Info("Started XInput polling loop for Controller 0.");
            
            while (!token.IsCancellationRequested)
            {
                XInputState currentState = new XInputState();
                // 0 = First Controller.
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
                                Logger.Debug($"Checking Lossless Scaling hotkey: [{string.Join(",", shortcutKeys)}]");

                                if (settings.IsListeningForKeyBinding.Value)
                                {
                                    // Skip detection if we are listening for key binding
                                    previousState = currentState;
                                    continue;
                                }

                                // Check if all keys in the shortcut are pressed
                                bool allKeysPressed = true;
                                foreach (int keyVal in shortcutKeys)
                                {
                                    if (!IsKeyPressed(currentState, keyVal))
                                    {
                                        allKeysPressed = false;
                                        break;
                                    }
                                }

                                if (allKeysPressed && shortcutKeys.Count >= 2)
                                {
                                    // Debounce: Check if they were NOT all pressed in the previous state
                                    bool wasPressedBefore = true;
                                    foreach (int keyVal in shortcutKeys)
                                    {
                                        if (!IsKeyPressed(previousState, keyVal))
                                        {
                                            wasPressedBefore = false;
                                            break;
                                        }
                                    }

                                    if (!wasPressedBefore)
                                    {
                                        Logger.Info($"Lossless Scaling Shortcut triggered! Keys: {string.Join("+", shortcutKeys)}");

                                        // Trigger the Lossless Scaling keyboard hotkey
                                        var keyboardKeys = LosslessScalingManager.GetHotkey();
                                        if (keyboardKeys.Count > 0)
                                        {
                                            Logger.Info($"Sending Keyboard Input: {string.Join("+", keyboardKeys)}");
                                            
                                            // Prepare injected input
                                            var injectedInputs = new List<InjectedInputKeyboardInfo>();
                                            
                                            // Key Downs
                                            foreach (var key in keyboardKeys)
                                            {
                                                injectedInputs.Add(new InjectedInputKeyboardInfo 
                                                { 
                                                    VirtualKey = key, 
                                                    KeyOptions = InjectedInputKeyOptions.None 
                                                });
                                            }

                                            // Key Ups (Reverse order)
                                            for (int i = keyboardKeys.Count - 1; i >= 0; i--)
                                            {
                                                injectedInputs.Add(new InjectedInputKeyboardInfo 
                                                { 
                                                    VirtualKey = keyboardKeys[i], 
                                                    KeyOptions = InjectedInputKeyOptions.KeyUp 
                                                });
                                            }

                                            // Inject
                                            if (inputInjector != null)
                                            {
                                                inputInjector.InjectKeyboardInput(injectedInputs);
                                            }
                                            else
                                            {
                                                Logger.Error("InputInjector is null, cannot inject hotkey.");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("No Lossless Scaling hotkey found or parsed.");
                                        }
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

        private bool IsKeyPressed(XInputState state, int keyVal)
        {
            // Triggers are analog values in XInput, mapped to VirtualKey by our logic
            if (keyVal == (int)global::Windows.System.VirtualKey.GamepadLeftTrigger)
            {
                return state.Gamepad.LeftTrigger > 30; // Threshold
            }
            if (keyVal == (int)global::Windows.System.VirtualKey.GamepadRightTrigger)
            {
                return state.Gamepad.RightTrigger > 30; // Threshold
            }

            if (ButtonMapping.TryGetValue(keyVal, out GamepadButtonFlags flag))
            {
                return (((GamepadButtonFlags)state.Gamepad.Buttons) & flag) == flag;
            }

            return false;
        }
    }
}
