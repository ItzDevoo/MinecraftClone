using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DevCraft
{
    public class Camera
    {
        public bool UpdateOccured => cameraDelta.Length() > 0;

        public Matrix View { get; set; }
        public Matrix Projection { get; set; }

        public Vector3 Direction { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 HorizontalDirection;

        public BoundingFrustum Frustum { get; set; }

        Vector3 target;

        Vector2 cameraDelta;
        Vector2 screenCenter;

        MouseState previousMouseState;

        readonly float rotationSpeed;


        public Camera(GraphicsDevice graphics, Vector3 position, Vector3 target)
        {
            this.target = target;

            Position = position;
            Direction = target - position;
            Direction = Vector3.Normalize(Direction);

            HorizontalDirection = new Vector3(Direction.X, 0f, Direction.Z);
            HorizontalDirection.Normalize();

            rotationSpeed = 2.2f; // Slightly reduced for smoother camera movement

            View = Matrix.CreateLookAt(position, target, Vector3.Up);

            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70), 
                (float)graphics.Viewport.Width / graphics.Viewport.Height, 0.1f, 200);

            screenCenter = new Vector2(graphics.Viewport.Width / 2,
                                       graphics.Viewport.Height / 2);
            Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);

            previousMouseState = Mouse.GetState();

            Frustum = new BoundingFrustum(View * Projection);
        }

        public void Update(GameTime gameTime)
        {
            MouseState currentMouseState = Mouse.GetState();

            float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 20f;
            cameraDelta = delta * (new Vector2(currentMouseState.X, currentMouseState.Y) -
                new Vector2(previousMouseState.X, previousMouseState.Y));
            
            // Improved mouse sensitivity and clamping for smoother camera movement
            cameraDelta = Vector2.Clamp(cameraDelta, new Vector2(-15, -15), new Vector2(15, 15));
            cameraDelta *= rotationSpeed;

            // Prevent camera from flipping at extreme angles
            if (Math.Abs(Direction.Y) > 0.99f && (Math.Sign(cameraDelta.Y) != Math.Sign(Direction.Y)))
            {
                cameraDelta.Y = 0;
            }

            // Apply camera rotation with improved sensitivity
            Direction = Vector3.Transform(Direction,
                Matrix.CreateFromAxisAngle(Vector3.Up, (-MathHelper.PiOver4 / 140) * cameraDelta.X));
            Direction = Vector3.Transform(Direction,
                Matrix.CreateFromAxisAngle(Vector3.Cross(Vector3.Up, Direction), (MathHelper.PiOver4 / 90) * cameraDelta.Y));

            Direction = Vector3.Normalize(Direction);

            HorizontalDirection.X = Direction.X;
            HorizontalDirection.Z = Direction.Z;
            HorizontalDirection.Normalize();

            target = Direction + Position;

            // Reset mouse position when it gets too far from center
            if ((new Vector2(currentMouseState.X, currentMouseState.Y) - screenCenter).Length() > 150)
            {
                Mouse.SetPosition((int)screenCenter.X, (int)screenCenter.Y);
                currentMouseState = Mouse.GetState();
            }

            previousMouseState = currentMouseState;

            View = Matrix.CreateLookAt(Position, target, Vector3.Up);

            Frustum.Matrix = View * Projection;
        }
    }
}
