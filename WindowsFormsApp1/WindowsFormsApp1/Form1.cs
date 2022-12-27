﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private long receive_count = 0;//接收字节计数
        private long send_count = 0;
        private StringBuilder sb = new StringBuilder();     //为了避免在接收处理函数中反复调用，依然声明为一个全局变量
        private DateTime current_time = new DateTime();    //为了避免在接收处理函数中反复调用，依然声明为一个全局变量
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            //批量添加波特率列表
            string[] baud = {"9600", "43000", "56000", "57600", "115200", "128000", "230400", "256000", "460800" };
            comboBox2.Items.AddRange(baud);
            //获取电脑当前可用串口并添加到选项列表中
            string[] GetPortNames =System.IO.Ports.SerialPort.GetPortNames();//获取已经存在的串口名称
            comboBox1.Items.AddRange(GetPortNames);
            //初始化串口
            comboBox1.Text = GetPortNames[0];
            comboBox2.Text = baud[0];
            comboBox3.Text = "8";
            comboBox4.Text = "None";
            comboBox5.Text = "1";    
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //将可能产生异常的代码放置在try块中
                //根据当前串口属性来判断是否打开
                if (SerialPort.IsOpen)
                {
                    //串口已经处于打开状态
                    SerialPort.Close();    //关闭串口
                    button1.Text = "打开串口";
                    button1.BackColor = Color.ForestGreen;
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    comboBox3.Enabled = true;
                    comboBox4.Enabled = true;
                    comboBox5.Enabled = true;
                    textBox_receive.Text = "";  //清空接收区
                    textBox_send.Text = "";     //清空发送区
                    label6.Text = "串口已关闭";
                    label6.ForeColor = Color.Red;
                }
                else
                {
                    //串口已经处于关闭状态，则设置好串口属性后打开
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    comboBox3.Enabled = false;
                    comboBox4.Enabled = false;
                    comboBox5.Enabled = false;
                    SerialPort.PortName = comboBox1.Text;
                    SerialPort.BaudRate = Convert.ToInt32(comboBox2.Text);
                    SerialPort.DataBits = Convert.ToInt16(comboBox3.Text);

                    if (comboBox4.Text.Equals("None"))
                        SerialPort.Parity = System.IO.Ports.Parity.None;
                    else if (comboBox4.Text.Equals("Odd"))
                        SerialPort.Parity = System.IO.Ports.Parity.Odd;
                    else if (comboBox4.Text.Equals("Even"))
                        SerialPort.Parity = System.IO.Ports.Parity.Even;
                    else if (comboBox4.Text.Equals("Mark"))
                        SerialPort.Parity = System.IO.Ports.Parity.Mark;
                    else if (comboBox4.Text.Equals("Space"))
                        SerialPort.Parity = System.IO.Ports.Parity.Space;

                    if (comboBox5.Text.Equals("1"))
                        SerialPort.StopBits = System.IO.Ports.StopBits.One;
                    else if (comboBox5.Text.Equals("1.5"))
                        SerialPort.StopBits = System.IO.Ports.StopBits.OnePointFive;
                    else if (comboBox5.Text.Equals("2"))
                        SerialPort.StopBits = System.IO.Ports.StopBits.Two;

                    SerialPort.Open();     //打开串口
                    button1.Text = "关闭串口";
                    button1.BackColor = Color.Firebrick;
                    label6.Text = "串口已打开";
                    label6.ForeColor = Color.Green;
                }
            }
            catch (Exception ex)
            {
                //捕获可能发生的异常并进行处理

                //捕获到异常，创建一个新的对象，之前的不可以再用
                SerialPort = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
                label6.Text = "串口已关闭";
                label6.ForeColor = Color.Red;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            byte[] temp = new byte[1];
            try
            {
                //首先判断串口是否开启
                if (SerialPort.IsOpen)
                {
                    int num = 0;   //获取本次发送字节数 //串口处于开启状态，将发送区文本发送

                    //判断发送模式
                    if (radioButton4.Checked)
                    {
                        //以HEX模式发送
                        //首先需要用正则表达式将用户输入字符中的十六进制字符匹配出来
                        string buf = textBox_send.Text;
                        string pattern = @"\s";
                        string replacement = "";
                        Regex rgx = new Regex(pattern);
                        string send_data = rgx.Replace(buf, replacement);

                        //不发送新行
                        num = (send_data.Length - send_data.Length % 2) / 2;
                        for (int i = 0; i < num; i++)
                        {
                            temp[0] = Convert.ToByte(send_data.Substring(i * 2, 2), 16);
                            SerialPort.Write(temp, 0, 1);  //循环发送
                        }
                        //如果用户输入的字符是奇数，则单独处理
                        if (send_data.Length % 2 != 0)
                        {
                            temp[0] = Convert.ToByte(send_data.Substring(textBox_send.Text.Length - 1, 1), 16);
                            SerialPort.Write(temp, 0, 1);
                            num++;
                        }
                        //判断是否需要发送新行
                        if (checkBox2.Checked)
                        {
                            //自动发送新行
                            SerialPort.WriteLine("");
                        }
                    }
                    else
                    {
                        //以ASCII模式发送
                        //判断是否需要发送新行
                        if (checkBox2.Checked)
                        {
                            //自动发送新行
                            SerialPort.WriteLine(textBox_send.Text);
                            num = textBox_send.Text.Length + 2; //回车占两个字节
                        }
                        else
                        {
                            //不发送新行
                            SerialPort.Write(textBox_send.Text);
                            num = textBox_send.Text.Length;
                        }
                    }

                    send_count += num;      //计数变量累加
                    label8.Text = "Tx:" + send_count.ToString() + "Bytes";   //刷新界面
                }
            }
            catch (Exception ex)
            {
                SerialPort.Close();
                //捕获到异常，创建一个新的对象，之前的不可以再用
                SerialPort = new System.IO.Ports.SerialPort();
                //刷新COM口选项
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
                //响铃并显示异常给用户
                System.Media.SystemSounds.Beep.Play();
                button1.Text = "打开串口";
                button1.BackColor = Color.ForestGreen;
                MessageBox.Show(ex.Message);
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                comboBox3.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
            }
        }
        //串口接收事件处理
        private void SerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int num = SerialPort.BytesToRead;//获取串口接收缓冲区中的字节数
            byte[] received_buf = new byte[num];//声明一个大小为num的字节数据用于存放读出的byte型数据
            receive_count += num;//接收字节计数变量增加nun
            SerialPort.Read(received_buf,0,num);//读取接收缓冲区中num个字节到byte数组中

            sb.Clear();     //防止出错,首先清空字符串构造器
            //sb.Append(Encoding.ASCII.GetString(received_buf));  //将整个数组解码为ASCII数组
            if (radioButton2.Checked)
            {
                //选中HEX模式显示
                foreach (byte b in received_buf)
                {
                    sb.Append(b.ToString("X2") + ' ');    //将byte型数据转化为2位16进制文本显示,用空格隔开
                }
            }
            else
            {
                //选中ASCII模式显示
                sb.Append(Encoding.ASCII.GetString(received_buf));  //将整个数组解码为ASCII数组
            }
            //遍历数组进行字符串转化及拼接
            //foreach (byte b in received_buf)
            // {
            //     sb.Append(b.ToString());
            // }

            try
             {
                //因为要访问UI资源，所以需要使用invoke方式同步ui
                Invoke((EventHandler)(delegate
                {
                    if (checkBox1.Checked)
                    {
                        //显示时间
                        current_time = System.DateTime.Now;     //获取当前时间
                        //textBox_receive.AppendText(current_time.ToString("yyyy:MM:dd:HH:mm:ss") + "  " + sb.ToString());
                        textBox_receive.AppendText(current_time.ToString("HH:mm:ss:fff") + "  " + sb.ToString());
                    }
                    else
                    {
                        //不显示时间 
                        textBox_receive.AppendText(sb.ToString());
                    }
                    label7.Text = "Rx:" + receive_count.ToString() + "Bytes";
                }
                  )
                );


             }
             catch (Exception ex)
             {
                 //响铃并显示异常给用户
                 System.Media.SystemSounds.Beep.Play();
                 MessageBox.Show(ex.Message);

             }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox_receive.Text = "";  //清空接收区
            receive_count = 0;          //计数清零
            label7.Text = "Rx:" + receive_count.ToString() + "Bytes";   //刷新界面

        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox_send.Text = "";     //清空发送区
            send_count = 0;          //计数清零
            label8.Text = "Tx:" + send_count.ToString() + "Bytes";   //刷新界面
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                //自动发送功能选中,开始自动发送
                numericUpDown1.Enabled = false;     //失能时间选择
                timer1.Interval = (int)numericUpDown1.Value;     //定时器赋初值
                timer1.Start();     //启动定时器
                label6.Text = "串口已打开" + " 自动发送中...";
            }
            else
            {
                //自动发送功能未选中,停止自动发送
                numericUpDown1.Enabled = true;     //使能时间选择
                timer1.Stop();     //停止定时器
                label6.Text = "串口已打开";

            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //定时时间到
            button2_Click(button2, new EventArgs());    //调用发送按钮回调函数
        }
    }
}

