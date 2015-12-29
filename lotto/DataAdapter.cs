using Devart.Data.PostgreSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lotto
{
    public class DataAdapter
    {
        private SqlConnection conn;
        private PgSqlConnection pgconn;

        public string CommandText { get; set; } = "";

        public DataAdapter(string sqlstr)
        {
            if (lotto.Properties.Resources.DbTyp == "SQL")
            {
                conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["lottoMaindata"].ToString());
                conn.Open();
            }
            else
            {
                pgconn = new PgSqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["pg_lottoMaindata"].ToString());
                pgconn.Open();
            }
            CommandText = sqlstr;
        }
        SqlDataAdapter sdp = null;
        PgSqlDataAdapter pgsdp = null;
        public void Fill(ref DataTable dtb)
        {
            if (lotto.Properties.Resources.DbTyp == "SQL")
            {
                SqlDataAdapter sdp = new SqlDataAdapter(CommandText, conn);
                sdp.Fill(dtb);
            }
            else
            {
                PgSqlDataAdapter pgsdp = new PgSqlDataAdapter(CommandText, pgconn);
                pgsdp.Fill(dtb);
            }
        }

        public void Update(DataTable dtb,string sql)
        {
            if (lotto.Properties.Resources.DbTyp == "SQL")
            {
                SqlDataAdapter sdp = new SqlDataAdapter(sql, conn);
                SqlCommandBuilder builder = new SqlCommandBuilder(sdp);
                sdp.UpdateCommand = builder.GetUpdateCommand();
                sdp.Update(dtb);
            }
            else
            {
                PgSqlDataAdapter pgsdp = new PgSqlDataAdapter(sql, pgconn);
                if (sql.IndexOf("avg") == -1)
                {
                    PgSqlCommandBuilder builder = new PgSqlCommandBuilder(pgsdp);
                    pgsdp.UpdateCommand = builder.GetUpdateCommand();
                }
                pgsdp.Update(dtb);
            }
        }



    }
   
}
