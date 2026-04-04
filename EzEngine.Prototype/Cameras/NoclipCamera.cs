using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EzEngine.ContentManagement.Mono.Interop;
using EzEngine.ContentManagement.Mono.Interop.Models;

namespace EzEngine.Prototype.Cameras;

public class NoclipCamera
{
    private Vector3 _to;
    private Vector3 _from;
    private Vector3 _up;
    private GraphicsDevice _graphicsDevice;
    public Matrix ViewMatrix { get; set; }
    public Matrix ProjectionMatrix { get; set; }
    public float AspectRatio { get; private set; }

    public double LocalXRotation { get; private set; }
    public double ZRotation { get; private set; }
    public Vector3 Motion { get; private set; }
    public double CurrentMaximumSpeed { get; private set; }
    public double MaximumSpeed { get; private set; }
    public double Acceleration { get; private set; }
    public double MouseSensitivity { get; private set; }

    public Point LastMousePosition { get; set; }

    public NoclipCamera(GraphicsDevice graphicsDevice)
    {
        _from = new Vector3(-2048.0F, 1024.0F, 0.0F);
        _to = new Vector3(0.0F, 1024.0F, 0.0F);
        _up = new Vector3(0.0F, 0.0F, 1.0F);
        _graphicsDevice = graphicsDevice;

        AspectRatio = (float)(1920.0D / 1080.0D);

        MouseSensitivity = 0.25D;
        ZRotation = 0.0D;
        LocalXRotation = 0.0D;
        Motion = new Vector3(0.0F, 0.0F, 0.0F);
        MaximumSpeed = 8.0D;
        CurrentMaximumSpeed = 8.0D;
        Acceleration = 1.0F;

        Mouse.SetPosition((int)(_graphicsDevice.Viewport.Width * 0.5), (int)(_graphicsDevice.Viewport.Height * 0.5));
    }

    public void Update(MouseState mouseState, KeyboardState keyboardState, ProcessedPolyOneFileVolumeSet[] volumeSets)
    {
        var newMousePosition = new Point(mouseState.X, mouseState.Y);

        var mousePositionDifferenceX = newMousePosition.X - LastMousePosition.X;
        var mousePositionDifferenceY = newMousePosition.Y - LastMousePosition.Y;

        LocalXRotation -= (mousePositionDifferenceY * MouseSensitivity) % 360.0D;

        ZRotation -= (mousePositionDifferenceX * MouseSensitivity) % 360.0D;
        var zRotationCos = Math.Cos(double.DegreesToRadians(ZRotation));
        var zRotationSin = Math.Sin(double.DegreesToRadians(ZRotation));
        var zPerpendicularCos = Math.Cos(double.DegreesToRadians(ZRotation - 90.0D));
        var zPerpendicularSin = Math.Sin(double.DegreesToRadians(ZRotation - 90.0D));
        var xRotationCos = Math.Cos(double.DegreesToRadians(LocalXRotation));
        var xPerpendicularCos = Math.Cos(double.DegreesToRadians(LocalXRotation - 90.0D));

        var localMotionSignX = 0;
        var localMotionSignY = 0;
        if (keyboardState.IsKeyDown(Keys.W))
        {
            localMotionSignX += 1;
        }
        else if (keyboardState.IsKeyDown(Keys.S))
        {
            localMotionSignX -= 1;
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            localMotionSignY += 1;
        }
        if (keyboardState.IsKeyDown(Keys.A))
        {
            localMotionSignY -= 1;
        }
        if (keyboardState.IsKeyDown(Keys.Space))
        {
            _from.Z += 4;
        }
        if (keyboardState.IsKeyDown(Keys.C))
        {
            _from.Z -= 4;
        }
        Motion = new Vector3(
            (float)(Motion.X + Acceleration * zRotationCos * localMotionSignX + Acceleration * zPerpendicularCos * localMotionSignY),
            (float)(Motion.Y + Acceleration * zRotationSin * localMotionSignX + Acceleration * zPerpendicularSin * localMotionSignY),
            0.0F
        );

        CurrentMaximumSpeed = MaximumSpeed;

        foreach (var volumeSet in volumeSets)
        {
            var collidedVolumeIndex = volumeSet.PointIsWithinAnyVolume(new Vector3(_from.X + Motion.X, _from.Y + Motion.Y, _from.Z + Motion.Z));

            if (collidedVolumeIndex is not null)
            {
                var lastCollidedEdgeVector = volumeSet.GetLastNonCollidedSide(_from, collidedVolumeIndex.Value);
                if (lastCollidedEdgeVector is not null)
                {
                    var edgeNormal = lastCollidedEdgeVector.Value / (float)Math.Sqrt(Helpers.DistanceSquared(lastCollidedEdgeVector.Value));
                    var motionVectorLength = (float)Math.Sqrt(Helpers.DistanceSquared(_to - _from));
                    var motionNormal = (_to - _from) / motionVectorLength;

                    var dotProduct = edgeNormal.X * motionNormal.X
                        + edgeNormal.Y * motionNormal.Y;
                    CurrentMaximumSpeed *= dotProduct;

                    Motion = new Vector3(
                        lastCollidedEdgeVector.Value.X,
                        lastCollidedEdgeVector.Value.Y,
                        Motion.Z);
                }
                else
                {
                    Motion = new Vector3(
                        0.0F,
                        0.0F,
                        Motion.Z);
                }
            }
        }

        //Clamp speed
        var motionVectorLengthSquared = Helpers.DistanceSquared(Motion);
        if (motionVectorLengthSquared > CurrentMaximumSpeed * CurrentMaximumSpeed)
        {
            var motionVectorLength = Math.Sqrt(motionVectorLengthSquared);
            Motion = new Vector3(
                (float)(Motion.X / (motionVectorLength / CurrentMaximumSpeed)),
                (float)(Motion.Y / (motionVectorLength / CurrentMaximumSpeed)),
                0.0F
            );
        }
        //Apply friction/speed reduction
        if (localMotionSignX == 0 && localMotionSignY == 0)
        {
            motionVectorLengthSquared = Helpers.DistanceSquared(Motion);
            if (motionVectorLengthSquared < Acceleration * Acceleration)
            {
                Motion = new Vector3(0.0F, 0.0F, 0.0F);
            }
            else if (motionVectorLengthSquared > 0.0D)
            {
                var motionVectorLength = Math.Sqrt(motionVectorLengthSquared);
                Motion = new Vector3(
                    (float)(Motion.X / (motionVectorLength / (motionVectorLength - Acceleration))),
                    (float)(Motion.Y / (motionVectorLength / (motionVectorLength - Acceleration))),
                    0.0F
                );
            }
        }

        _from.X += Motion.X;
        _from.Y += Motion.Y;

        _to.X = (float)(_from.X + zRotationCos * xPerpendicularCos);
        _to.Y = (float)(_from.Y + zRotationSin * xPerpendicularCos);
        _to.Z = (float)(_from.Z - xRotationCos);

        Mouse.SetPosition((int)(_graphicsDevice.Viewport.Width * 0.5), (int)(_graphicsDevice.Viewport.Height * 0.5));

        LastMousePosition = new Point((int)(_graphicsDevice.Viewport.Width * 0.5), (int)(_graphicsDevice.Viewport.Height * 0.5));

        ViewMatrix = Matrix.CreateLookAt(_from, _to, _up);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75.0F), AspectRatio, 1, 4095.0f);
    }
}