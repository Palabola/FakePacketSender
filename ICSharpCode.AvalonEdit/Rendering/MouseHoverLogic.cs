﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ICSharpCode.AvalonEdit.Rendering
{
    /// <summary>
    /// Encapsulates and adds MouseHover support to UIElements.
    /// </summary>
    public class MouseHoverLogic : IDisposable
    {
        private UIElement target;

        private DispatcherTimer mouseHoverTimer;
        private Point mouseHoverStartPoint;
        private MouseEventArgs mouseHoverLastEventArgs;
        private bool mouseHovering;

        /// <summary>
        /// Creates a new instance and attaches itself to the <paramref name="target" /> UIElement.
        /// </summary>
        public MouseHoverLogic(UIElement target)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            this.target = target;
            this.target.MouseLeave += MouseHoverLogicMouseLeave;
            this.target.MouseMove += MouseHoverLogicMouseMove;
        }

        private void MouseHoverLogicMouseMove(object sender, MouseEventArgs e)
        {
            Point newPosition = e.GetPosition(this.target);
            Vector mouseMovement = mouseHoverStartPoint - newPosition;
            if (Math.Abs(mouseMovement.X) > SystemParameters.MouseHoverWidth
                || Math.Abs(mouseMovement.Y) > SystemParameters.MouseHoverHeight)
            {
                StopHovering();
                mouseHoverStartPoint = newPosition;
                mouseHoverLastEventArgs = e;
                mouseHoverTimer = new DispatcherTimer(SystemParameters.MouseHoverTime, DispatcherPriority.Background,
                                                      OnMouseHoverTimerElapsed, this.target.Dispatcher);
                mouseHoverTimer.Start();
            }
            // do not set e.Handled - allow others to also handle MouseMove
        }

        private void MouseHoverLogicMouseLeave(object sender, MouseEventArgs e)
        {
            StopHovering();
            // do not set e.Handled - allow others to also handle MouseLeave
        }

        private void StopHovering()
        {
            if (mouseHoverTimer != null)
            {
                mouseHoverTimer.Stop();
                mouseHoverTimer = null;
            }
            if (mouseHovering)
            {
                mouseHovering = false;
                OnMouseHoverStopped(mouseHoverLastEventArgs);
            }
        }

        private void OnMouseHoverTimerElapsed(object sender, EventArgs e)
        {
            mouseHoverTimer.Stop();
            mouseHoverTimer = null;

            mouseHovering = true;
            OnMouseHover(mouseHoverLastEventArgs);
        }

        /// <summary>
        /// Occurs when the mouse starts hovering over a certain location.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseHover;

        /// <summary>
        /// Raises the <see cref="MouseHover"/> event.
        /// </summary>
        protected virtual void OnMouseHover(MouseEventArgs e)
        {
            if (MouseHover != null)
            {
                MouseHover(this, e);
            }
        }

        /// <summary>
        /// Occurs when the mouse stops hovering over a certain location.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseHoverStopped;

        /// <summary>
        /// Raises the <see cref="MouseHoverStopped"/> event.
        /// </summary>
        protected virtual void OnMouseHoverStopped(MouseEventArgs e)
        {
            if (MouseHoverStopped != null)
            {
                MouseHoverStopped(this, e);
            }
        }

        private bool disposed;

        /// <summary>
        /// Removes the MouseHover support from the target UIElement.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                this.target.MouseLeave -= MouseHoverLogicMouseLeave;
                this.target.MouseMove -= MouseHoverLogicMouseMove;
            }
            disposed = true;
        }
    }
}