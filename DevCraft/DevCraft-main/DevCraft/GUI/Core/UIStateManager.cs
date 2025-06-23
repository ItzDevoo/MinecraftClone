using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DevCraft.GUI.Core
{
    /// <summary>
    /// Industry-standard UI state management following patterns from Unity, Unreal, and Mojang
    /// Provides robust state transitions, error handling, and performance optimization
    /// </summary>
    public class UIStateManager : IDisposable
    {
        public enum UIState
        {
            Hidden,
            MainMenu,
            NewWorld,
            LoadWorld,
            Settings,
            InGame,
            Inventory,
            Paused,
            Loading,
            Error
        }

        private UIState currentState = UIState.MainMenu;
        private UIState previousState = UIState.MainMenu;
        private readonly Stack<UIState> stateHistory = new();
        private readonly Dictionary<UIState, TimeSpan> stateEnterTimes = new();

        public UIState CurrentState => currentState;
        public UIState PreviousState => previousState;

        // Events for robust state management
        public event Action<UIState, UIState> OnStateChanged;
        public event Action<UIState> OnStateEntered;
        public event Action<UIState> OnStateExited;
        public event Action<string> OnError;

        /// <summary>
        /// Safely transition to a new state with validation and error handling
        /// </summary>
        public bool TryChangeState(UIState newState)
        {
            try
            {
                if (currentState == newState) return true;

                // Validate state transition
                if (!IsValidTransition(currentState, newState))
                {
                    OnError?.Invoke($"Invalid transition from {currentState} to {newState}");
                    return false;
                }

                // Record timing for performance monitoring
                stateEnterTimes[newState] = TimeSpan.FromMilliseconds(Environment.TickCount);

                // Exit current state
                OnStateExited?.Invoke(currentState);

                previousState = currentState;
                stateHistory.Push(currentState);
                currentState = newState;

                // Enter new state
                OnStateEntered?.Invoke(currentState);
                OnStateChanged?.Invoke(previousState, currentState);

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"State transition error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Legacy method for backward compatibility
        /// </summary>
        public void ChangeState(UIState newState)
        {
            TryChangeState(newState);
        }

        /// <summary>
        /// Return to previous state with validation
        /// </summary>
        public bool PopState()
        {
            if (stateHistory.Count > 0)
            {
                var targetState = stateHistory.Pop();
                return TryChangeState(targetState);
            }
            return false;
        }

        /// <summary>
        /// Validate if state transition is allowed (prevents UI bugs)
        /// </summary>
        private bool IsValidTransition(UIState from, UIState to)
        {
            // Never allow transitions to/from Error state except to MainMenu
            if (from == UIState.Error && to != UIState.MainMenu) return false;
            
            // Loading state can only transition to specific states
            if (from == UIState.Loading && to != UIState.InGame && to != UIState.MainMenu && to != UIState.Error) 
                return false;

            // In-game states
            if (from == UIState.InGame && (to == UIState.NewWorld || to == UIState.LoadWorld)) 
                return false;

            return true;
        }

        /// <summary>
        /// Check if currently in any menu state
        /// </summary>
        public bool IsInMenuState() => currentState != UIState.InGame && currentState != UIState.Hidden;

        /// <summary>
        /// Check if UI should block game input
        /// </summary>
        public bool ShouldBlockGameInput() => currentState != UIState.InGame && currentState != UIState.Hidden;

        /// <summary>
        /// Get how long we've been in current state (for animations/timeouts)
        /// </summary>
        public TimeSpan GetTimeInCurrentState()
        {
            if (stateEnterTimes.TryGetValue(currentState, out TimeSpan enterTime))
            {
                return TimeSpan.FromMilliseconds(Environment.TickCount) - enterTime;
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Emergency reset to main menu (for error recovery)
        /// </summary>
        public void EmergencyReset()
        {
            stateHistory.Clear();
            previousState = currentState;
            currentState = UIState.MainMenu;
            OnStateChanged?.Invoke(previousState, currentState);
        }

        public void Dispose()
        {
            OnStateChanged = null;
            OnStateEntered = null;
            OnStateExited = null;
            OnError = null;            stateHistory.Clear();
            stateEnterTimes.Clear();
        }

        /// <summary>
        /// Check if we can navigate back to previous state
        /// </summary>
        public bool CanGoBack() => stateHistory.Count > 0;
    }
}
