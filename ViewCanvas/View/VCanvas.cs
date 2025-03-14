using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace ViewCanvas.View
{
    public class VCanvas : Canvas
    {
        public event Callbacks.SelectionChangedEventHandler? SelectionChanged;
        public event Callbacks.MouseChangedEventHandler? MouseChanged;
        private readonly MatrixTransform _transform = new MatrixTransform();
        private Point _initialMousePosition;
        private Point elementPosition;
        private bool _dragging;

        private Vector _draggingDelta;
        public float Zoomfactor { get; set; } = 1.1f;

        public Point MousePosition
        {
            get; private set;
        }

        public UIElement? SelectedElement
        {
            get; private set;
        }

        protected virtual void OnSelectionChanged(VCanvas vCanvas, UIElement? uIElement)
        {
            SelectionChanged?.Invoke(vCanvas, uIElement);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(this));
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                if (Children.Contains((UIElement)e.Source))
                {
                    SelectedElement = (UIElement)e.Source;

                    OnSelectionChanged(this, SelectedElement);

                    Point mousePosition = Mouse.GetPosition(this);
                    double x = Canvas.GetLeft(SelectedElement);
                    double y = Canvas.GetTop(SelectedElement);

                    elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePosition;
                }
                _dragging = true;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MousePosition = _transform.Inverse.Transform(e.GetPosition(this));
            MouseChanged?.Invoke(this, MousePosition);

            if (e.MiddleButton == MouseButtonState.Pressed)
            {

                Vector delta = Point.Subtract(MousePosition, _initialMousePosition);
                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;

                foreach (UIElement child in Children)
                {
                    child.RenderTransform = _transform;
                }
            }

            if (_dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                double x = Mouse.GetPosition(this).X;
                double y = Mouse.GetPosition(this).Y;

                if (SelectedElement != null)
                {
                    Canvas.SetLeft(SelectedElement, x + _draggingDelta.X);
                    Canvas.SetTop(SelectedElement, y + _draggingDelta.Y);
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            _dragging = false;
            SelectedElement = null;
            base.OnPreviewMouseDown(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            zoom(e.Delta, Zoomfactor, Mouse.GetPosition(this));
            base.OnMouseWheel(e);
        }

        //   zoom(-1, 2, new Point(ActualWidth / 2 - 400, ActualHeight / 2));
        public void zoom(int Delta, float scaleFactor, Point mousePostion)
        {



            if (Delta < 0)
            {
                scaleFactor = 1f / scaleFactor;
            }

            //Point mousePostion = Mouse.GetPosition(this);// e.GetPosition(this);

            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            _transform.Matrix = scaleMatrix;


            foreach (UIElement child in Children.AsParallel())
            {
                double x = Canvas.GetLeft(child);
                double y = Canvas.GetTop(child);

                double sx = x * scaleFactor;
                double sy = y * scaleFactor;

                Canvas.SetLeft(child, sx);
                Canvas.SetTop(child, sy);

                child.RenderTransform = _transform;
            }
        }

    }
}
