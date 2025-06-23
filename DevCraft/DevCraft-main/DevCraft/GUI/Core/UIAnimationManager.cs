using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DevCraft.Configuration;

namespace DevCraft.GUI.Core
{
    /// <summary>
    /// Industry-standard UI animation system following patterns from AAA game studios
    /// Provides smooth transitions, easing functions, and performance-optimized animations
    /// </summary>
    public class UIAnimationManager : IDisposable
    {
        public enum EasingType
        {
            Linear,
            EaseInQuad,
            EaseOutQuad,
            EaseInOutQuad,
            EaseInCubic,
            EaseOutCubic,
            EaseInOutCubic,
            EaseInElastic,
            EaseOutElastic,
            EaseInBack,
            EaseOutBack
        }

        public class AnimationTarget
        {
            public Vector2 StartPosition;
            public Vector2 EndPosition;
            public Vector2 CurrentPosition;
            public float StartAlpha;
            public float EndAlpha;
            public float CurrentAlpha;
            public float StartScale;
            public float EndScale;
            public float CurrentScale;
            public TimeSpan Duration;
            public TimeSpan ElapsedTime;
            public EasingType Easing;
            public bool IsComplete;
            public Action OnComplete;

            public float Progress => Math.Min(1.0f, (float)(ElapsedTime.TotalMilliseconds / Duration.TotalMilliseconds));
        }

        private readonly Dictionary<string, AnimationTarget> activeAnimations = new();

        /// <summary>
        /// Start a position animation (slide in/out effects)
        /// </summary>
        public void AnimatePosition(string id, Vector2 startPos, Vector2 endPos, TimeSpan duration, 
            EasingType easing = EasingType.EaseOutQuad, Action onComplete = null)
        {
            activeAnimations[id] = new AnimationTarget
            {
                StartPosition = startPos,
                EndPosition = endPos,
                CurrentPosition = startPos,
                StartAlpha = 1.0f,
                EndAlpha = 1.0f,
                CurrentAlpha = 1.0f,
                StartScale = 1.0f,
                EndScale = 1.0f,
                CurrentScale = 1.0f,
                Duration = duration,
                ElapsedTime = TimeSpan.Zero,
                Easing = easing,
                OnComplete = onComplete
            };
        }

        /// <summary>
        /// Start a fade animation (alpha transitions)
        /// </summary>
        public void AnimateFade(string id, float startAlpha, float endAlpha, TimeSpan duration,
            EasingType easing = EasingType.EaseOutQuad, Action onComplete = null)
        {
            var existing = activeAnimations.ContainsKey(id) ? activeAnimations[id] : new AnimationTarget();
            
            activeAnimations[id] = new AnimationTarget
            {
                StartPosition = existing.CurrentPosition,
                EndPosition = existing.CurrentPosition,
                CurrentPosition = existing.CurrentPosition,
                StartAlpha = startAlpha,
                EndAlpha = endAlpha,
                CurrentAlpha = startAlpha,
                StartScale = existing.CurrentScale,
                EndScale = existing.CurrentScale,
                CurrentScale = existing.CurrentScale,
                Duration = duration,
                ElapsedTime = TimeSpan.Zero,
                Easing = easing,
                OnComplete = onComplete
            };
        }

        /// <summary>
        /// Start a scale animation (zoom effects)
        /// </summary>
        public void AnimateScale(string id, float startScale, float endScale, TimeSpan duration,
            EasingType easing = EasingType.EaseOutBack, Action onComplete = null)
        {
            var existing = activeAnimations.ContainsKey(id) ? activeAnimations[id] : new AnimationTarget();
            
            activeAnimations[id] = new AnimationTarget
            {
                StartPosition = existing.CurrentPosition,
                EndPosition = existing.CurrentPosition,
                CurrentPosition = existing.CurrentPosition,
                StartAlpha = existing.CurrentAlpha,
                EndAlpha = existing.CurrentAlpha,
                CurrentAlpha = existing.CurrentAlpha,
                StartScale = startScale,
                EndScale = endScale,
                CurrentScale = startScale,
                Duration = duration,
                ElapsedTime = TimeSpan.Zero,
                Easing = easing,
                OnComplete = onComplete
            };
        }

        /// <summary>
        /// Update all active animations
        /// </summary>
        public void Update(GameTime gameTime)
        {
            var completedAnimations = new List<string>();

            foreach (var kvp in activeAnimations)
            {
                var animation = kvp.Value;
                animation.ElapsedTime += gameTime.ElapsedGameTime;

                if (animation.Progress >= 1.0f)
                {
                    // Animation complete
                    animation.CurrentPosition = animation.EndPosition;
                    animation.CurrentAlpha = animation.EndAlpha;
                    animation.CurrentScale = animation.EndScale;
                    animation.IsComplete = true;
                    animation.OnComplete?.Invoke();
                    completedAnimations.Add(kvp.Key);
                }
                else
                {
                    // Apply easing function
                    float easedProgress = ApplyEasing(animation.Progress, animation.Easing);

                    // Interpolate values
                    animation.CurrentPosition = Vector2.Lerp(animation.StartPosition, animation.EndPosition, easedProgress);
                    animation.CurrentAlpha = MathHelper.Lerp(animation.StartAlpha, animation.EndAlpha, easedProgress);
                    animation.CurrentScale = MathHelper.Lerp(animation.StartScale, animation.EndScale, easedProgress);
                }
            }

            // Remove completed animations
            foreach (var id in completedAnimations)
            {
                activeAnimations.Remove(id);
            }
        }

        /// <summary>
        /// Get current animation state for an element
        /// </summary>
        public AnimationTarget GetAnimation(string id)
        {
            return activeAnimations.TryGetValue(id, out AnimationTarget animation) ? animation : null;
        }

        /// <summary>
        /// Stop an animation immediately
        /// </summary>
        public void StopAnimation(string id)
        {
            activeAnimations.Remove(id);
        }

        /// <summary>
        /// Check if an animation is currently running
        /// </summary>
        public bool IsAnimating(string id)
        {
            return activeAnimations.ContainsKey(id);
        }

        /// <summary>
        /// Apply easing functions for smooth animations
        /// </summary>
        private float ApplyEasing(float t, EasingType easing)
        {
            return easing switch
            {
                EasingType.Linear => t,
                EasingType.EaseInQuad => t * t,
                EasingType.EaseOutQuad => 1 - (1 - t) * (1 - t),
                EasingType.EaseInOutQuad => t < 0.5f ? 2 * t * t : 1 - (float)Math.Pow(-2 * t + 2, 2) / 2,
                EasingType.EaseInCubic => t * t * t,
                EasingType.EaseOutCubic => 1 - (float)Math.Pow(1 - t, 3),
                EasingType.EaseInOutCubic => t < 0.5f ? 4 * t * t * t : 1 - (float)Math.Pow(-2 * t + 2, 3) / 2,
                EasingType.EaseInElastic => t == 0 ? 0 : t == 1 ? 1 : -(float)Math.Pow(2, 10 * (t - 1)) * (float)Math.Sin((t - 1.1f) * 5 * Math.PI),
                EasingType.EaseOutElastic => t == 0 ? 0 : t == 1 ? 1 : (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t - 0.1f) * 5 * Math.PI) + 1,
                EasingType.EaseInBack => 2.70158f * t * t * t - 1.70158f * t * t,
                EasingType.EaseOutBack => 1 + 2.70158f * (float)Math.Pow(t - 1, 3) + 1.70158f * (float)Math.Pow(t - 1, 2),
                _ => t
            };
        }

        public void Dispose()
        {
            activeAnimations.Clear();
        }
    }
}
