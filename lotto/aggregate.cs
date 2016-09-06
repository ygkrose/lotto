using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lotto
{
    class aggregate
    {
        private string PopulationSDate = "2016/1/1";
        private string PopulationEDate = "2016/12/31";
        private int[][] allnum; //[no][0~5] 記錄各期的6個號碼
        private Dictionary<int, Dictionary<int,int>> aggBag = new Dictionary<int, Dictionary<int,int>>(); //<號碼,<下一期號碼,累加出現次數>>
        private const int bignum = 49;

        public aggregate(DataTable dtb)
        {
            DataRow[] dra = dtb.Select("'" +PopulationSDate + "' < date and date <'" + PopulationEDate + "'");
            var allserial = from r in dra.AsEnumerable()
                            select new[] {
                                r.Field<int>("num1"),
                                r.Field<int>("num2"),
                                r.Field<int>("num3"),
                                r.Field<int>("num4"),
                                r.Field<int>("num5"),
                                r.Field<int>("num6") };
            allnum = allserial.ToArray();
        }

        private void calcAggregate(int maxnum)
        {
            //巡迴每一期
            for (int i = 0; i < allnum.Length; i++)
            {
                //巡迴每一號碼
                for (int j=0; j< maxnum; j++)
                {
                    if (i == allnum.Length - 1)
                    {
                        //最後一期
                        var orderbyAggBag = aggBag.OrderBy(p => p.Key);
                        break;
                    }
                   
                    if (aggBag.Keys.Contains(allnum[i][j]))
                    {
                        
                        for (int z =0; z< maxnum; z++)
                        {
                            if (aggBag[allnum[i][j]].Keys.Contains(allnum[i + 1][z]))
                            {
                                aggBag[allnum[i][j]][allnum[i + 1][z]] += 1;
                            }
                            else
                            {
                                aggBag[allnum[i][j]].Add(allnum[i + 1][z], 1);
                            }
                        }
                    }
                    else
                    {
                        Dictionary<int, int> subserial = new Dictionary<int, int>();
                        for (int z = 0; z < maxnum; z++)
                        {
                            subserial.Add(allnum[i+1][z], 1);
                        }
                        aggBag.Add(allnum[i][j], subserial);
                    }
                }
            }
        }

        public Dictionary<int, Dictionary<int, int>> getBagData()
        {
            aggBag.Clear();
            calcAggregate(6);
            return aggBag;
        }

        public Dictionary<int, Dictionary<int, int>> getSpecBagData(DataTable dtb)
        {
            DataRow[] dra = dtb.Select("'" + PopulationSDate + "' < date and date < '" + PopulationEDate + "'");
            var specnum = from r in dra.AsEnumerable()
                          select new[] {
                                r.Field<int>("nums")};
            allnum = specnum.ToArray();
            aggBag.Clear();
            calcAggregate(1);
            return aggBag;
        }

    }
}
