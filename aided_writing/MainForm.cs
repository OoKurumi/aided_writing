﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace aidedWriting
{
    public partial class MainForm : Form
    {
        #region 得到光标在屏幕上的位置
        [DllImport("user32")]
        public static extern bool GetCaretPos(out Point lpPoint);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();
        [DllImport("user32.dll")]
        private static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, int fAttach);
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentThreadId();
        [DllImport("user32.dll")]
        private static extern void ClientToScreen(IntPtr hWnd, ref Point p);

        private Point CaretPos()
        {
            IntPtr ptr = GetForegroundWindow();
            Point p = new Point();

            if (ptr.ToInt32() != 0)
            {
                IntPtr targetThreadID = GetWindowThreadProcessId(ptr, IntPtr.Zero);
                IntPtr localThreadID = GetCurrentThreadId();

                if (localThreadID != targetThreadID)
                {
                    AttachThreadInput(localThreadID, targetThreadID, 1);
                    ptr = GetFocus();
                    if (ptr.ToInt32() != 0)
                    {
                        GetCaretPos(out p);
                        ClientToScreen(ptr, ref p);
                    }
                    AttachThreadInput(localThreadID, targetThreadID, 0);
                }
            }
            return p;
        }
        #endregion

        //预处理后的文本文件的路径
        private readonly string res_map_path = "res_map.txt";

        public MainForm()
        {
            InitializeComponent();
        }

        private Dictionary<string, string> auto_write_map = new Dictionary<string, string>();

        //加载map文件
        private void Form1_Load(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(res_map_path, Encoding.Default);
            while (!sr.EndOfStream)
            {
                string str1 = sr.ReadLine();
                string str2 = sr.ReadLine();
                if (str1 != null && str2 != null && !auto_write_map.ContainsKey(str1))
                {
                    auto_write_map.Add(str1, str2);
                }
            }
            if (sr != null)
            {
                sr.Close();
            }
            textBox2.Hide();
        }

        //在文本框中按键后修改自动补全的内容
        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (textBox1.Text.Length >= 4 && textBox1.SelectionStart >= 4)
            {
                string str = textBox1.Text.Substring(textBox1.SelectionStart - 4, 4);
                if (auto_write_map.ContainsKey(str))
                {
                    str = auto_write_map[str].Replace("#", "\r\n");
                    textBox2.Text = str;
                    Point temp;
                    GetCaretPos(out temp);
                    temp.Y += 30;
                    if (temp.X < 530)
                    {
                        temp.X += 18;
                    }
                    else
                    {
                        temp.X = 535;
                    }
                    textBox2.Location = temp;
                    textBox2.Show();
                    return;
                }
            }
            textBox2.Hide();
            textBox2.Text = "";
        }

        //自动填充补全的第一条内容
        private void Button1_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length > 0)
            {
                string s = textBox1.Text;
                int idx = textBox1.SelectionStart;
                s = s.Insert(idx, textBox2.Text.Replace("\r", "").Split('\n')[0]);
                textBox1.Text = s;
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.Focus();
            }
        }
    }
}
