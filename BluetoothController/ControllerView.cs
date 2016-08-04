using System;
using System.Threading;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Views;

namespace Controller
{
    public class ControllerView : View, View.IOnTouchListener
    {
        // Screen metrics in px
        public readonly float SCREEN_WIDTH;
        public readonly float SCREEN_HEIGHT;

        // Joystick ovals
        private ShapeDrawable m_ShapeStickLeft;
        private ShapeDrawable m_ShapeStickRight;

        // Displacement ovals
        private ShapeDrawable m_ShapeRadiusLeft;
        private ShapeDrawable m_ShapeRadiusRight;

        // Joystick border
        private ShapeDrawable m_ShapeBorderStickLeft;
        private ShapeDrawable m_ShapeBorderStickRight;

        // Displacement border
        private ShapeDrawable m_ShapeBorderRadiusLeft;
        private ShapeDrawable m_ShapeBorderRadiusRight;

        // Joystick controllers
        private Joystick m_LeftJS;
        private Joystick m_RightJS;

        // Controller position
        private readonly bool m_Inverted;

        // Transfer data via bluetooth
        private readonly BluetoothController.DataTransfer m_Transfer;

        // Interrupter
        private readonly BluetoothController.BluetoothInterrupt m_Interrupt;

        // Timer for sending data and checking BT connection
        private readonly System.Timers.Timer m_WriteDispatcherTimer;
        private long oldtime;

        public ControllerView(Context context, bool inverted) : base(context)
        {
            m_Inverted = inverted;

            SetOnTouchListener(this);
            SetBackgroundColor(Color.White);

            SCREEN_WIDTH = Resources.DisplayMetrics.WidthPixels;
            SCREEN_HEIGHT = Resources.DisplayMetrics.HeightPixels;

            m_Transfer = new BluetoothController.DataTransfer(this);

            InitShapes();
            InitJoysticks();
            oldtime = DateTime.Now.Millisecond;
            m_WriteDispatcherTimer = new System.Timers.Timer();
            m_WriteDispatcherTimer.Interval = 20;
            m_WriteDispatcherTimer.AutoReset = true;
            m_WriteDispatcherTimer.Elapsed += (sender, e) => Write(sender, e);
            m_WriteDispatcherTimer.Start();
        }

        /// <summary>
        /// Initializes the joystick and displacement shapes
        /// </summary>
        private void InitShapes()
        {
            // Paint for joystick ovals
            var paintStick = new Paint();
            paintStick.SetARGB(255, 88, 88, 88);
            paintStick.SetStyle(Paint.Style.Fill);
            // Shape for left joystick
            m_ShapeStickLeft = new ShapeDrawable(new OvalShape());
            m_ShapeStickLeft.Paint.Set(paintStick);
            // Shape for right joystick
            m_ShapeStickRight = new ShapeDrawable(new OvalShape());
            m_ShapeStickRight.Paint.Set(paintStick);

            // Paint for displacement ovals
            var paintRadius = new Paint();
            paintRadius.Color = Color.LightGray;
            //paintRadius.SetARGB(255, 230, 230, 230);
            //paintRadius.SetStyle(Paint.Style.Fill);
            paintRadius.SetStyle(Paint.Style.Fill);
            // Shape for left displacement 
            m_ShapeRadiusLeft = new ShapeDrawable(new OvalShape());
            m_ShapeRadiusLeft.Paint.Set(paintRadius);
            // Shape for right displacement
            m_ShapeRadiusRight = new ShapeDrawable(new OvalShape());
            m_ShapeRadiusRight.Paint.Set(paintRadius);

            // Paint for border ovals
            var paintBorder = new Paint();
            paintBorder.SetARGB(255, 44, 44, 44);
            paintStick.SetStyle(Paint.Style.Fill);
            // Shape for left joystick border
            m_ShapeBorderStickLeft = new ShapeDrawable(new OvalShape());
            m_ShapeBorderStickLeft.Paint.Set(paintBorder);
            // Shape for right joystick border
            m_ShapeBorderStickRight = new ShapeDrawable(new OvalShape());
            m_ShapeBorderStickRight.Paint.Set(paintBorder);
            // Shape for left displacement border
            m_ShapeBorderRadiusLeft = new ShapeDrawable(new OvalShape());
            m_ShapeBorderRadiusLeft.Paint.Set(paintBorder);
            // Shape for right displacement border
            m_ShapeBorderRadiusRight = new ShapeDrawable(new OvalShape());
            m_ShapeBorderRadiusRight.Paint.Set(paintBorder);
        }

        /// <summary>
        /// Sets the bounds for every joystick and displacement oval
        /// </summary>
        private void InitJoysticks()
        {
            m_LeftJS = new Joystick(SCREEN_WIDTH, SCREEN_HEIGHT, true, m_Inverted);
            m_RightJS = new Joystick(SCREEN_WIDTH, SCREEN_HEIGHT, false, m_Inverted);

            SetBoundsForLeftStick(
                (int)m_LeftJS.CenterX - (int)Joystick.STICK_RADIUS,
                m_Inverted ? (int)m_LeftJS.CenterY - (int)Joystick.STICK_RADIUS : (int)m_LeftJS.CenterY + (int)Joystick.STICK_RADIUS,
                (int)m_LeftJS.CenterX + (int)Joystick.STICK_RADIUS,
                m_Inverted ? (int)m_LeftJS.CenterY + (int)Joystick.STICK_RADIUS : (int)m_LeftJS.CenterY + 3 * (int)Joystick.STICK_RADIUS);

            SetBoundsForRightStick(
                (int)m_RightJS.CenterX - (int)Joystick.STICK_RADIUS,
                m_Inverted ? (int)m_RightJS.CenterY + (int)Joystick.STICK_RADIUS : (int)m_RightJS.CenterY - (int)Joystick.STICK_RADIUS,
                (int)m_RightJS.CenterX + (int)Joystick.STICK_RADIUS,
                m_Inverted ? (int)m_RightJS.CenterY + 3 * (int)Joystick.STICK_RADIUS : (int)m_RightJS.CenterY + (int)Joystick.STICK_RADIUS);

            m_ShapeRadiusLeft.SetBounds(
                (int)m_LeftJS.CenterX - (int)Joystick.DISPLACEMENT_RADIUS,
                (int)m_LeftJS.CenterY - (int)Joystick.DISPLACEMENT_RADIUS,
                (int)m_LeftJS.CenterX + (int)Joystick.DISPLACEMENT_RADIUS,
                (int)m_LeftJS.CenterY + (int)Joystick.DISPLACEMENT_RADIUS);

            m_ShapeRadiusRight.SetBounds(
                (int)m_RightJS.CenterX - (int)Joystick.DISPLACEMENT_RADIUS,
                (int)m_RightJS.CenterY - (int)Joystick.DISPLACEMENT_RADIUS,
                (int)m_RightJS.CenterX + (int)Joystick.DISPLACEMENT_RADIUS,
                (int)m_RightJS.CenterY + (int)Joystick.DISPLACEMENT_RADIUS);

            m_ShapeBorderRadiusLeft.SetBounds(
                (int)m_LeftJS.CenterX - (int)Joystick.DISPLACEMENT_RADIUS - 2,
                (int)m_LeftJS.CenterY - (int)Joystick.DISPLACEMENT_RADIUS - 2,
                (int)m_LeftJS.CenterX + (int)Joystick.DISPLACEMENT_RADIUS + 2,
                (int)m_LeftJS.CenterY + (int)Joystick.DISPLACEMENT_RADIUS + 2);

            m_ShapeBorderRadiusRight.SetBounds(
                (int)m_RightJS.CenterX - (int)Joystick.DISPLACEMENT_RADIUS - 2,
                (int)m_RightJS.CenterY - (int)Joystick.DISPLACEMENT_RADIUS - 2,
                (int)m_RightJS.CenterX + (int)Joystick.DISPLACEMENT_RADIUS + 2,
                (int)m_RightJS.CenterY + (int)Joystick.DISPLACEMENT_RADIUS + 2);
        }

        /// <summary>
        /// Checks single or multitouch and sets new bounds
        /// </summary>
        public bool OnTouch(View v, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Up:
                    if (m_Inverted)
                    {
                        if (e.GetX() <= SCREEN_WIDTH / 2)
                        {
                            UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY);
                        }
                        else
                        {
                            UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY + Joystick.DISPLACEMENT_RADIUS);
                        }
                    }
                    else
                    {
                        if (e.GetX() <= SCREEN_WIDTH / 2)
                        {
                            UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY + Joystick.DISPLACEMENT_RADIUS);
                        }
                        else
                        {
                            UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
                        }
                    }
                    break;
                case MotionEventActions.Pointer1Up:
                    if (m_Inverted)
                    {
                        for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                        {
                            if (e.GetX(i) <= SCREEN_WIDTH / 2)
                            {
                                UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY);
                            }
                            else
                            {
                                UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY + Joystick.DISPLACEMENT_RADIUS);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                        {
                            if (e.GetX(i) <= SCREEN_WIDTH / 2)
                            {
                                UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY + Joystick.DISPLACEMENT_RADIUS);
                            }
                            else
                            {
                                UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
                            }
                        }
                    }
                    break;
                case MotionEventActions.Pointer2Up:
                    if (m_Inverted)
                    {
                        for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                        {
                            if (e.GetX(i) <= SCREEN_WIDTH / 2)
                            {
                                UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY);
                            }
                            else
                            {
                                UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY + Joystick.DISPLACEMENT_RADIUS);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                        {
                            if (e.GetX(i) <= SCREEN_WIDTH / 2)
                            {
                                UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY + Joystick.DISPLACEMENT_RADIUS);
                            }
                            else
                            {
                                UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
                            }
                        }
                    }
                    break;
                default:
                    for (int i = 0; i < Math.Min(2, e.PointerCount); i++)
                    {
                        UpdateOvals(e.GetX(i), e.GetY(i));
                    }
                    break;
            }

            if (m_Inverted)
            {
                if (e.PointerCount == 1 && e.GetX() <= SCREEN_WIDTH / 2 && !m_RightJS.IsCentered())
                {
                    UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY + Joystick.DISPLACEMENT_RADIUS);
                }
                else if (e.PointerCount == 1 && e.GetX() > SCREEN_WIDTH / 2 && !m_LeftJS.IsCentered())
                {
                    UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY);
                }
            }
            else
            {
                if (e.PointerCount == 1 && e.GetX() <= SCREEN_WIDTH / 2 && !m_RightJS.IsCentered())
                {
                    UpdateOvals(m_RightJS.CenterX, m_RightJS.CenterY);
                }
                else if (e.PointerCount == 1 && e.GetX() > SCREEN_WIDTH / 2 && !m_LeftJS.IsCentered())
                {
                    UpdateOvals(m_LeftJS.CenterX, m_LeftJS.CenterY + Joystick.DISPLACEMENT_RADIUS);
                }
            }

            Invalidate();
            return true;
        }

        /// <summary>
        /// Sets new bounds for the joystick oval
        /// </summary>
        /// <param name="xPosition">X-Position of the touch</param>
        /// <param name="yPosition">Y-Position of the touch</param>
        private void UpdateOvals(float xPosition, float yPosition)
        {
            // Check if touch is in left or right half of the screen
            if (xPosition <= SCREEN_WIDTH / 2)
            {
                // Handle touch in the left half
                m_LeftJS.SetPosition(xPosition, yPosition);
                // Check if touch was inside the displacement radius
                if ((m_LeftJS.Abs) <= Joystick.DISPLACEMENT_RADIUS)
                {
                    // Draw left joystick with original coordinates
                    SetBoundsForLeftStick(
                    (int)xPosition - (int)Joystick.STICK_RADIUS,
                    (int)yPosition - (int)Joystick.STICK_RADIUS,
                    (int)xPosition + (int)Joystick.STICK_RADIUS,
                    (int)yPosition + (int)Joystick.STICK_RADIUS);
                }
                else
                {
                    // Draw left joystick with maximum coordinates
                    SetBoundsForLeftStick(
                    (int)(Joystick.DISPLACEMENT_RADIUS * Math.Cos(m_LeftJS.Angle * Math.PI / 180)) - (int)Joystick.STICK_RADIUS + (int)m_LeftJS.CenterX,
                    (int)(Joystick.DISPLACEMENT_RADIUS * Math.Sin(m_LeftJS.Angle * Math.PI / 180)) - (int)Joystick.STICK_RADIUS + (int)m_LeftJS.CenterY,
                    (int)(Joystick.DISPLACEMENT_RADIUS * Math.Cos(m_LeftJS.Angle * Math.PI / 180)) + (int)Joystick.STICK_RADIUS + (int)m_LeftJS.CenterX,
                    (int)(Joystick.DISPLACEMENT_RADIUS * Math.Sin(m_LeftJS.Angle * Math.PI / 180)) + (int)Joystick.STICK_RADIUS + (int)m_LeftJS.CenterY);

                    // OPTION: Set position
                    //m_LeftJS.SetPosition((int)(m_LeftJS.m_DisplacementRadius * Math.Cos(m_LeftJS.GetAngle() * Math.PI / 180)) + (int)m_LeftJS.CENTER_X, 
                    //    (int)(m_LeftJS.m_DisplacementRadius * Math.Sin(m_LeftJS.GetAngle() * Math.PI / 180)) + (int)m_LeftJS.CENTER_Y);

                }
            }
            else
            {
                // Handle touch in the right half
                m_RightJS.SetPosition(xPosition, yPosition);
                // Check if touch was inside the displacement radius
                if ((m_RightJS.Abs) <= Joystick.DISPLACEMENT_RADIUS)
                {
                    // Draw right joystick with original coordinates
                    SetBoundsForRightStick(
                     (int)xPosition - (int)Joystick.STICK_RADIUS,
                     (int)yPosition - (int)Joystick.STICK_RADIUS,
                     (int)xPosition + (int)Joystick.STICK_RADIUS,
                     (int)yPosition + (int)Joystick.STICK_RADIUS);
                }
                else
                {
                    // Draw left joystick with maximum coordinates
                    SetBoundsForRightStick(
                    (int)(Joystick.DISPLACEMENT_RADIUS * Math.Cos(m_RightJS.Angle * Math.PI / 180)) - (int)Joystick.STICK_RADIUS + (int)m_RightJS.CenterX,
                    (int)(Joystick.DISPLACEMENT_RADIUS * Math.Sin(m_RightJS.Angle * Math.PI / 180)) - (int)Joystick.STICK_RADIUS + (int)m_RightJS.CenterY,
                    (int)(Joystick.DISPLACEMENT_RADIUS * Math.Cos(m_RightJS.Angle * Math.PI / 180)) + (int)Joystick.STICK_RADIUS + (int)m_RightJS.CenterX,
                    (int)(Joystick.DISPLACEMENT_RADIUS * Math.Sin(m_RightJS.Angle * Math.PI / 180)) + (int)Joystick.STICK_RADIUS + (int)m_RightJS.CenterY);

                    // OPTION: Set position 
                    //m_RightJS.SetPosition((int)(m_RightJS.m_DisplacementRadius * Math.Cos(m_RightJS.GetAngle() * Math.PI / 180)) + (int)m_RightJS.CENTER_X,
                    //    (int)(m_RightJS.m_DisplacementRadius * Math.Sin(m_RightJS.GetAngle() * Math.PI / 180)) + (int)m_RightJS.CENTER_Y);
                }
            }
        }

        /// <summary>
        /// Draws the shapes onto the canvas, which is displayed afterwards
        /// </summary>
        protected override void OnDraw(Canvas canvas)
        {
            // Draw shapes
            m_ShapeBorderRadiusLeft.Draw(canvas);
            m_ShapeBorderRadiusRight.Draw(canvas);
            m_ShapeRadiusLeft.Draw(canvas);
            m_ShapeRadiusRight.Draw(canvas);
            m_ShapeBorderStickLeft.Draw(canvas);
            m_ShapeBorderStickRight.Draw(canvas);
            m_ShapeStickLeft.Draw(canvas);
            m_ShapeStickRight.Draw(canvas);

            // Set paint for data text
            var paint = new Paint();
            paint.SetARGB(255, 0, 0, 0);
            paint.TextSize = 20;
            paint.TextAlign = Paint.Align.Center;
            paint.StrokeWidth = 5;

            m_LeftJS.CalculateValues();
            m_RightJS.CalculateValues();

            if (!m_Inverted)
            {
                // Draw data text for left joystick
                canvas.DrawText("DATA LEFT JOYSTICK", m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2 - 30, paint);
                canvas.DrawText("Throttle: " + m_LeftJS.ThrottleValue, m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2, paint);
                canvas.DrawText("Rudder: " + m_LeftJS.RudderValue, m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2 + 30, paint);
                canvas.DrawText("Direction: " + m_LeftJS.Direction, m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2 + 60, paint);
                canvas.DrawText("Centered: " + m_LeftJS.IsCentered(), m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2 + 90, paint);

                // Draw data text for right joystick
                canvas.DrawText("DATA RIGHT JOYSTICK", m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2 - 30, paint);
                canvas.DrawText("Elevator: " + m_RightJS.ElevatorValue, m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2, paint);
                canvas.DrawText("Aileron: " + m_RightJS.AileronValue, m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2 + 30, paint);
                canvas.DrawText("Direction: " + m_RightJS.Direction, m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2 + 60, paint);
                canvas.DrawText("Centered: " + m_RightJS.IsCentered(), m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2 + 90, paint);
            }
            else if (m_Inverted)
            {
                // Draw data text for left joystick
                canvas.DrawText("DATA LEFT JOYSTICK", m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2 - 30, paint);
                canvas.DrawText("Elevator: " + m_LeftJS.ElevatorValue, m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2, paint);
                canvas.DrawText("Rudder: " + m_LeftJS.RudderValue, m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2 + 30, paint);
                canvas.DrawText("Direction: " + m_LeftJS.Direction, m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2 + 60, paint);
                canvas.DrawText("Centered: " + m_LeftJS.IsCentered(), m_LeftJS.CenterX, m_LeftJS.CenterY - SCREEN_HEIGHT / 2 + 90, paint);

                // Draw data text for right joystick
                canvas.DrawText("DATA RIGHT JOYSTICK", m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2 - 30, paint);
                canvas.DrawText("Throttle: " + m_RightJS.ThrottleValue, m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2, paint);
                canvas.DrawText("Aileron: " + m_RightJS.AileronValue, m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2 + 30, paint);
                canvas.DrawText("Direction: " + m_RightJS.Direction, m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2 + 60, paint);
                canvas.DrawText("Centered: " + m_RightJS.IsCentered(), m_RightJS.CenterX, m_RightJS.CenterY - SCREEN_HEIGHT / 2 + 90, paint);
            }

            // TO BE ADDED: Displaying received data
        }

        /// <summary>
        /// Helper method for setting the bounds of the left joystick
        /// </summary>
        /// <param name="left">Position of left bound</param>
        /// <param name="top">Position of top bound</param>
        /// <param name="right">Position of right bound</param>
        /// <param name="bottom">Position of bottom bound</param>
        private void SetBoundsForLeftStick(int left, int top, int right, int bottom)
        {
            m_ShapeStickLeft.SetBounds(left, top, right, bottom);
            m_ShapeBorderStickLeft.SetBounds(left - 2, top - 2, right + 2, bottom + 2);
        }

        /// <summary>
        /// Helper method for setting the bounds of the right joystick
        /// </summary>
        /// <param name="left">Position of left bound</param>
        /// <param name="top">Position of top bound</param>
        /// <param name="right">Position of right bound</param>
        /// <param name="bottom">Position of bottom bound</param>
        private void SetBoundsForRightStick(int left, int top, int right, int bottom)
        {
            m_ShapeStickRight.SetBounds(left, top, right, bottom);
            m_ShapeBorderStickRight.SetBounds(left - 2, top - 2, right + 2, bottom + 2);
        }

        /// <summary>
        /// Helper method for sending data via bluetooth to the device
        /// </summary>
        public void Write(object sender, System.Timers.ElapsedEventArgs e)
        {
            long newtime = DateTime.Now.Millisecond;
            int timesincelastsend = (int) (newtime - oldtime);
            oldtime = newtime;
            Console.WriteLine("Time: " + timesincelastsend);
            if (!m_Inverted)
            {
                m_Transfer.Write(m_LeftJS.ThrottleValue, m_LeftJS.RudderValue,
                                 m_RightJS.ElevatorValue, m_RightJS.AileronValue);
            }
            else
            {
                m_Transfer.Write(m_RightJS.ThrottleValue, m_LeftJS.RudderValue,
                                 m_LeftJS.ElevatorValue, m_RightJS.AileronValue);
            }
        }
    }
}