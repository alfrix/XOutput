using SlimDX.DirectInput;
using System;
using System.Linq;
using System.Windows.Forms;

namespace XOutput
{
    public partial class ControllerOptions : Form
    {
        ControllerDevice dev;
        public ControllerOptions(ControllerDevice device)
        {
            InitializeComponent();
            dev = device;
            foreach(Button button in Controls.OfType<Button>())
            {
                int ind = Int32.Parse(button.Tag.ToString());
                button.Text = getBindingText(ind);
                button.Click += Button_Click;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            MouseEventArgs me = (MouseEventArgs)e;
            int ind = Int32.Parse(button.Tag.ToString());
            if (me.Button == MouseButtons.Left)
            {             
                detectButton(ind, button);
            }
            else if (me.Button == MouseButtons.Right)
            {
                dev.mapping[ind * 2] = 255;
            }
        }

        private string getBindingText(int i)
        {
            if (dev.mapping[i * 2] == 255) {
                return "Disabled";
            }
            byte subType = (byte)(dev.mapping[i * 2] & 0x0F);
            byte type = (byte)((dev.mapping[i * 2] & 0xF0) >> 4);
            byte num = (byte)(dev.mapping[(i * 2) + 1] + 1);
            string[] typeString = new string[] { "Button {0}", "{1}Axis {0}", "D-Pad {0} {2}" };

            string[] dpadString = new string[] { "Up", "Down", "Left", "Right" };
            return string.Format(typeString[type], num, "", dpadString[subType]);
        }

        // Button Detection

        private int[] getAxes(JoystickState jstate)
        {
            return new int[] { jstate.X, jstate.Y, jstate.Z, jstate.RotationX, jstate.RotationY, jstate.RotationZ };
        }

        private byte[] ifPressed(int[] axesStart)
        {
            dev.joystick.Poll();
            JoystickState jState = dev.joystick.GetCurrentState();
            bool[] buttons = jState.GetButtons();
            int[] dPads = jState.GetPointOfViewControllers();
            int[] axes = getAxes(jState);

            // Buttons
            int i = 0;
            foreach(bool button in buttons)
            {
                if (button)
                    return new byte[]{ 0, (byte)i};
                i++;
            }

            // dPads
            i = 0;
            foreach(int dPad in dPads)
            {
                if (dPad > -1)
                    return new byte[] { (byte)(32 + i), 0};
                i++;
            }

            // Axes
            i = 0;
            foreach(int axis in axes)
            {
                if (axis != axesStart[i])
                    return new byte[] { 16, (byte)i};

                i++;
            }
            return null;
        }

        private async void detectButton(int ind, Button button)
        {
            // Still axes position
            dev.joystick.Poll();
            int[] axes = getAxes(dev.joystick.GetCurrentState());

            byte[] res;

            // Wait until button pressed
            while (true) {
                res = ifPressed(axes);
                if (res != null)
                {
                    // Map Button
                    dev.mapping[ind * 2] = res[0];
                    dev.mapping[(ind * 2) + 1] = res[1];
                    break;
                }              
            }

            button.Text = getBindingText(ind);
        }

        private void onClose(object sender, EventArgs e) {
            dev.Save();
        }

    }
}
