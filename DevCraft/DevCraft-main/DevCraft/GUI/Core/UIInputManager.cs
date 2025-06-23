using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DevCraft.GUI.Core
{
    /// <summary>
    /// Industry-standard input management system following patterns from Unity and Unreal Engine
    /// Provides reliable input handling, debouncing, and multi-context support
    /// </summary>
    public class UIInputManager : IDisposable
    {
        public enum InputContext
        {
            Game,
            Menu,
            Inventory,
            Dialog,
            Debug
        }

        private InputContext currentContext = InputContext.Game;
        private readonly Dictionary<InputContext, bool> contextBlocking = new();
        
        // Input state tracking
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;
        private MouseState currentMouseState;
        private MouseState previousMouseState;
        
        // Debouncing and repeat prevention
        private readonly Dictionary<Keys, DateTime> keyPressTimestamps = new();
        private readonly Dictionary<Keys, bool> keyHeldStates = new();
        private readonly TimeSpan debounceThreshold = TimeSpan.FromMilliseconds(50);
        private readonly TimeSpan holdThreshold = TimeSpan.FromMilliseconds(500);
        
        // Mouse tracking
        private Point previousMousePosition;
        private bool isDragging;
        private Point dragStartPosition;
        private readonly float dragThreshold = 5.0f;
        
        public InputContext CurrentContext => currentContext;
        public Point MousePosition => currentMouseState.Position;
        public Point MouseDelta => currentMouseState.Position - previousMousePosition;
        public bool IsDragging => isDragging;
        public Point DragStartPosition => dragStartPosition;
        
        // Events for UI components
        public event Action<Keys> OnKeyPressed;
        public event Action<Keys> OnKeyReleased;
        public event Action<Keys> OnKeyHeld;
        public event Action<Point> OnMouseMove;
        public event Action<Point> OnLeftClick;
        public event Action<Point> OnRightClick;
        public event Action<Point> OnMiddleClick;
        public event Action<Point, Point> OnDragStart;
        public event Action<Point, Point> OnDragUpdate;
        public event Action<Point, Point> OnDragEnd;
        public event Action<int> OnScrollWheelMove;

        public UIInputManager()
        {
            // Initialize context blocking states
            contextBlocking[InputContext.Game] = false;
            contextBlocking[InputContext.Menu] = true;
            contextBlocking[InputContext.Inventory] = true;
            contextBlocking[InputContext.Dialog] = true;
            contextBlocking[InputContext.Debug] = false;
        }

        /// <summary>
        /// Update input states and process events
        /// </summary>
        public void Update()
        {
            // Store previous states
            previousKeyboardState = currentKeyboardState;
            previousMouseState = currentMouseState;
            previousMousePosition = currentMouseState.Position;
            
            // Get current states
            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();
            
            ProcessKeyboardInput();
            ProcessMouseInput();
        }

        /// <summary>
        /// Set the current input context (affects which inputs are processed)
        /// </summary>
        public void SetContext(InputContext context)
        {
            if (currentContext != context)
            {
                currentContext = context;
                // Clear held states when switching contexts
                keyHeldStates.Clear();
            }
        }

        /// <summary>
        /// Check if game input should be blocked by UI
        /// </summary>
        public bool ShouldBlockGameInput()
        {
            return contextBlocking.GetValueOrDefault(currentContext, false);
        }

        /// <summary>
        /// Check if a key was just pressed this frame
        /// </summary>
        public bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if a key was just released this frame
        /// </summary>
        public bool IsKeyReleased(Keys key)
        {
            return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if a key is currently held down (with debouncing)
        /// </summary>
        public bool IsKeyHeld(Keys key)
        {
            return keyHeldStates.GetValueOrDefault(key, false);
        }

        /// <summary>
        /// Check if left mouse button was just clicked
        /// </summary>
        public bool IsLeftMousePressed()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed && 
                   previousMouseState.LeftButton == ButtonState.Released;
        }

        /// <summary>
        /// Check if right mouse button was just clicked
        /// </summary>
        public bool IsRightMousePressed()
        {
            return currentMouseState.RightButton == ButtonState.Pressed && 
                   previousMouseState.RightButton == ButtonState.Released;
        }

        /// <summary>
        /// Check if middle mouse button was just clicked
        /// </summary>
        public bool IsMiddleMousePressed()
        {
            return currentMouseState.MiddleButton == ButtonState.Pressed && 
                   previousMouseState.MiddleButton == ButtonState.Released;
        }

        /// <summary>
        /// Get scroll wheel delta
        /// </summary>
        public int GetScrollWheelDelta()
        {
            return currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
        }

        /// <summary>
        /// Process keyboard input with debouncing and repeat detection
        /// </summary>
        private void ProcessKeyboardInput()
        {
            var pressedKeys = currentKeyboardState.GetPressedKeys();
            var previousPressedKeys = previousKeyboardState.GetPressedKeys();
            
            DateTime currentTime = DateTime.Now;
            
            // Check for newly pressed keys
            foreach (var key in pressedKeys)
            {
                if (!previousKeyboardState.IsKeyDown(key))
                {
                    // Key just pressed
                    if (!keyPressTimestamps.ContainsKey(key) || 
                        currentTime - keyPressTimestamps[key] > debounceThreshold)
                    {
                        keyPressTimestamps[key] = currentTime;
                        OnKeyPressed?.Invoke(key);
                    }
                }
                else
                {
                    // Key held down
                    if (keyPressTimestamps.TryGetValue(key, out DateTime pressTime) &&
                        currentTime - pressTime > holdThreshold)
                    {
                        if (!keyHeldStates.GetValueOrDefault(key, false))
                        {
                            keyHeldStates[key] = true;
                            OnKeyHeld?.Invoke(key);
                        }
                    }
                }
            }
            
            // Check for released keys
            foreach (var key in previousPressedKeys)
            {
                if (!currentKeyboardState.IsKeyDown(key))
                {
                    keyHeldStates[key] = false;
                    OnKeyReleased?.Invoke(key);
                }
            }
        }

        /// <summary>
        /// Process mouse input with drag detection
        /// </summary>
        private void ProcessMouseInput()
        {
            // Mouse movement
            if (MouseDelta != Point.Zero)
            {
                OnMouseMove?.Invoke(MousePosition);
            }
            
            // Mouse button presses
            if (IsLeftMousePressed())
            {
                OnLeftClick?.Invoke(MousePosition);
                
                // Start potential drag
                if (!isDragging)
                {
                    dragStartPosition = MousePosition;
                }
            }
            
            if (IsRightMousePressed())
            {
                OnRightClick?.Invoke(MousePosition);
            }
            
            if (IsMiddleMousePressed())
            {
                OnMiddleClick?.Invoke(MousePosition);
            }
            
            // Drag handling
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                float distance = Vector2.Distance(MousePosition.ToVector2(), dragStartPosition.ToVector2());
                
                if (!isDragging && distance > dragThreshold)
                {
                    isDragging = true;
                    OnDragStart?.Invoke(dragStartPosition, MousePosition);
                }
                else if (isDragging)
                {
                    OnDragUpdate?.Invoke(dragStartPosition, MousePosition);
                }
            }
            else if (isDragging)
            {
                isDragging = false;
                OnDragEnd?.Invoke(dragStartPosition, MousePosition);
            }
            
            // Scroll wheel
            int scrollDelta = GetScrollWheelDelta();
            if (scrollDelta != 0)
            {
                OnScrollWheelMove?.Invoke(scrollDelta);
            }
        }

        /// <summary>
        /// Clear all input state (useful for context switches)
        /// </summary>
        public void ClearInputState()
        {
            keyPressTimestamps.Clear();
            keyHeldStates.Clear();
            isDragging = false;
        }

        public void Dispose()
        {
            OnKeyPressed = null;
            OnKeyReleased = null;
            OnKeyHeld = null;
            OnMouseMove = null;
            OnLeftClick = null;
            OnRightClick = null;
            OnMiddleClick = null;
            OnDragStart = null;
            OnDragUpdate = null;
            OnDragEnd = null;
            OnScrollWheelMove = null;
            
            ClearInputState();
        }
    }
}
