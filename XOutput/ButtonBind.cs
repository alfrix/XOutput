using SlimDX.DirectInput;
using System.Windows.Forms;
using System.Threading;

namespace XOutput
{
    public partial class ButtonBind : Form
    {
        private ControllerDevice dev;
        private bool isBinding;

        public ButtonBind(ControllerDevice device, int index)
        {
            InitializeComponent();
            dev = device;
            isBinding = true;
            new Thread(() => { detectButton(index); }).Start();
        }


        // Button Detection
        public bool[] getPov(int i)
        {
            bool[] b = new bool[4];
            switch (i)
            {
                case -1: b[0] = false; b[1] = false; b[2] = false; b[3] = false; break;
                case 0: b[0] = true; b[1] = false; b[2] = false; b[3] = false; break;
                case 4500: b[0] = true; b[1] = false; b[2] = false; b[3] = true; break;
                case 9000: b[0] = false; b[1] = false; b[2] = false; b[3] = true; break;
                case 13500: b[0] = false; b[1] = true; b[2] = false; b[3] = true; break;
                case 18000: b[0] = false; b[1] = true; b[2] = false; b[3] = false; break;
                case 22500: b[0] = false; b[1] = true; b[2] = true; b[3] = false; break;
                case 27000: b[0] = false; b[1] = false; b[2] = true; b[3] = false; break;
                case 31500: b[0] = true; b[1] = false; b[2] = true; b[3] = false; break;
            }
            return b;
        }

        private byte[] ifPressed(int[] axesStart)
        {
            dev.joystick.Poll();
            JoystickState jState = dev.joystick.GetCurrentState();
            bool[] buttons = jState.GetButtons();
            int[] dPads = jState.GetPointOfViewControllers();
            int[] axes = ControllerDevice.GetAxes(jState);

            // Buttons
            int i = 0;
            foreach (bool button in buttons)
            {
                if (button)
                    return new byte[] { 0, (byte)i };
                i++;
            }

            // dPads
            i = 0;
            foreach (int dPad in dPads)
            {
                bool[] dp = getPov(dPad);
                for(int x = 0; x<4; x++)
                {
                    if(dp[x])
                        return new byte[] { (byte)(32+x), (byte)i};
                }
                i++;
            }

            // Axes
            i = 0;
            foreach (int axis in axes)
            {
                if (axis != axesStart[i])
                    return new byte[] { 16, (byte)i };

                i++;
            }
            return null;
        }

        private void detectButton(int ind)
        {
            // Still axes position
            dev.joystick.Poll();
            int[] axes = ControllerDevice.GetAxes(dev.joystick.GetCurrentState());

            byte[] res;

            // Wait until button pressed
            while (isBinding)
            {
                res = ifPressed(axes);
                if (res != null)
                {
                    // Map Button
                    dev.mapping[ind * 2] = res[0];
                    dev.mapping[(ind * 2) + 1] = res[1];
                    Invoke((MethodInvoker)delegate { Hide(); });
                    DialogResult = DialogResult.OK;
                    isBinding = false;                               
                    break;
                }
            }
            
        }

        private void btn_cancel_Click(object sender, System.EventArgs e)
        {
            isBinding = false;
        }
    }
}
