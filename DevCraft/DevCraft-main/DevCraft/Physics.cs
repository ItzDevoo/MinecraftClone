using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using DevCraft.Utilities;

namespace DevCraft
{
    public class Physics(Player player)
    {
        public bool Moving => Velocity.Length() > 0;
        public Vector3 Velocity;

        readonly Player player = player;
        
        // Movement smoothing
        private Vector2 inputVelocity = Vector2.Zero;
        private Vector2 targetInputVelocity = Vector2.Zero;

        public void ResolveCollision(BoundingBox blockBox)
        {
            int iterations = 0;
            const int maxIterations = 5;

            // Loop until the player's bounding box no longer intersects the block
            // or until the iteration limit is reached
            while (player.Bound.Intersects(blockBox) && iterations < maxIterations)
            {
                var playerBox = player.Bound;

                // Compute penetration (overlap) amounts along each axis
                float overlapX = Math.Min(playerBox.Max.X, blockBox.Max.X) - Math.Max(playerBox.Min.X, blockBox.Min.X);
                float overlapY = Math.Min(playerBox.Max.Y, blockBox.Max.Y) - Math.Max(playerBox.Min.Y, blockBox.Min.Y);
                float overlapZ = Math.Min(playerBox.Max.Z, blockBox.Max.Z) - Math.Max(playerBox.Min.Z, blockBox.Min.Z);

                // Choose the axis with the smallest overlap to resolve first
                float minOverlap = overlapX;
                AxisDirection side = AxisDirection.X;
                if (overlapY < minOverlap)
                {
                    minOverlap = overlapY;
                    side = AxisDirection.Y;
                }
                if (overlapZ < minOverlap)
                {
                    side = AxisDirection.Z;
                }

                Vector3 playerCenter = (playerBox.Min + playerBox.Max) * 0.5f;
                Vector3 blockCenter = (blockBox.Min + blockBox.Max) * 0.5f;

                // Resolve collision along the axis with momentum preservation
                if (side == AxisDirection.X)
                {
                    int dir = Math.Sign(playerCenter.X - blockCenter.X);
                    player.Position.X += dir * (overlapX);
                    
                    // Only stop X movement if moving into the wall
                    if (Math.Sign(Velocity.X) != dir)
                    {
                        Velocity.X = 0;
                    }
                    player.Sprinting = false;
                }
                else if (side == AxisDirection.Y)
                {
                    int dir = Math.Sign(playerCenter.Y - blockCenter.Y);

                    if (dir > 0) // Floor collision
                    {
                        player.Position.Y += overlapY;
                        player.Walking = true;
                        player.Flying = false;
                    }
                    else // Ceiling collision
                    {
                        player.Position.Y -= overlapY;
                    }
                    
                    // Only stop Y movement if moving into the surface
                    if (Math.Sign(Velocity.Y) != dir)
                    {
                        Velocity.Y = 0;
                    }
                }
                else // side == AxisDirection.Z
                {
                    int dir = Math.Sign(playerCenter.Z - blockCenter.Z);
                    player.Position.Z += dir * overlapZ;
                    
                    // Only stop Z movement if moving into the wall  
                    if (Math.Sign(Velocity.Z) != dir)
                    {
                        Velocity.Z = 0;
                    }
                    player.Sprinting = false;
                }

                // Update the player's bounding box after each correction.
                player.UpdateBound();
                player.UpdateCamera();
                iterations++;
            }
        }        public void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            // Improved movement constants for better feel
            const float walkSpeed = 0.065f;      // Base walking speed
            const float sprintMultiplier = 1.3f; // Sprint speed multiplier  
            const float flySpeed = 0.085f;       // Flying speed
            const float gravityAcceleration = 9.8f;
            const float walkAcceleration = 0.025f;   // Ground acceleration
            const float airAcceleration = 0.015f;    // Air acceleration  
            const float groundFriction = 0.6f;       // Ground friction
            const float airFriction = 0.15f;         // Air friction
            const float inputSmoothingGround = 0.35f; // Ground input smoothing
            const float inputSmoothingAir = 0.2f;     // Air input smoothing
            const float eps = 1e-4f;
            const float terminalVelocity = 1.2f;

            float friction = player.Flying ? airFriction : (player.Walking ? groundFriction : airFriction);
            float acceleration = player.Flying ? walkAcceleration : (player.Walking ? walkAcceleration : airAcceleration);
            float currentMaxSpeed = walkSpeed;
            
            // Apply sprint multiplier
            if (player.Sprinting && player.Walking && !player.Flying)
            {
                currentMaxSpeed *= sprintMultiplier;
            }
            
            // Use fly speed when flying
            if (player.Flying)
            {
                currentMaxSpeed = flySpeed;
            }

            double elapsedTime = gameTime.ElapsedGameTime.TotalMilliseconds;
            float delta = (float)elapsedTime / 20f;

            // Apply friction to Y velocity when flying
            if (player.Flying && Math.Abs(Velocity.Y) > eps)
            {
                Velocity.Y -= Math.Sign(Velocity.Y) * delta * friction * acceleration;
            }
            else if (player.Flying)
            {
                Velocity.Y = 0;
            }

            // Apply gravity when not flying
            if (!player.Flying)
            {
                if (Velocity.Y > -terminalVelocity)
                {
                    Velocity.Y -= (delta * gravityAcceleration) / 800f;
                }
            }

            // Get input for movement
            Vector2 inputDir = Vector2.Zero;
            if (ks.IsKeyDown(Keys.W)) inputDir.Y += 1f;
            if (ks.IsKeyDown(Keys.S)) inputDir.Y -= 1f;
            if (ks.IsKeyDown(Keys.A)) inputDir.X -= 1f;
            if (ks.IsKeyDown(Keys.D)) inputDir.X += 1f;
            
            // Normalize diagonal movement
            if (inputDir.Length() > 1f)
                inputDir.Normalize();

            // Smooth input for better feel
            float inputSmoothing = player.Walking ? inputSmoothingGround : inputSmoothingAir;
            targetInputVelocity = inputDir;
            inputVelocity = Vector2.Lerp(inputVelocity, targetInputVelocity, inputSmoothing);

            // Calculate desired horizontal velocity based on camera direction
            Vector3 forward = Vector3.Normalize(new Vector3(player.Camera.Direction.X, 0, player.Camera.Direction.Z));
            Vector3 right = Vector3.Cross(forward, Vector3.Up);  // Fixed: swapped parameters to get correct right vector
            
            Vector3 desiredVelocity = (forward * inputVelocity.Y + right * inputVelocity.X) * currentMaxSpeed;
            
            // Apply horizontal movement
            Vector3 horizontalVelocity = new Vector3(Velocity.X, 0, Velocity.Z);
            Vector3 velocityDiff = desiredVelocity - horizontalVelocity;
            
            // Apply acceleration towards desired velocity
            float accelFactor = delta * acceleration;
            Vector3 accelVector = velocityDiff * accelFactor;
            
            // Limit acceleration magnitude for realistic feel
            if (accelVector.Length() > acceleration * delta)
            {
                accelVector = Vector3.Normalize(accelVector) * acceleration * delta;
            }
            
            Velocity.X += accelVector.X;
            Velocity.Z += accelVector.Z;

            // Apply friction when no input
            if (inputDir.Length() < eps)
            {
                if (Math.Abs(Velocity.X) > eps)
                {
                    Velocity.X -= Math.Sign(Velocity.X) * delta * friction * acceleration;
                    if (Math.Abs(Velocity.X) < eps) Velocity.X = 0;
                }
                
                if (Math.Abs(Velocity.Z) > eps)
                {
                    Velocity.Z -= Math.Sign(Velocity.Z) * delta * friction * acceleration;
                    if (Math.Abs(Velocity.Z) < eps) Velocity.Z = 0;
                }
            }

            // Vertical movement when flying
            if (player.Flying)
            {
                if (ks.IsKeyDown(Keys.Space))
                {
                    if (Math.Abs(Velocity.Y) < 2 * currentMaxSpeed)
                    {
                        Velocity.Y += delta * acceleration;
                    }
                    else
                    {
                        Velocity.Y = 2 * currentMaxSpeed;
                    }
                }

                if (ks.IsKeyDown(Keys.LeftShift))
                {
                    if (Math.Abs(Velocity.Y) < 2 * currentMaxSpeed)
                    {
                        Velocity.Y -= delta * acceleration;
                    }
                    else
                    {
                        Velocity.Y = -2 * currentMaxSpeed;
                    }
                }
            }

            // Calculate position delta and update position
            Vector3 positionDelta = Velocity * delta;
            player.Position += positionDelta;

            player.UpdateBound();
        }
    }
}
