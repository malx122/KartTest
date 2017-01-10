using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;


namespace blend
{
    public static class MouseSimulation
    {
        /// <summary>
        /// Struct representing a point.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        //This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);

        public const int MOUSEEVENTF_MOVE               = 0x0001; /* mouse move */
        public const int MOUSEEVENTF_LEFTDOWN           = 0x0002; /* left button down */
        public const int MOUSEEVENTF_LEFTUP             = 0x0004; /* left button up */
        public const int MOUSEEVENTF_RIGHTDOWN          = 0x0008; /* right button down */
        public const int MOUSEEVENTF_RIGHTUP            = 0x0010; /* right button down */
        public const int MOUSEEVENTF_MIDDLEDOWN         = 0x0020; /* middle button down */
        public const int MOUSEEVENTF_MIDDLEUP           = 0x0040; /* middle button up */
        public const int MOUSEEVENTF_XDOWN              = 0x0080; /* x button down */
        public const int MOUSEEVENTF_XUP                = 0x0100; /* x button down */
        public const int MOUSEEVENTF_WHEEL              = 0x0800; /* wheel button rolled */
        public const int MOUSEEVENTF_HWHEEL             = 0x01000 ;/* hwheel button rolled */
        public const int MOUSEEVENTF_MOVE_NOCOALESCE    = 0x2000; /* do not coalesce mouse moves */
        public const int MOUSEEVENTF_VIRTUALDESK       = 0x4000; /* map to entire virtual desktop */
        public const int MOUSEEVENTF_ABSOLUTE           = 0x8000; /* absolute move */

        public const int XBUTTON1 = 0x0001;
        public const int XBUTTON2 = 0x0002;

        public const int DEFAULT_MOUSE_SPEED = 2;
        public const int MAX_MOUSE_SPEED = 100;

        public const uint KEY_IGNORE = 0xFFC3D44F;

        public enum Button
        {
            Left,
            Right,
            Middle,
            X1,
            X2
        }

        public static int MOUSE_COORD_TO_ABS(double coord, int width_or_height)
        {
            return (int)(((65536 * coord) / width_or_height) + 1);
        }

        public static int ABS_TO_MOUSE_COORD(double coord, int width_or_height)
        {
            return (int)((coord-1)*width_or_height/65536);
        }

        public static void DoMouseDelay() // Helper function for the mouse functions below.
        {
	        Thread.Sleep(10);
        }


        public static void MouseMove(int x, int y, int event_flags, int speed)
        {

	        // The playback mode returned from above doesn't need these flags added because they're ignored for clicks:
	        event_flags |= MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE; // Done here for caller, for easier maintenance.
	       
	        // Find dimensions of primary monitor.
	        // Without the MOUSEEVENTF_VIRTUALDESK flag (supported only by SendInput, and then only on
	        // Windows 2000/XP or later), MOUSEEVENTF_ABSOLUTE coordinates are relative to the primary monitor.
	        int screen_width = (int)(GetSystemMetrics(SystemMetric.SM_CXVIRTUALSCREEN)*1.5);
	        int screen_height = (int)(GetSystemMetrics(SystemMetric.SM_CYSCREEN)*1.5);


	        x = MOUSE_COORD_TO_ABS(x, screen_width);
	        y = MOUSE_COORD_TO_ABS(y, screen_height);
	
	        if (speed < 0)  // This can happen during script's runtime due to something like: MouseMove, X, Y, %VarContainingNegative%
		        speed = 0;  // 0 is the fastest.
	        else
		        if (speed > MAX_MOUSE_SPEED)
			        speed = MAX_MOUSE_SPEED;
	        if (speed == 0)// || sSendMode == SM_INPUT) // Instantanous move to destination coordinates with no incremental positions in between.
	        {
          		        // See the comments in the playback-mode section at the top of this function for why SM_INPUT ignores aSpeed.
                mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, new UIntPtr(KEY_IGNORE));
		        DoMouseDelay(); // Inserts delay for all modes except SendInput, for which it does nothing.
		        return;
	        }

	        // Since above didn't return, use the incremental mouse move to gradually move the cursor until
	        // it arrives at the destination coordinates.
	        // Convert the cursor's current position to mouse event coordinates (MOUSEEVENTF_ABSOLUTE).
            POINT lpPoint;
            GetCursorPos(out lpPoint);

            Console.Out.WriteLine("Start: {0} {1}  End: {2} {3}", 
                MOUSE_COORD_TO_ABS(lpPoint.X, screen_width), MOUSE_COORD_TO_ABS(lpPoint.Y, screen_height), 
                x, y);

	        DoIncrementalMouseMove(
		          MOUSE_COORD_TO_ABS(lpPoint.X, screen_width)  // Source/starting coords.
		        , MOUSE_COORD_TO_ABS(lpPoint.Y, screen_height) //
		        , x, y, speed);                                // Destination/ending coords.

            GetCursorPos(out lpPoint);

            Console.Out.WriteLine("Result: {0} {1}",
                MOUSE_COORD_TO_ABS(lpPoint.X, screen_width), MOUSE_COORD_TO_ABS(lpPoint.Y, screen_height));

	        
        }

        public static int INCR_MOUSE_MIN_SPEED = 32;

        public static void DoIncrementalMouseMove(int aX1, int aY1, int aX2, int aY2, int aSpeed)
        // aX1 and aY1 are the starting coordinates, and "2" are the destination coordinates.
        // Caller has ensured that aSpeed is in the range 0 to 100, inclusive.
        {
	        // AutoIt3: So, it's a more gradual speed that is needed :)
	        int delta;
	
	        while (aX1 != aX2 || aY1 != aY2)
	        {
		        if (aX1 < aX2)
		        {
			        delta = (aX2 - aX1) / aSpeed;
			        if (delta == 0 || delta < INCR_MOUSE_MIN_SPEED)
				        delta = INCR_MOUSE_MIN_SPEED;
			        if ((aX1 + delta) > aX2)
				        aX1 = aX2;
			        else
				        aX1 += delta;
		        } 
		        else 
			        if (aX1 > aX2)
			        {
				        delta = (aX1 - aX2) / aSpeed;
				        if (delta == 0 || delta < INCR_MOUSE_MIN_SPEED)
					        delta = INCR_MOUSE_MIN_SPEED;
				        if ((aX1 - delta) < aX2)
					        aX1 = aX2;
				        else
					        aX1 -= delta;
			        }

		        if (aY1 < aY2)
		        {
			        delta = (aY2 - aY1) / aSpeed;
			        if (delta == 0 || delta < INCR_MOUSE_MIN_SPEED)
				        delta = INCR_MOUSE_MIN_SPEED;
			        if ((aY1 + delta) > aY2)
				        aY1 = aY2;
			        else
				        aY1 += delta;
		        } 
		        else 
			        if (aY1 > aY2)
			        {
				        delta = (aY1 - aY2) / aSpeed;
				        if (delta == 0 || delta < INCR_MOUSE_MIN_SPEED)
					        delta = INCR_MOUSE_MIN_SPEED;
				        if ((aY1 - delta) < aY2)
					        aY1 = aY2;
				        else
					        aY1 -= delta;
			        }

		        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, aX1, aY1,0,new UIntPtr(0));
		        DoMouseDelay();
	        } // while()
        }



        public static void MouseClickDrag(Button button, int startX, int startY, int endX, int endY, int speed)
        {
            SetCursorPos(startX, startY);
            int event_down = 0, event_up = 0, event_data = 0, event_flags = 0;
            switch (button)
            {
                case Button.Left:
                    event_down = MOUSEEVENTF_LEFTDOWN;
                    event_up = MOUSEEVENTF_LEFTUP;
                    break;
                case Button.Right:
                    event_down = MOUSEEVENTF_RIGHTDOWN;
                    event_up = MOUSEEVENTF_RIGHTUP;
                    break;
                case Button.Middle:
                    event_down = MOUSEEVENTF_MIDDLEDOWN;
                    event_up = MOUSEEVENTF_MIDDLEUP;
                    break;
                case Button.X1:
                case Button.X2:
                    event_down = MOUSEEVENTF_XDOWN;
                    event_up = MOUSEEVENTF_XUP;
                    event_data = (button == Button.X1) ? XBUTTON1 : XBUTTON2;
                    break;
            }

            MouseMove(startX, startY, event_flags, speed);

            mouse_event(event_flags | event_down, startX, startY, event_data, new UIntPtr(KEY_IGNORE)); // It ignores aX and aY when MOUSEEVENTF_MOVE is absent.
            DoMouseDelay(); // Inserts delay for all modes except SendInput, for which it does nothing.
            // Now that the mouse button has been pushed down, move the mouse to perform the drag:
            MouseMove(endX, endY, event_flags, speed); // It calls DoMouseDelay() and also converts aX2 and aY2 to MOUSEEVENTF_ABSOLUTE coordinates.
            DoMouseDelay(); // Duplicate, see below.
            // Above is a *duplicate* delay because MouseMove() already did one. But it seems best to keep it because:
            // 1) MouseClickDrag can be a CPU intensive operation for the target window depending on what it does
            //    during the drag (selecting objects, etc.)  Thus, and extra delay might help a lot of things.
            // 2) It would probably break some existing scripts to remove the delay, due to timing issues.
            // 3) Dragging is pretty rarely used, so the added performance of removing the delay wouldn't be
            //    a big benefit.
            mouse_event(event_flags | event_up, endX, endY, event_data, new UIntPtr(KEY_IGNORE)); // It ignores aX and aY when MOUSEEVENTF_MOVE is absent.
            DoMouseDelay();
            // Above line: It seems best to always do this delay too in case the script line that
            // caused us to be called here is followed immediately by another script line which
            // is either another mouse click or something that relies upon this mouse drag
            // having been completed:
        }
    }

}
