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


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ShopDataBase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DataTable ProfitByMonth = CalculateTotalSales(dateTimePicker1.Value, dateTimePicker2.Value);
            dataGridView2.DataSource = ProfitByMonth;

            Dictionary<int, string> monthNames = new Dictionary<int, string>()
            {
                {1, "Январь"},
                {2, "Февраль"},
                {3, "Март"},
                {4, "Апрель"},
                {5, "Май"},
                {6, "Июнь"},
                {7, "Июль"},
                {8, "Август"},
                {9, "Сентябрь"},
                {10, "Октябрь"},
                {11, "Ноябрь"},
                {12, "Декабрь"}
            };

            dataGridView2.Columns["Month"].HeaderText = "Месяц";
            dataGridView2.Columns["Profit"].HeaderText = "Прибыль";

            foreach (DataRow row in ProfitByMonth.Rows)
            {
                int month = Convert.ToInt32(row["Month"]);
                row["Month"] = month;
            }

            chart2.Series.Clear();
            chart2.ChartAreas.Clear();
            chart2.Titles.Clear();
            chart2.ChartAreas.Add(new ChartArea("SalesChart"));

            Series series = new Series("Прибыль");
            series.ChartType = SeriesChartType.Column;

            foreach (DataRow row in ProfitByMonth.Rows)
            {
                int month = Convert.ToInt32(row["Month"]);
                series.Points.AddXY(monthNames[month], Convert.ToDouble(row["Profit"]));
            }

            chart2.Series.Add(series);
            chart2.ChartAreas[0].AxisX.Interval = 1;
            chart2.ChartAreas[0].AxisX.Title = "Месяц";
            chart2.ChartAreas[0].AxisY.Title = "Прибыль";

            chart2.Titles.Add("Прибыль за период");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DataTable salesBySalers = GetSalesBySalers(dateTimePicker3.Value, dateTimePicker4.Value);
            dataGridView1.DataSource = salesBySalers;
            dataGridView1.Columns["Salesperson"].HeaderText = "Кассир";
            dataGridView1.Columns["TotalSales"].HeaderText = "Выручка";

            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.Titles.Clear();
            chart1.ChartAreas.Add(new ChartArea("SalesChart"));

            Series series = new Series("Выручка");
            series.ChartType = SeriesChartType.Column;

            foreach (DataRow row in salesBySalers.Rows)
            {
                series.Points.AddXY(row["Salesperson"].ToString(), Convert.ToDouble(row["TotalSales"]));
            }

            chart1.Series.Add(series);
            chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.Title = "Кассир";
            chart1.ChartAreas[0].AxisY.Title = "Выручка";

            chart1.Titles.Add("Заработок");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DataTable profitByGoods = GetProfitByGoods(dateTimePicker5.Value, dateTimePicker6.Value);
            dataGridView3.DataSource = profitByGoods;
            dataGridView3.Columns["Goods"].HeaderText = "Товары";
            dataGridView3.Columns["Profit"].HeaderText = "Выручка";

            chart3.Series.Clear();
            chart3.ChartAreas.Clear();
            chart3.Titles.Clear();
            chart3.ChartAreas.Add(new ChartArea("SalesChart"));

            Series series = new Series("Выручка");
            series.ChartType = SeriesChartType.Column;

            foreach (DataRow row in profitByGoods.Rows)
            {
                series.Points.AddXY(row["Goods"].ToString(), Convert.ToDouble(row["Profit"]));
            }

            chart3.Series.Add(series);
            chart3.ChartAreas[0].AxisX.Interval = 1;
            chart3.ChartAreas[0].AxisX.Title = "Товар";
            chart3.ChartAreas[0].AxisY.Title = "Выручка";

            chart3.Titles.Add("Прибыльность товаров");
        }

        private DataTable CalculateTotalSales(DateTime startDate, DateTime endDate)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Month", typeof(int));
            table.Columns.Add("Profit", typeof(decimal));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT DATEPART(MONTH, S.date_sale) AS Month, SUM(S.count * G.price) AS Profit " +
                               "FROM Sales S INNER JOIN Goods G ON S.product_id = G.product_id " +
                               "WHERE S.date_sale BETWEEN @startDate AND @endDate " +
                               "GROUP BY DATEPART(MONTH, S.date_sale)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        table.Rows.Add(reader["Month"], reader["Profit"]);
                    }
                }
            }

            return table;
        }

        private DataTable GetSalesBySalers(DateTime startDate, DateTime endDate)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Salesperson", typeof(string));
            table.Columns.Add("TotalSales", typeof(decimal));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT E.employee_name + ' ' + E.employee_lastname AS Salesperson, SUM(S.count * G.price) AS TotalSales FROM Sales S " +
                               "INNER JOIN Goods G ON S.product_id = G.product_id " +
                               "INNER JOIN Employees E ON S.employee_id = E.employee_id " +
                               "WHERE S.date_sale BETWEEN @startDate AND @endDate " +
                               "GROUP BY E.employee_id, E.employee_name, E.employee_lastname";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        table.Rows.Add(reader["Salesperson"], reader["TotalSales"]);
                    }
                }
            }

            return table;
        }

        private DataTable GetProfitByGoods(DateTime startDate, DateTime endDate)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Goods", typeof(string));
            table.Columns.Add("Profit", typeof(decimal));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT G.product_name AS Good, SUM(S.count * G.price) AS Profit FROM Sales S " +
                               "INNER JOIN Goods G ON S.product_id = G.product_id " +
                               "WHERE S.date_sale BETWEEN @startDate AND @endDate " +
                               "GROUP BY G.product_name";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        table.Rows.Add(reader["Good"], reader["Profit"]);
                    }
                }
            }

            return table;
        }
    }
}
