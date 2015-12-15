using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using RDotNet;
using System.Net;
using System.IO;
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
            // TODO: 這行程式碼會將資料載入 'lottoDataSet.maindata' 資料表。您可以視需要進行移動或移除。
            this.maindataTableAdapter.Fill(this.lottoDataSet.maindata);
            this.Width = 900;
            this.Height = 600;
            this.Left = 5;
            this.Top = 5;
            getMainTable();
            dataGridView1.DataSource = maintab;
            
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

                var dts = maintab.AsEnumerable()
                .Select(c => c.Field<DateTime>("date"))
                .OrderByDescending(ody => ody);
                comboBox2.DataSource = dts.ToList();
            }
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
                    //DataRow[] dr = maintab.Select("date='#" + textBox2.Text.Trim() + "#'");
                    var sdata = maintab.AsEnumerable()
                    .Where(dr => dr.Field<DateTime>("date") == Convert.ToDateTime(comboBox2.Text.Trim()))
                    .Select(o => new { num1 = o.Field<int>("num1").ToString(), num2 = o.Field<int>("num2").ToString(), num3 = o.Field<int>("num3").ToString(), num4 = o.Field<int>("num4").ToString(), num5 = o.Field<int>("num5").ToString(), num6 = o.Field<int>("num6").ToString() });
                    foreach (var row in sdata)
                    {
                        ary = new string[6];
                        ary[0] = row.num1;
                        ary[1] = row.num2;
                        ary[2] = row.num3;
                        ary[3] = row.num4;
                        ary[4] = row.num5;
                        ary[5] = row.num6;
                    }
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
                if (DateTime.Now > qryDate)
                    rtn.Add(qryDate.ToString("yyyy/MM/dd"));
                dt_last = qryDate;
            }
            return rtn;
        }
    }
}
