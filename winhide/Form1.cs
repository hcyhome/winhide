using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winhide
{
    public partial class Form1 : Form
    {
        // Registers a hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);
        // Unregisters the hot key with Windows.
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        // Show window
        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        public const int WM_HOTKEY_MSG_ID = 0x0312;
        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x004;
        public const int MOD_NOREPEAT = 0x400;
        public const int WM_HOTKEY = 0x312;
        public const int DSIX = 0x36;

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOW = 5;

        private int kill_hotkeyToRegister = 0;
        private int kill_hotkeyInUse = 0;

        private int hide_hotkeyToRegister = 0;
        private int hide_hotkeyInUse = 0;
        private bool hideStatus = false;
        private int[] hideWndArray = null;

        private string processNameToHandle = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // register hotkey for kill the process
            if (!string.IsNullOrEmpty(textBox1.Text) && button1.Text != "取消结束热键")
            {
                if (RegisterHotKey(this.Handle, 1, 0, kill_hotkeyToRegister))
                {
                    button1.Text = "取消结束热键";
                    kill_hotkeyInUse = kill_hotkeyToRegister;
                }
                else
                {
                    MessageBox.Show("注册热键失败");
                }
            }
            else
            {
                UnregisterHotKey(this.Handle, 1);
                button1.Text = "注册一键结束热键";
                kill_hotkeyInUse = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // register hotkey for kill the process
            if (!string.IsNullOrEmpty(textBox2.Text) && button2.Text != "取消隐藏热键")
            {
                if (RegisterHotKey(this.Handle, 1, 0, hide_hotkeyToRegister))
                {
                    button2.Text = "取消隐藏热键";
                    hide_hotkeyInUse = hide_hotkeyToRegister;
                }
                else
                {
                    MessageBox.Show("注册热键失败");
                }
            }
            else
            {
                UnregisterHotKey(this.Handle, 1);
                button2.Text = "注册一键隐藏热键";
                hide_hotkeyInUse = 0;
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            textBox1.Text = e.KeyData.ToString();
            kill_hotkeyToRegister = (int)e.KeyCode;
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            textBox2.Text = e.KeyData.ToString();
            hide_hotkeyToRegister = (int)e.KeyCode;
        }

        /// <summary>
        /// Handle all hotkeys and executes the functions
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY_MSG_ID)
            {
                // get keyvalue
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);

                if ((int)key == hide_hotkeyInUse)
                {
                    if (hideStatus == false)
                    {
                        Process[] processList = Process.GetProcessesByName(processNameToHandle);
                        if (processList != null && processList.Length > 0)
                        {
                            int hideCount = 0;
                            hideWndArray = new int[processList.Length];

                            // hide all window
                            foreach (Process p in processList)
                            {
                                int hWnd = p.MainWindowHandle.ToInt32();
                                if (hWnd != 0)
                                {
                                    hideStatus = true;
                                    hideWndArray[hideCount++] = hWnd;
                                    Console.WriteLine("hide wnd: {0}", hWnd);
                                    AppendCommentLine("hide wnd: " + hWnd);
                                    ShowWindow(hWnd, SW_HIDE);
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (int hideWnd in hideWndArray)
                        {
                            Console.WriteLine("Show hide wnd: {0}", hideWnd);
                            AppendCommentLine("Show hide wnd: " + hideWnd);
                            ShowWindow(hideWnd, SW_SHOW);
                        }
                        hideStatus = false;
                    }
                }

                if ((int)key == kill_hotkeyInUse)
                {
                    Process[] processList = Process.GetProcessesByName(processNameToHandle);
                    if (processList != null && processList.Length > 0)
                    {
                        foreach (Process p in processList)
                        {
                            Console.WriteLine("Kill process: {0}", p.Id);
                            AppendCommentLine("Kill process: " + p.Id);
                            p.Kill();
                        }
                    }
                }

                Console.WriteLine("hot key received, key = {0}, hide_hotkeyInUse = {1}, kill_hotkeyInUse = {2}", (int)key, hide_hotkeyInUse, kill_hotkeyInUse);
                //textBox3.Text += mMsg + Keys.Return;
            }

            base.WndProc(ref m);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            
        }

        private void AppendCommentLine(string comment)
        {
            textBox3.Text += comment + Environment.NewLine;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Console.WriteLine("Window minimized ...");
                notifyIcon1.Visible = true;
                //notifyIcon1.ShowBalloonTip(2000);
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                Console.WriteLine("Window normal ...");
            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Normal;
            //this.ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            textBox4.Text = "wow-64";
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            processNameToHandle = textBox4.Text;
        }
    }
}
