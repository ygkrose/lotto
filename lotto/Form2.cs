﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lotto
{
    public partial class Form2 : Form
    {
        private string numsString = "";
        List<string> _forcast = null;

        public Form2(string serial,List<string> forcast)
        {
            InitializeComponent();
            numsString = serial;
            _forcast = forcast;
        }


        private void Form2_Load(object sender, EventArgs e)
        {
           
            string[] src_ary = numsString.Split(new char[] { ',' });
            if (src_ary.Length > 5)
            {
               src_ary =  rearrangeAry(src_ary);
            }
            string[] dst_ary = { "", "", "", "", "", "" };
            listBox1.Items.Add(String.Join("," , _forcast.ToArray()));
            this.Text = "預測共" + Convert.ToString(src_ary.Length-6) + "組" ;
            for (int i = 0; i < src_ary.Length - 6 ; i++)
            {
                Array.Copy(src_ary, i, dst_ary, 0, 6);
                Array.Sort(dst_ary);
                
                listBox1.Items.Add(String.Join(",", dst_ary));
                Array.Clear(dst_ary, 0, 6);
            }
            
        }

        private string[] rearrangeAry(string[] _array)
        {
            List<string> _ary = new List<string>(_array);
            
            _ary.AddRange(_ary.GetRange(0, 5));

            return _ary.ToArray();
        }

        private string[] addAvg(string[] _ary)
        {
            double avg = _ary.Average(item => Convert.ToInt32(item));
            List<string> rtn = new List<string>(_ary);
            rtn.Add(avg.ToString());
            return rtn.ToArray();
        }

        private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
        {
            ListBox list = (ListBox)sender;

            // Draw the background of the ListBox control for each item.
            e.DrawBackground();
            // Define the default color of the brush as black.
            Brush myBrush = Brushes.Black;
            Brush redBrush = Brushes.Red;
            float seperate = e.Bounds.Width / 11;
            float _xposition = e.Bounds.Left;
            float _yposition = e.Bounds.Top;
            foreach (string s in list.Items[e.Index].ToString().Split(new char[] {',' }))
            {
                if (_forcast.Contains(s.Trim()))
                    e.Graphics.DrawString(s + " ", e.Font, redBrush, _xposition, _yposition);
                else
                    e.Graphics.DrawString(s + " ", e.Font, myBrush, _xposition, _yposition);
                _xposition += seperate;

            }
            //e.Graphics.DrawString(list.Items[e.Index].ToString(),
           //     e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }
    }
}
