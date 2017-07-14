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
                button.MouseUp += Button_Click;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            MouseEventArgs me = (MouseEventArgs)e;

            // Get button index
            int ind = Int32.Parse(button.Tag.ToString());

            if(me.Button == MouseButtons.Left) // Left mouse button event
            {
                using (ButtonBind dialog = new ButtonBind(dev, ind))
                {
                    dialog.ShowDialog();
                    if (dialog.DialogResult == DialogResult.OK)
                    {
                        button.Text = getBindingText(ind);
                    }
                }
            }
            else if(me.Button == MouseButtons.Right) // Right mouse button event
            {
                dev.mapping[ind * 2] = 255;
                dev.mapping[(ind * 2) + 1] = 0;
                button.Text = getBindingText(ind);
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

        private void onClose(object sender, EventArgs e) {
            dev.Save();
        }

    }
}
