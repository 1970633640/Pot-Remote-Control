using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Forms;
using SharpDX.XInput;

namespace Pot_Remote_Control
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        string keycode = "";
        Controller controller;
        Gamepad gamepad;
        State previousState;
        public bool connected = false;
        AutomationFocusChangedEventHandler focusHandler = null;
        private bool timerCache=true;

        public Form1()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/1970633640/Pot-Remote-Control");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SubscribeToFocusChange();
            controller = new Controller(UserIndex.One);
            connected = controller.IsConnected;
            if (!connected)
            {
                MessageBox.Show("Controller required!\nhttps://github.com/1970633640/Pot-Remote-Control", "No Xinput controller detected!");
                this.Close();
            }
        }

        private void OnFocusChange(object sender, AutomationFocusChangedEventArgs e)
        {
            AutomationElement focusedElement = sender as AutomationElement;
            if (focusedElement != null)
            {
                int processId = focusedElement.Current.ProcessId;
                using (Process process = Process.GetProcessById(processId))
                {
                    Debug.WriteLine(process.ProcessName);
                    if (process.ProcessName == "PotPlayerMini64")
                        timer1.Enabled = true;
                    else
                        timer1.Enabled = false;
                }
            }
        }

        private void SubscribeToFocusChange()
        {
            focusHandler = new AutomationFocusChangedEventHandler(OnFocusChange);
            Automation.AddAutomationFocusChangedEventHandler(focusHandler);
        }

        private void UnsubscribeFocusChange()
        {
            if (focusHandler != null)
            {
                Automation.RemoveAutomationFocusChangedEventHandler(focusHandler);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                UnsubscribeFocusChange();
                SubscribeToFocusChange();
            }
            else
            {
                timer1.Enabled = false;
                UnsubscribeFocusChange();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var state = controller.GetState();

            if (previousState.PacketNumber != state.PacketNumber)
            {
                gamepad = controller.GetState().Gamepad;
                if (gamepad.Buttons == GamepadButtonFlags.A)
                    SendKeys.SendWait(" ");
                if (gamepad.Buttons == GamepadButtonFlags.B)
                    SendKeys.SendWait("%{F4}");
                if (gamepad.Buttons == GamepadButtonFlags.Start)
                    SendKeys.SendWait("{ENTER}");
                if (gamepad.Buttons == GamepadButtonFlags.RightShoulder)
                    SendKeys.SendWait("{PGDN}");
                if (gamepad.Buttons == GamepadButtonFlags.LeftShoulder)
                    SendKeys.SendWait("{PGUP}");

                if (gamepad.Buttons == GamepadButtonFlags.DPadRight && (!(gamepad.Buttons == GamepadButtonFlags.X)))
                    SendKeys.SendWait("{RIGHT}");
                if (gamepad.Buttons == GamepadButtonFlags.DPadLeft && (!(gamepad.Buttons == GamepadButtonFlags.X)))
                    SendKeys.SendWait("{LEFT}");
                if (gamepad.Buttons == GamepadButtonFlags.DPadUp && (!(gamepad.Buttons == GamepadButtonFlags.X)))
                    keybd_event((byte)Keys.VolumeUp, 0, 0, 0); // increase volume
                if (gamepad.Buttons == GamepadButtonFlags.DPadDown && (!(gamepad.Buttons == GamepadButtonFlags.X)))
                    keybd_event((byte)Keys.VolumeDown, 0, 0, 0); // decrease volume

                if (gamepad.Buttons == (GamepadButtonFlags.DPadRight | GamepadButtonFlags.X))
                    SendKeys.SendWait("%{RIGHT}");
                if (gamepad.Buttons == (GamepadButtonFlags.DPadLeft | GamepadButtonFlags.X))
                    SendKeys.SendWait("%{LEFT}");
                if (gamepad.Buttons == (GamepadButtonFlags.DPadUp |  GamepadButtonFlags.X))
                    SendKeys.SendWait("%{UP}");
                if (gamepad.Buttons == (GamepadButtonFlags.DPadDown | GamepadButtonFlags.X))
                    SendKeys.SendWait("%{DOWN}");

                if ((float)gamepad.RightTrigger < 5 && (float)gamepad.LeftTrigger < 5)
                {
                    timer2.Enabled = false;
                    timerCache = true;
                }

                if ((float)gamepad.RightTrigger >= 5 && (float)gamepad.RightTrigger <= 100)
                { timer2.Enabled = true; keycode = "{RIGHT}"; if (timerCache) timer2.Interval = 10; }
                if ((float)gamepad.RightTrigger >= 101 && (float)gamepad.RightTrigger <= 150)
                { timer2.Enabled = true; keycode = "^{RIGHT}"; if (timerCache) timer2.Interval = 10; }
                if ((float)gamepad.RightTrigger >= 151 && (float)gamepad.RightTrigger <= 200)
                { timer2.Enabled = true; keycode = "+{RIGHT}"; if (timerCache) timer2.Interval = 10; }
                if ((float)gamepad.RightTrigger >= 201 && (float)gamepad.RightTrigger <= 255)
                { timer2.Enabled = true; keycode = "^%{RIGHT}"; if (timerCache) timer2.Interval = 10; }

                if ((float)gamepad.LeftTrigger >= 5 && (float)gamepad.LeftTrigger <= 100)
                { timer2.Enabled = true; keycode = "{LEFT}"; if (timerCache) timer2.Interval = 10; }
                if ((float)gamepad.LeftTrigger >= 101 && (float)gamepad.LeftTrigger <= 150)
                { timer2.Enabled = true; keycode = "^{LEFT}"; if (timerCache) timer2.Interval = 10; }
                if ((float)gamepad.LeftTrigger >= 151 && (float)gamepad.LeftTrigger <= 200)
                { timer2.Enabled = true; keycode = "+{LEFT}"; if (timerCache) timer2.Interval = 10; }
                if ((float)gamepad.LeftTrigger >= 201 && (float)gamepad.LeftTrigger <= 255)
                { timer2.Enabled = true; keycode = "^%{LEFT}"; if (timerCache) timer2.Interval = 10; }
            }
            previousState = state;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            SendKeys.SendWait(keycode);
            timerCache = false;
            timer2.Interval = 500;
        }
    }
}
