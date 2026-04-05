using EzEngine.ContentManagement.Mono.Interop;
using EzEngine.ContentManagement.Mono.Interop.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EzEngine.Prototype.Cameras;

public class FirstPersonShooterCamera
{
    private Vector3 _from;
    private Vector3 _to;
    private Vector3 _up;
    private float _fov;
    private float _aspectRatio;
    private float _farClip;
    private float _characterEyeLevel;

    public Matrix ViewMatrix { get; private set; }
    public Matrix ProjectionMatrix { get; private set; }

    private GraphicsDevice _graphicsDevice;

    private double _localXRotation;
    private double _zRotation;
    private Vector3 _motion;
    private double _currentMaximumSpeed;
    private double _maximumSpeed;
    private double _acceleration;
    private float _jumpSpeed;
    private float _maximumFallingSpeed;
    private float _gravity;
    private float _floorZPrevious;
    private float _floorZ;
    private double _mouseSensitivity;

    private Point _lastMousePosition;

    public FirstPersonShooterCamera(GraphicsDevice graphicsDevice)
    {
        _from = new Vector3(-1792.0F, 1024.0F, 0.0F);
        _to = new Vector3(0.0F, 1024.0F, 0.0F);
        _up = new Vector3(0.0F, 0.0F, 1.0F);
        _graphicsDevice = graphicsDevice;

        _fov = MathHelper.ToRadians(75.0F);
        _aspectRatio = (float)(1920.0D / 1080.0D);
        _farClip = 8191.0F;
        _characterEyeLevel = 67.0F;

        _mouseSensitivity = 0.25D;
        _zRotation = 0.0D;
        _localXRotation = 0.0D;
        _motion = new Vector3(0.0F, 0.0F, 0.0F);
        _maximumSpeed = 8.0D;
        _currentMaximumSpeed = 8.0D;
        _acceleration = 1.0F;
        _maximumFallingSpeed = 16.0F;
        _gravity = 0.4F;
        _jumpSpeed = 8.0F;
        _floorZ = 0.0F;

        Mouse.SetPosition((int)(_graphicsDevice.Viewport.Width * 0.5D), (int)(_graphicsDevice.Viewport.Height * 0.5D));
    }

    public void Update(MouseState mouseState, KeyboardState keyboardState, ProcessedPolyOneFileVolumeSet[] volumeSets)
    {
        var newMousePosition = new Point(mouseState.X, mouseState.Y);

        var mousePositionDifferenceX = mouseState.X - _lastMousePosition.X;
        var mousePositionDifferenceY = mouseState.Y - _lastMousePosition.Y;

        _localXRotation -= (mousePositionDifferenceY * _mouseSensitivity) % 360.0D;
        _zRotation -= (mousePositionDifferenceX * _mouseSensitivity) % 360.0D;

        var zRotationCos = Math.Cos(double.DegreesToRadians(_zRotation));
        var zRotationSin = Math.Sin(double.DegreesToRadians(_zRotation));
        var zPerpendicularCos = Math.Cos(double.DegreesToRadians(_zRotation - 90.0D));
        var zPerpendicularSin = Math.Sin(double.DegreesToRadians(_zRotation - 90.0D));
        var xRotationCos = Math.Cos(double.DegreesToRadians(_localXRotation));
        var xPerpendicularCos = Math.Cos(double.DegreesToRadians(_localXRotation - 90.0D));

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
        _motion = new Vector3(
            (float)(_motion.X + _acceleration * zRotationCos * localMotionSignX + _acceleration * zPerpendicularCos * localMotionSignY),
            (float)(_motion.Y + _acceleration * zRotationSin * localMotionSignX + _acceleration * zPerpendicularSin * localMotionSignY),
            _motion.Z
        );

        _currentMaximumSpeed = _maximumSpeed;
        _floorZPrevious = _floorZ;
        _floorZ = -1024.0F;

        CollisionCheck(volumeSets);

        //Clamp speed
        var motionVectorLengthSquared = Helpers.DistanceSquared(_motion.X, _motion.Y);
        if (motionVectorLengthSquared > _currentMaximumSpeed * _currentMaximumSpeed)
        {
            var motionVectorLength = Math.Sqrt(motionVectorLengthSquared);
            _motion.X = (float)(_motion.X / (motionVectorLength / _currentMaximumSpeed));
            _motion.Y = (float)(_motion.Y / (motionVectorLength / _currentMaximumSpeed));
        }
        //Apply friction/speed reduction
        if (localMotionSignX == 0 && localMotionSignY == 0)
        {
            motionVectorLengthSquared = Helpers.DistanceSquared(_motion.X, _motion.Y);
            if (motionVectorLengthSquared < _acceleration * _acceleration)
            {
                _motion.X = 0.0F;
                _motion.Y = 0.0F;
            }
            else if (motionVectorLengthSquared > 0.0D)
            {
                var motionVectorLength = Math.Sqrt(motionVectorLengthSquared);
                _motion.X = (float)(_motion.X / (motionVectorLength / (motionVectorLength - _acceleration)));
                _motion.Y = (float)(_motion.Y / (motionVectorLength / (motionVectorLength - _acceleration)));
            }
        }

        if (_from.Z > _floorZ && _motion.Z > -_maximumFallingSpeed)
        {
            _motion.Z -= _gravity;
        }
        if (keyboardState.IsKeyDown(Keys.Space) && _from.Z == _floorZ)
        {
            _motion.Z = _jumpSpeed;
        }

        _from.X += _motion.X;
        _from.Y += _motion.Y;
        _from.Z += _motion.Z;

        _to.X = (float)(_from.X + zRotationCos * xPerpendicularCos);
        _to.Y = (float)(_from.Y + zRotationSin * xPerpendicularCos);
        _to.Z = (float)(_from.Z - xRotationCos);

        Mouse.SetPosition((int)(_graphicsDevice.Viewport.Width * 0.5), (int)(_graphicsDevice.Viewport.Height * 0.5));

        _lastMousePosition = new Point((int)(_graphicsDevice.Viewport.Width * 0.5), (int)(_graphicsDevice.Viewport.Height * 0.5));

        var viewPosition = new Vector3(0.0F, 0.0F, _characterEyeLevel);
        ViewMatrix = Matrix.CreateLookAt(_from + viewPosition, _to + viewPosition, _up);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(_fov, _aspectRatio, 1, _farClip);
    }

    private void CollisionCheck(ProcessedPolyOneFileVolumeSet[] volumeSets)
    {
        for (var i = 0; i < volumeSets.Length; i++)
        {
            var collidedVolumeIndex = volumeSets[i].PointIsWithinAnyVolume2D(_from.X + _motion.X, _from.Y + _motion.Y);
            if (collidedVolumeIndex is not null)
            {
                _floorZ = ProcessedPolyOneFileVolumeSet.GetZPlanarIntersection(
                    volumeSets[i].UpperVertices[collidedVolumeIndex.Value * 3],
                    volumeSets[i].UpperVerticesSurfaceNormals[collidedVolumeIndex.Value * 3],
                    _from + _motion);

                //Snapping for stairs
                var distanceToFloor = _floorZ - (_from.Z + _motion.Z);
                if (/*_from.Z == _floorZPrevious &&*/ distanceToFloor < 8.0F && distanceToFloor > -8.0F)
                {
                    _from.Z = _floorZ;
                    _motion.Z = 0.0F;
                }

                if (volumeSets[i].AxisAlignedBoundingBoxes[collidedVolumeIndex.Value].PointIsWithinZ(_from.Z + _motion.Z)
                    && _from.Z < _floorZ)
                {
                    var lastCollidedEdgeVector = volumeSets[i].GetLastNonCollidedSide(_from, collidedVolumeIndex.Value);
                    if (lastCollidedEdgeVector is not null)
                    {
                        var edgeNormal = lastCollidedEdgeVector.Value / (float)Math.Sqrt(Helpers.DistanceSquared(lastCollidedEdgeVector.Value));
                        var motionVectorLength = (float)Math.Sqrt(Helpers.DistanceSquared(_to - _from));
                        var motionNormal = (_to - _from) / motionVectorLength;

                        var dotProduct = edgeNormal.X * motionNormal.X
                            + edgeNormal.Y * motionNormal.Y;
                        _currentMaximumSpeed *= dotProduct;

                        _motion.X = lastCollidedEdgeVector.Value.X;
                        _motion.Y = lastCollidedEdgeVector.Value.Y;
                    }
                    else
                    {
                        _motion.X = 0.0F;
                        _motion.Y = 0.0F;
                    }
                }
            }
        }
    }
}