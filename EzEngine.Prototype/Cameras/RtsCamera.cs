using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EzEngine.Prototype.Cameras;

public class RtsCamera
{
    private Vector3 _to;
    private Vector3 _from;
    private Vector3 _up;
    public Matrix ViewMatrix { get; set; }
    public Matrix ProjectionMatrix { get; set; }
    public float AspectRatio { get; private set; }

    private int _currentZoomLevel;
    private float _currentZoomDistance;
    private float _zoomDistanceDesired;
    private float[] _zoomDistances;
    private readonly int _zoomLevelCount = 100;

    private Vector3 _desiredPosition;
    private Vector3 _lagPosition;
    private Vector3 _positionDifference;

    private int _lastScrollWheelValue;
    private Point _panStartWindowPosition;
    private Point _panDifference;
    private Vector2 _panStartCameraPosition;
    private ButtonState _lastLeftMouseButtonState = ButtonState.Released;

    public RtsCamera()
    {
        _currentZoomLevel = 30;
        _zoomDistances = new float[_zoomLevelCount];
        _zoomDistances[0] = 1.0F;
        for (int i = 1; i < _zoomLevelCount; i++)
        {
            _zoomDistances[i] = _zoomDistances[i - 1] * 1.125F;
        }
        _currentZoomDistance = _zoomDistances[_currentZoomLevel];
        _zoomDistanceDesired = _zoomDistances[_currentZoomLevel];

        _desiredPosition = new Vector3(0.0F, 30.0F + _zoomDistances[_currentZoomLevel], _zoomDistances[_currentZoomLevel]);
        _lagPosition = new Vector3(_desiredPosition.X, _desiredPosition.Y, _desiredPosition.Z);
        _positionDifference = new Vector3(0.0F, 0.0F, 0.0F);

        _from = new Vector3(0.0F, 0.0F, 0.0F);
        _to = new Vector3(0.0F, 0.0F, 0.0F);
        _up = Vector3.Normalize(new Vector3(0.0F, -1.0F, 1.0F));

        _lastScrollWheelValue = 0;

        AspectRatio = (float)(1920.0D / 1080.0D);

        _panStartWindowPosition = new Point();
        _panDifference = new Point();
        _panStartCameraPosition = new Vector2();
    }

    public void Update(MouseState mouseState)
    {
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            if (_lastLeftMouseButtonState == ButtonState.Released)
            {
                _panStartWindowPosition.X = mouseState.X;
                _panStartWindowPosition.Y = mouseState.Y;
                _panDifference.X = 0;
                _panDifference.Y = 0;

                _panStartCameraPosition.X = _desiredPosition.X;
                _panStartCameraPosition.Y = _desiredPosition.Y;
            }
            else
            {
                _panDifference.X = mouseState.X - _panStartWindowPosition.X;
                _panDifference.Y = mouseState.Y - _panStartWindowPosition.Y;

                _desiredPosition.X = _panStartCameraPosition.X + _panDifference.X;
                _desiredPosition.Y = _panStartCameraPosition.Y + _panDifference.Y;
            }
        }
        _lastLeftMouseButtonState = mouseState.LeftButton;

        if (mouseState.ScrollWheelValue > _lastScrollWheelValue)
        {
            _currentZoomLevel = Math.Max(_currentZoomLevel - 1, 0);
        }
        else if (mouseState.ScrollWheelValue < _lastScrollWheelValue)
        {
            _currentZoomLevel = Math.Min(_currentZoomLevel + 1, _zoomLevelCount - 1);
        }
        _desiredPosition.Z = _zoomDistances[_currentZoomLevel];
        _lastScrollWheelValue = mouseState.ScrollWheelValue;

        var zoomDistanceDifference = _zoomDistances[_currentZoomLevel] - _currentZoomDistance;
        _currentZoomDistance += zoomDistanceDifference * 0.125F;

        _positionDifference = _desiredPosition - _lagPosition;

        _lagPosition += _positionDifference * 0.125F;

        _from.X = -_lagPosition.X;
        _from.Y = _lagPosition.Y + _lagPosition.Z;
        _from.Z = _lagPosition.Z;

        _to.X = -_lagPosition.X;
        _to.Y = _lagPosition.Y;

        ViewMatrix = Matrix.CreateLookAt(_from, _to, _up);
        ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75.0F), AspectRatio, 1, 10240);
    }
}
