using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using System.Net;
using System.Web.Script.Serialization;

namespace lotto
{
    public partial class Form1 : Form
    {
        private SqlConnection conn;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Width = 900;
            this.Height = 600;
            this.Left = 5;
            this.Top = 5;
            this.StartPosition = FormStartPosition.CenterScreen;
            getMainTable();
            dataGridView1.DataSource = maintab;
            fillDateDataSource();
        }
        DataTable maintab = new DataTable();
        SqlDataAdapter sdp;
        private void getMainTable()
        {
            conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["lottoMaindata"].ToString());

            conn.Open();
            sdp = new SqlDataAdapter("select * from maindata order by date", conn);
            SqlCommandBuilder builder = new SqlCommandBuilder(sdp);
            sdp.UpdateCommand = builder.GetUpdateCommand();
            sdp.Fill(maintab);
            //sdp.Dispose();

        }

        private void uptAvg()
        {
            updateAvg(ref maintab);
            sdp.Update(maintab);
            MessageBox.Show("OK");
        }

        private void updateAvg(ref DataTable dtb)
        {
            for (int i = 0; i < dtb.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(dtb.Rows[i]["avg"].ToString()))
                {
                    dtb.Rows[i].SetModified();
                    dtb.Rows[i]["avg"] = Math.Round((double)(int.Parse(dtb.Rows[i]["num1"].ToString()) + int.Parse(dtb.Rows[i]["num2"].ToString()) + int.Parse(dtb.Rows[i]["num3"].ToString()) + int.Parse(dtb.Rows[i]["num4"].ToString()) + int.Parse(dtb.Rows[i]["num5"].ToString()) + int.Parse(dtb.Rows[i]["num6"].ToString())) / 6);
                    List<int> num = new List<int>();
                    for (int j = 2; j < 8; j++)
                    {
                        num.Add(int.Parse(dtb.Rows[i][j].ToString()));
                    }
                    int big = 0, small = 0;
                    getminmax(num, ref big, ref small);
                    //dtb.Rows[i]["min"] = small;
                    //dtb.Rows[i]["max"] = big;
                    dtb.Rows[i]["range"] = big - small;

                }
            }
        }

        private void getminmax(List<int> bags, ref int max, ref int min)
        {
            int big = 0;
            int small = 50;
            foreach (int a in bags)
            {
                if (a > big)
                    big = a;
                if (a < small)
                    small = a;
            }
            max = big;
            min = small;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            uptAvg();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable tmpdtb = new DataTable();
            if ((sender as TabControl).SelectedTab.Text.Equals("統計預測"))
            {
                //var avg1= maintab.AsEnumerable().Average(d => (int)d["num1"]);

                sdp.SelectCommand.CommandText = "select A.* ,(A.avg1+A.avg2+A.avg3+A.avg4+A.avg5+A.avg6)/6 tavg from ( select  avg(num1) avg1,avg(num2) avg2,avg(num3) avg3,avg(num4) avg4,avg(num5) avg5,avg(num6) avg6,avg(nums) avgs from maindata) A";
                sdp.Fill(tmpdtb);
                maxAppearNum(ref tmpdtb);
                dataGridView2.DataSource = tmpdtb;
                setGridViewHeader();
                fillDateDataSource();
            }
        }

        private void fillDateDataSource()
        {
            comboBox2.DataSource = getAllDate();
        }

        private List<DateTime> getAllDate()
        {
            var dts = maintab.AsEnumerable()
               .Select(c => c.Field<DateTime>("date"))
               .OrderByDescending(ody => ody);
            return dts.ToList();
        }
        private List<DateTime> getAllDatebyDate(string sd,string ed)
        {
            sd = sd + "/1/1";
            DateTime sd1 = Convert.ToDateTime(sd);
            ed = ed + "/12/31";
            DateTime ed1 = Convert.ToDateTime(ed);
            var dts = maintab.AsEnumerable()
               .Where(d=>d.Field<DateTime>("date")>=sd1 && d.Field<DateTime>("date") <= ed1)
               .Select(c => c.Field<DateTime>("date"))
               .OrderByDescending(ody => ody);
            return dts.ToList();
        }

        private void setGridViewHeader()
        {
            DataGridViewRowHeaderCell avgheader = new DataGridViewRowHeaderCell();
            avgheader.Value = "平均";
            dataGridView2.RowHeadersWidth = 68;
            dataGridView2.Rows[0].HeaderCell = avgheader;
            DataGridViewRowHeaderCell hugeheader = new DataGridViewRowHeaderCell();
            hugeheader.Value = "眾數1";
            dataGridView2.Rows[1].HeaderCell = hugeheader;
            DataGridViewRowHeaderCell hugeheader1 = new DataGridViewRowHeaderCell();
            hugeheader1.Value = "次數1";
            dataGridView2.Rows[2].HeaderCell = hugeheader1;
            DataGridViewRowHeaderCell hugeheader2 = new DataGridViewRowHeaderCell();
            hugeheader2.Value = "眾數2";
            dataGridView2.Rows[3].HeaderCell = hugeheader2;
            DataGridViewRowHeaderCell hugeheader3 = new DataGridViewRowHeaderCell();
            hugeheader3.Value = "次數2";
            dataGridView2.Rows[4].HeaderCell = hugeheader3;
            foreach (DataGridViewColumn dc in dataGridView2.Columns)
            {
                dc.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void maxAppearNum(ref DataTable table)
        {
            //var grp = maintab.AsEnumerable().GroupBy(r => new {pp1 = r.Field<int>("num1"), pp2 = r.Field<int>("num2"), pp3 = r.Field<int>("num3"), pp4 = r.Field<int>("num4"), pp5 = r.Field<int>("num5"), pp6 = r.Field<int>("num6"), pps = r.Field<int>("nums") }).ToList();
            DataRow dr1 = table.NewRow();//眾數
            DataRow dr2 = table.NewRow();//出現次數
            DataRow dr3 = table.NewRow();//第二眾數
            DataRow dr4 = table.NewRow();//出現次數
            for (int i = 1; i < 8; i++)
            {
                string colnam = i == 7 ? "nums" : "num" + i.ToString();
                var grp = maintab.AsEnumerable()
                .GroupBy(r1 => new { pp1 = r1.Field<int>(colnam) })
                .Select(gp => new { key = gp.Key, cnt = gp.Count() })
                .OrderByDescending(ody => ody.cnt);

                dr1.SetField(i - 1, grp.ElementAt(0).key.pp1);
                dr2.SetField(i - 1, grp.ElementAt(0).cnt);
                dr3.SetField(i - 1, grp.ElementAt(1).key.pp1);
                dr4.SetField(i - 1, grp.ElementAt(1).cnt);
            }

            table.Rows.Add(dr1);
            table.Rows.Add(dr2);
            table.Rows.Add(dr3);
            table.Rows.Add(dr4);

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 || e.RowIndex >= dataGridView1.RowCount - 1) return;
            int[] s1 = new int[6];
            for (int i = 2; i < 8; i++)
            {
                s1[i - 2] = Convert.ToInt16(dataGridView1.Rows[e.RowIndex].Cells[i].Value);
            }
            setChartView(dataGridView1.Rows[e.RowIndex].Cells[1].FormattedValue.ToString(), s1);
        }

        private void setChartView(string date, int[] array)
        {
            Series series1 = new Series(date, 50);
            Random rnd = new Random(DateTime.Now.Second);
            series1.Color = ColorTranslator.FromWin32(rnd.Next());
            series1.ChartType = SeriesChartType.Line;
            if (array.Length < 7)
                series1.IsValueShownAsLabel = true;
            //將數值新增至序列
            for (int index = 0; index < array.Length; index++)
            {
                series1.Points.AddXY(index, array[index]);
            }
            if (this.chart1.Series.IndexOf(date) == -1)
                this.chart1.Series.Add(series1);
            //this.chart1.Titles.Add("");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            chart1.Series.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DateTime sd = new DateTime(2004, 01, 01).Date;
            DateTime ed = DateTime.Now.Date;
            if (dataGridView1.SelectedCells.Count <= 2)
            {
                sd = (DateTime)dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[1].Value;
                ed = (DateTime)dataGridView1.Rows[dataGridView1.SelectedCells[1].RowIndex].Cells[1].Value;
                if (sd > ed)
                {
                    sd = (DateTime)dataGridView1.Rows[dataGridView1.SelectedCells[1].RowIndex].Cells[1].Value;
                    ed = (DateTime)dataGridView1.Rows[dataGridView1.SelectedCells[0].RowIndex].Cells[1].Value;
                }
            }
            else
            {
                MessageBox.Show("請用ctrl+滑鼠左鍵選兩筆資料作為計算區間");
                return;

            }
            var sdata = maintab.AsEnumerable()
                     .Where(dr => dr.Field<DateTime>("date") >= sd && dr.Field<DateTime>("date") <= ed)
                     .Select(o => o.Field<int>("avg"));
            this.chart1.Series.Clear();
            setChartView("均值序列", sdata.ToArray());
        }

        private void button4_Click(object sender, EventArgs e)
        {
            aggregate agg = new aggregate(maintab);
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            summary.Clear();
            if (string.IsNullOrEmpty(textBox1.Text.Trim()) && string.IsNullOrEmpty(comboBox2.Text.Trim()))
            {
                calcByNum(agg);
            }
            else
            {
                string[] ary = null;
                if (!string.IsNullOrEmpty(comboBox2.Text.Trim()))
                {
                    ary = getNumbyDate(comboBox2.Text.Trim(),false);
                }
                else
                {
                    if (textBox1.Text.IndexOf(",") > -1)
                    {
                        ary = textBox1.Text.Split(new char[] { ',' });
                    }
                    else
                    {
                        ary = textBox1.Text.Split(new char[] { '	' });
                    }

                }

                Dictionary<int, Dictionary<int, int>> serial = agg.getBagData();

                IEnumerable<KeyValuePair<int, Dictionary<int, int>>> ss = serial.Where(p => ary.Contains(p.Key.ToString())).ToList();
                listBox1.Items.AddRange(addUIData(ss).ToArray());

            }
            sumColumn();
            //特別號累加
            IEnumerable<KeyValuePair<int, Dictionary<int, int>>> specbag = agg.getSpecBagData().OrderBy(r => r.Key);
            listBox2.Items.AddRange(addUIData(specbag).ToArray());
        }

        private string[] getNumbyDate(string date,bool withS)
        {
            DataRow[] drow = maintab.Select("date='" + date + "'");
            
            string[] ary = null;
            if (withS)
            {
                ary = new string[7];
                ary[0] = drow[0]["num1"].ToString().Length == 1? "0" + drow[0]["num1"].ToString() : drow[0]["num1"].ToString();
                ary[1] = drow[0]["num2"].ToString().Length == 1 ? "0" + drow[0]["num2"].ToString() : drow[0]["num2"].ToString();
                ary[2] = drow[0]["num3"].ToString().Length == 1 ? "0" + drow[0]["num3"].ToString() : drow[0]["num3"].ToString();
                ary[3] = drow[0]["num4"].ToString().Length == 1 ? "0" + drow[0]["num4"].ToString() : drow[0]["num4"].ToString();
                ary[4] = drow[0]["num5"].ToString().Length == 1 ? "0" + drow[0]["num5"].ToString() : drow[0]["num5"].ToString();
                ary[5] = drow[0]["num6"].ToString().Length == 1 ? "0" + drow[0]["num6"].ToString() : drow[0]["num6"].ToString();
                ary[6] = drow[0]["nums"].ToString().Length == 1 ? "0" + drow[0]["nums"].ToString() : drow[0]["nums"].ToString();
            }
            else
            {
                ary = new string[6];
                ary[0] = drow[0]["num1"].ToString();
                ary[1] = drow[0]["num2"].ToString();
                ary[2] = drow[0]["num3"].ToString();
                ary[3] = drow[0]["num4"].ToString();
                ary[4] = drow[0]["num5"].ToString();
                ary[5] = drow[0]["num6"].ToString();
            }
            
            return ary;
        }

        private void calcByNum(aggregate agg)
        {
            var serial = agg.getBagData().OrderBy(r => r.Key);
            listBox1.Items.AddRange(addUIData(agg.getBagData()).ToArray());
        }

        Dictionary<int, int> summary = new Dictionary<int, int>();
        private List<string> addUIData(IEnumerable<KeyValuePair<int, Dictionary<int, int>>> serial)
        {
            List<string> outstr = new List<string>();

            foreach (var item in serial.OrderBy(r => r.Key))
            {
                int looptimes = string.IsNullOrEmpty(comboBox1.Text) ? 0 : int.Parse(comboBox1.Text);
                var s1 = item.Value.OrderByDescending(o => o.Value);
                string cnt = "";
                foreach (var i1 in s1)
                {
                    if (looptimes > 0)
                    {
                        if (summary.Keys.Contains(i1.Key))
                        {
                            summary[i1.Key] += i1.Value;
                        }
                        else
                        {
                            summary.Add(i1.Key, i1.Value);
                        }
                        looptimes--;
                    }
                    cnt += i1.Key.ToString("00") + "(" + i1.Value + "),";
                }
                outstr.Add(item.Key.ToString("00") + " : " + cnt.Substring(0, cnt.Length - 1));
            }
            return outstr;
        }

        private List<string> getSelCount(int selectcount, IEnumerable<KeyValuePair<int, Dictionary<int, int>>> serial)
        {
            List<string> sumdict = new List<string>(); 
            foreach (var item in serial.OrderBy(r => r.Key))
            {
                var s1 = item.Value.OrderByDescending(o => o.Value);
                int tmpcnt = 0;
                foreach (var i1 in s1)
                {
                    if (tmpcnt < selectcount)
                    {
                        if (!sumdict.Contains(i1.Key.ToString("00")))
                            sumdict.Add(i1.Key.ToString("00"));
                        tmpcnt++;
                    }
                    else
                        break;
                }
            }
            return sumdict;
        }
        //將符合的加總放進前幾碼加總
        private void sumColumn()
        {
            string tmp = "";
            foreach (KeyValuePair<int, int> k in summary.OrderByDescending(p => p.Value))
            {
                //tmp += (k.Key.ToString("00") + "(" + k.Value.ToString("00") + "), ");
                tmp += (k.Key.ToString("00") + ",");
            }
            if (tmp.Length > 0)
                listBox3.Items.Add(tmp.Substring(0, tmp.Length - 1));
        }

        private void contextMenuStrip1_MouseClick(object sender, MouseEventArgs e)
        {
            if (listBox3.Items.Count > 0)
            {
                List<string> nextserial = new List<string>();
                if (comboBox2.Text.Trim().Length > 0)
                {
                    DataRow[] dr = maintab.Select("date='" + Convert.ToDateTime(comboBox2.Text.Trim()) + "'");
                    int nextid = Convert.ToInt32(dr[0][0]) + 1;
                    dr = maintab.Select("period='" + nextid.ToString() + "'");
                    if (dr.Length > 0)
                    {
                        for (int i = 2; i <= 8; i++)
                            nextserial.Add(dr[0][i].ToString().Length == 1 ? "0" + dr[0][i].ToString() : dr[0][i].ToString());
                    }

                }
                Form2 fm = new Form2(listBox3.Items[0].ToString(), nextserial);
                fm.Show();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            List<string> _date = targetDate();
            string success = "";
            string fail = "";
            Cursor = Cursors.WaitCursor;
            foreach (string sd in _date)
            {
                string url = @"http://jigang-xitun.rhcloud.com/date/" + sd;
                try {
                    getfromJson(url);
                    success += sd + " " ;
                }
                catch (Exception err)
                {
                    fail += sd + " ";
                    MessageBox.Show("更新"+ sd + "失敗，錯誤訊息:" +err.Message);
                }
            }
            Cursor = Cursors.Default;
            MessageBox.Show("更新完畢共" + _date.Count.ToString()+"筆\r\n" + "成功：" + (success.Trim().Length==0?"無":success.Replace(" ",",")) + "\r\n失敗：" + (fail.Trim().Length==0?"無":fail ));
        }

        private void getfromJson(string url)
        {
            WebClient wc = new WebClient();
            string json = wc.DownloadString(url);

            JavaScriptSerializer parser = new JavaScriptSerializer();
            dynamic info = parser.Deserialize<dynamic>(json);
            double avg = 0.0;
            foreach (string sn in info["ordernum"])
            {
                avg += Convert.ToInt32(sn);
            }
            avg = Math.Round(avg / 6);
            int range = Convert.ToInt32(info["ordernum"][5]) - Convert.ToInt32(info["ordernum"][0]);
            string s = info["period"] + "," + info["adate"] + "," + info["ordernum"][0] + "," + info["ordernum"][1] + "," + info["ordernum"][2] + "," + info["ordernum"][3] + "," + info["ordernum"][4] + "," + info["ordernum"][5] + "," + info["specialnum"] +"," + avg.ToString() + "," +range.ToString() ;
            save2DB(s.Split(new char[] { ','}));
        }

        private void save2DB(object[] uptdata)
        {
            maintab.Rows.Add(uptdata);
            sdp.Update(maintab);
        }

        private List<string> targetDate()
        {
            fillDateDataSource();
            DateTime dt_last = Convert.ToDateTime(comboBox2.Items[0].ToString());
            DateTime qryDate = DateTime.Now;
            List<string> rtn = new List<string>();
            while (DateTime.Now > dt_last)
            {
                if (dt_last.DayOfWeek == DayOfWeek.Tuesday)
                {
                    qryDate = dt_last.AddDays(3);
                    
                }
                else if (dt_last.DayOfWeek == DayOfWeek.Friday)
                {
                    qryDate = dt_last.AddDays(4);
                }
                if (DateTime.Now > qryDate.AddHours(21))
                    rtn.Add(qryDate.ToString("yyyy/MM/dd"));
                dt_last = qryDate;
            }
            return rtn;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            aggregate agg = new aggregate(maintab);
            listView1.BeginUpdate();
            List<DateTime> dts = getAllDatebyDate(numericUpDown2.Value.ToString(),numericUpDown3.Value.ToString());
            totalhitrate = 0;
            sumTotalCost = 0;
            sumTotalIncome = 0;
            listView1.Items.Clear();
            this.Cursor = Cursors.WaitCursor;
            for (int i = dts.Count-1; i > 0; i--)
            {
                string strDT = dts[i].ToString("yyyy/MM/dd");
                IEnumerable<KeyValuePair<int, Dictionary<int, int>>> ss1 = agg.getBagData().Where(p => getNumbyDate(strDT,false).Contains(p.Key.ToString())).ToList();
                fillListView(getSelCount((int)numericUpDown1.Value, ss1), dts[i], dts[i - 1]);
            }
            this.Cursor = Cursors.Default;
            listView1.EndUpdate();
            label8.Text = "總共" + (dts.Count-1).ToString() + "筆，中獎" + totalhitrate.ToString() + "筆，中獎率：" + Math.Round(((decimal)totalhitrate/ (dts.Count - 1)),3) + ",總損益：" + (sumTotalIncome - sumTotalCost).ToString();
        }

        private void fillListView(List<string> source, DateTime dt1, DateTime dt2)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = dt2.ToString("yyyy/MM/dd");
            //填入星期
            lvi.SubItems.Add(System.Globalization.DateTimeFormatInfo.CurrentInfo.DayNames[(byte)dt2.DayOfWeek]);
            //填入預測號碼
            source = accurately(source);
            string s = "";
            s = String.Join(",", source.Select(v => v.ToString()));
            ListViewItem.ListViewSubItem vls1 = new ListViewItem.ListViewSubItem(lvi,s);
            lvi.SubItems.Add(vls1);
            //填入獎號
            string[] hitnums = getNumbyDate(dt2.ToString("yyyy/MM/dd"),true);
            s = String.Join(",", hitnums);
            ListViewItem.ListViewSubItem vls2 = new ListViewItem.ListViewSubItem(lvi, s);
            lvi.SubItems.Add(vls2);
            //呼叫中獎號函式
            compareHitRate(source, hitnums,lvi);
            //呼叫組數中獎函式
            List<string> l = new List<string>();
            l.AddRange(hitnums);
            
            source.Sort();
            spreadhitlist(source.ToArray(),l,lvi);
            //listView1加入當日結果值
            listView1.Items.Add(lvi);
        }

        int totalhitrate = 0;
        private void compareHitRate(List<string> v, string[] hitnums, ListViewItem lvi)
        {
            int hitcnt = 0;
            foreach (string forcastnum in v)
            {
                if (hitnums.Contains(forcastnum.ToString()))
                {
                    hitcnt++;
                }
            }
            lvi.UseItemStyleForSubItems = false;
            ListViewItem.ListViewSubItem vls3 = new ListViewItem.ListViewSubItem(lvi, hitcnt.ToString());
            if (hitcnt >= 3)
            {
                //totalhitrate++;
                vls3.ForeColor = Color.Red;
                vls3.BackColor = Color.LightGray;
            }
            lvi.SubItems.Add(vls3);
        }

        double sumTotalCost = 0;
        double sumTotalIncome = 0;
        private void spreadhitlist(string[] forcast,List<string> hitnums, ListViewItem lvi)
        {
            if (forcast.Length > 5)
            {
                forcast = rearrangeAry(forcast);
            }
            string[] dst_ary = { "", "", "", "", "", "" };
            int hit = 0;
            int hit3 = 0;
            int hit4 = 0;
            int hit5 = 0;
            for (int i = 0; i <= forcast.Length - 6; i++)
            {
                Array.Copy(forcast, i, dst_ary, 0, 6);
                foreach (string s in dst_ary)
                {
                    if (hitnums.Contains(s))
                        hit++;
                }
                switch (hit)
                {
                    case 3:
                        hit3++;
                        break;
                    case 4:
                        hit4++;
                        break;
                    case 5:
                        hit5++;
                        break;
                }
                Array.Clear(dst_ary, 0, 6);
                hit = 0;
            }
            lvi.UseItemStyleForSubItems = false;
            Font fnt = new Font(lvi.Font, FontStyle.Bold);
            ListViewItem.ListViewSubItem vls4 = new ListViewItem.ListViewSubItem(lvi, hit3.ToString());
            ListViewItem.ListViewSubItem vls5 = new ListViewItem.ListViewSubItem(lvi, hit4.ToString());
            ListViewItem.ListViewSubItem vls6 = new ListViewItem.ListViewSubItem(lvi, hit5.ToString());
            if (hit3 > 0) vls4.Font = fnt;
            if (hit4 > 0) vls5.Font = fnt;
            if (hit5 > 0) vls6.Font = fnt;
            lvi.SubItems.Add(vls4); lvi.SubItems.Add(vls5); lvi.SubItems.Add(vls6);
            int totalCost = (forcast.Length - 6) * 50;
            sumTotalCost += totalCost;
            ListViewItem.ListViewSubItem vls7 = new ListViewItem.ListViewSubItem(lvi, Convert.ToString(totalCost));
            int totalInc = hit3 * 400 + hit4 * 1300 + hit5 * 25000;
            sumTotalIncome += totalInc;
            ListViewItem.ListViewSubItem vls8 = new ListViewItem.ListViewSubItem(lvi, Convert.ToString(totalInc));
            
            ListViewItem.ListViewSubItem vls9 = new ListViewItem.ListViewSubItem(lvi, Convert.ToString(totalInc-totalCost));
            if (totalInc - totalCost > 0)
            {
                totalhitrate++;
                vls9.ForeColor = Color.Red;
            }
            lvi.SubItems.Add(vls7); lvi.SubItems.Add(vls8); lvi.SubItems.Add(vls9);
        }

        public string[] rearrangeAry(string[] _array)
        {
            List<string> _ary = new List<string>(_array);

            _ary.AddRange(_ary.GetRange(0, 5));

            return _ary.ToArray();
        }

        private void 線圖ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 1)
            {
                chart1.Series.Clear();
                //setChartView("收益線", r.ToArray());
                Series series1 = new Series("收益線");
                Random rnd = new Random(DateTime.Now.Second);
                series1.Color = ColorTranslator.FromWin32(rnd.Next());
                switch (sender.ToString())
                {
                    case "直方圖":
                        series1.ChartType = SeriesChartType.Column;
                        break;
                    case "區域圖":
                        series1.ChartType = SeriesChartType.Area;
                        break;
                    default:
                        series1.ChartType = SeriesChartType.Line;
                        break;
                }
                
                series1.IsValueShownAsLabel = true;

                double sum = 0;
                
                //將數值新增至序列
                for (int index = 0; index < listView1.SelectedItems.Count; index++)
                {
                    sum += Convert.ToInt32(listView1.SelectedItems[index].SubItems[10].Text);
                    series1.Points.AddXY(listView1.SelectedItems[index].SubItems[0].Text, Convert.ToInt32(listView1.SelectedItems[index].SubItems[10].Text));
                }
                series1.Name = "總收益 " + sum.ToString();
                this.chart1.Series.Add(series1);
                tabControl1.SelectedIndex = 0;
            }         
                
        }

        private void 各組數中獎狀態ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1)
            {
                MessageBox.Show("請選擇一筆資料!");
                return;
            }
            List<string> ls = new List<string>();
            ls.AddRange(listView1.SelectedItems[0].SubItems[3].Text.Split(new char[] { ',' }));
            Form2 fm2 = new Form2(listView1.SelectedItems[0].SubItems[2].Text,ls);
            fm2.ShowDialog();
        }

        private List<string> accurately(List<string> s)
        {
            List<string> rtn = new List<string>();
            s.Sort();
            for (int i = 0; i < s.Count ; i+=2)
            {
                if (i == s.Count - 1) { rtn.Add(s[i]); continue; }
                decimal avg = Math.Truncate((decimal) (Convert.ToInt16(s[i]) + Convert.ToInt16(s[i + 1])) / 2);
                rtn.Add(s[i]);
                //if (!rtn.Contains(avg.ToString("00")))
                    rtn.Add(avg.ToString("00"));
                rtn.Add(s[i + 1]);
            }
            return rtn;
        }
    }
}
