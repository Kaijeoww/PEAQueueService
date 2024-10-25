using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using tutorial.Data;
using tutorial.Models;

namespace tutorial.Controllers
{
    public class QueueRawDataController : Controller
    {
        private readonly DataContext _context;
        private readonly ChonburiDataContext _CDataContext;

        public QueueRawDataController(DataContext context, ChonburiDataContext cDataContext)
        {
            _context = context;
            _CDataContext = cDataContext;
        }

        public ActionResult Queue2()
        {
            var model = PopulateDropdownLists();

            string connectionString1 = "Server=PeqnutN\\SQLEXPRESS; Database=QueueRawData; Trusted_Connection=True; TrustServerCertificate=True";
            string connectionString2 = "Server=PeqnutN\\SQLEXPRESS; Database=PEASmartQueue; Trusted_Connection=True; TrustServerCertificate=True";

            string queryQueueRawData = @"
    WITH TimeCalculations AS (
        SELECT 
            qb.BranchID,
            qb.BranchName, 
            DATEPART(YEAR, cq.QPress) AS Year,
            DATEPART(MONTH, cq.QPress) AS Month,
            COUNT(cq.QNumber) AS QNumberCount,
            AVG(DATEDIFF(SECOND, cq.QPress, cq.QEnd)) AS AvgProcessingSeconds,
            AVG(DATEDIFF(SECOND, cq.QPress, cq.QBegin)) AS AvgWaitingSeconds
        FROM 
            Queue_Branch qb
        INNER JOIN 
            CompleteQ cq ON qb.BranchID = cq.BranchID 
        GROUP BY 
            qb.BranchID,
            qb.BranchName, 
            DATEPART(YEAR, cq.QPress),
            DATEPART(MONTH, cq.QPress)
    )
    SELECT 
        BranchID,
        BranchName,
        Year,
        Month,
        QNumberCount,
        CONVERT(varchar, DATEADD(SECOND, AvgProcessingSeconds, 0), 108) AS AvgProcessingTime,
        CONVERT(varchar, DATEADD(SECOND, AvgWaitingSeconds, 0), 108) AS AvgWaitingTime,
        CONVERT(varchar, DATEADD(SECOND, 
            (AvgProcessingSeconds - AvgWaitingSeconds), 0), 108) AS ProcessingTimeAfterWaiting
    FROM 
        TimeCalculations
    WHERE 
        QNumberCount IS NOT NULL
    ORDER BY 
        BranchID;
    ";

            string queryPEASmartQueue = @"
    SELECT 
        qb.BranchID, 
        qb.BranchName, 
        DATEPART(YEAR, cq.QPress) AS Year,
        DATEPART(MONTH, cq.QPress) AS Month,
        COUNT(cq.QNumber) AS QNumberCount,
        AVG(DATEDIFF(SECOND, cq.QPress, cq.QEnd)) AS AvgProcessingSeconds,
        AVG(DATEDIFF(SECOND, cq.QPress, cq.QBegin)) AS AvgWaitingSeconds
    FROM 
        PEASmartQueue.dbo.CompleteQ cq
    JOIN 
        QueueRawData.dbo.Queue_Branch qb 
    ON 
        cq.BranchID = qb.BranchID
    GROUP BY 
        qb.BranchID, qb.BranchName, DATEPART(YEAR, cq.QPress), DATEPART(MONTH, cq.QPress)
    ORDER BY 
        qb.BranchID;
    ";

            DataTable dtQueueRawData = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString1))
            {
                SqlCommand command = new SqlCommand(queryQueueRawData, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                connection.Open();
                adapter.Fill(dtQueueRawData);
            }

            DataTable dtPEASmartQueue = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString2))
            {
                SqlCommand command = new SqlCommand(queryPEASmartQueue, connection);
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                connection.Open();
                adapter.Fill(dtPEASmartQueue);
            }

            var branchDictionary = new Dictionary<string, string>();
            foreach (DataRow row in dtQueueRawData.Rows)
            {
                string branchID = row["BranchID"].ToString();
                string branchName = row["BranchName"].ToString();
                branchDictionary[branchID] = branchName;
            }

            List<QueueView> queueData = new List<QueueView>();



            foreach (DataRow row in dtQueueRawData.Rows)
            {
                int qNumberCount = row["QNumberCount"] != DBNull.Value ? Convert.ToInt32(row["QNumberCount"]) : 0;
                if (qNumberCount > 0)
                {
                    int month = row["Month"] != DBNull.Value ? Convert.ToInt32(row["Month"]) : 0;
                    queueData.Add(new QueueView
                    {
                        BranchID = row["BranchID"]?.ToString(),
                        BranchName = row["BranchName"]?.ToString(),
                        Year = row["Year"] != DBNull.Value ? Convert.ToInt32(row["Year"]) : 0,
                        Month = month,
                        QNumberCount = qNumberCount,
                        AvgProcessingTime = row["AvgProcessingTime"]?.ToString() ?? "N/A",
                        AvgWaitingTime = row["AvgWaitingTime"]?.ToString() ?? "N/A",
                        ProcessingTimeAfterWaiting = row["ProcessingTimeAfterWaiting"]?.ToString() ?? "N/A"
                    });
                }
            }

            foreach (DataRow row in dtPEASmartQueue.Rows)
            {
                int qNumberCount = row["QNumberCount"] != DBNull.Value ? Convert.ToInt32(row["QNumberCount"]) : 0;
                if (qNumberCount > 0)
                {
                    int month = row["Month"] != DBNull.Value ? Convert.ToInt32(row["Month"]) : 0;
                    queueData.Add(new QueueView
                    {
                        BranchID = row["BranchID"]?.ToString(),
                        BranchName = row["BranchName"]?.ToString(),
                        Year = row["Year"] != DBNull.Value ? Convert.ToInt32(row["Year"]) : 0,
                        Month = month,
                        QNumberCount = qNumberCount,
                        AvgProcessingTime = row["AvgProcessingSeconds"] != DBNull.Value
                                            ? TimeSpan.FromSeconds(Convert.ToDouble(row["AvgProcessingSeconds"])).ToString(@"hh\:mm\:ss")
                                            : "N/A",
                        AvgWaitingTime = row["AvgWaitingSeconds"] != DBNull.Value
                                         ? TimeSpan.FromSeconds(Convert.ToDouble(row["AvgWaitingSeconds"])).ToString(@"hh\:mm\:ss")
                                         : "N/A",
                        ProcessingTimeAfterWaiting = row["AvgProcessingSeconds"] != DBNull.Value && row["AvgWaitingSeconds"] != DBNull.Value
                                                     ? TimeSpan.FromSeconds(Convert.ToDouble(row["AvgProcessingSeconds"]) - Convert.ToDouble(row["AvgWaitingSeconds"])).ToString(@"hh\:mm\:ss")
                                                     : "N/A"
                    });
                }
            }

            queueData = queueData.OrderBy(q => q.Year).ThenBy(q => q.Month).ToList();

            model.QueueData = queueData;
            return View("Queue2", model);
        }









        public ActionResult SummaryQ(int year, int month, string branchID)
        {
            var summaryQ = _context.CompleteQ
                .Where(q => q.QPress.HasValue
                    && q.QPress.Value.Year == year
                    && q.QPress.Value.Month == month
                    && q.BranchID == branchID)
                .OrderBy(q => q.QPress.Value.Month)
                .ThenBy(q => q.QPress)
                .Select(q => new CompleteQ
                {
                    QNumber = q.QNumber ?? "N/A",
                    QPress = q.QPress,
                    QBegin = q.QBegin,
                    QEnd = q.QEnd,
                    CounterID = q.CounterID ?? "N/A",
                    UserID = q.UserID ?? "N/A",
                    QStatus = q.QStatus ?? "N/A",
                    BranchID = q.BranchID ?? "N/A",
                    ServiceGroupID = q.ServiceGroupID ?? "N/A",
                    TimeToBeginFormatted = q.QBegin.HasValue && q.QPress.HasValue ?
                        ((int)(q.QBegin.Value - q.QPress.Value).TotalMinutes).ToString("00") + ":" +
                        (q.QBegin.Value - q.QPress.Value).Seconds.ToString("00") : "N/A",
                    TimeToEndFormatted = q.QEnd.HasValue && q.QPress.HasValue ?
                        ((int)(q.QEnd.Value - q.QPress.Value).TotalMinutes).ToString("00") + ":" +
                        (q.QEnd.Value - q.QPress.Value).Seconds.ToString("00") : "N/A"
                })
                .ToList();

            var summaryQFromPEASmartQueue = _CDataContext.CompleteQ
                .Where(q => q.QPress.HasValue
                    && q.QPress.Value.Year == year
                    && q.QPress.Value.Month == month
                    && q.BranchID == branchID)
                .OrderBy(q => q.QPress.Value.Month)
                .ThenBy(q => q.QPress)
                .Select(q => new CompleteQ
                {
                    QNumber = q.QNumber ?? "N/A",
                    QPress = q.QPress,
                    QBegin = q.QBegin,
                    QEnd = q.QEnd,
                    CounterID = q.CounterID ?? "N/A",
                    UserID = q.UserID ?? "N/A",
                    QStatus = q.QStatus ?? "N/A",
                    BranchID = q.BranchID ?? "N/A",
                    ServiceGroupID = q.ServiceGroupID ?? "N/A",
                    TimeToBeginFormatted = q.QBegin.HasValue && q.QPress.HasValue ?
                        ((int)(q.QBegin.Value - q.QPress.Value).TotalMinutes).ToString("00") + ":" +
                        (q.QBegin.Value - q.QPress.Value).Seconds.ToString("00") : "N/A",
                    TimeToEndFormatted = q.QEnd.HasValue && q.QPress.HasValue ?
                        ((int)(q.QEnd.Value - q.QPress.Value).TotalMinutes).ToString("00") + ":" +
                        (q.QEnd.Value - q.QPress.Value).Seconds.ToString("00") : "N/A"
                })
                .ToList();

            var combinedSummaryQ = summaryQ.Concat(summaryQFromPEASmartQueue).ToList();

            return View("SummaryQ", combinedSummaryQ);
        }





        private FilterMViewModel PopulateDropdownLists()
        {
            var branches = _context.Queue_Branch
                .Select(b => new SelectListItem
                {
                    Value = b.BranchID.ToString(),
                    Text = b.BranchID + " : " + b.BranchName
                }).ToList();

            var months = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "มกราคม" },
                new SelectListItem { Value = "2", Text = "กุมภาพันธ์" },
                new SelectListItem { Value = "3", Text = "มีนาคม" },
                new SelectListItem { Value = "4", Text = "เมษายน" },
                new SelectListItem { Value = "5", Text = "พฤษภาคม" },
                new SelectListItem { Value = "6", Text = "มิถุนายน" },
                new SelectListItem { Value = "7", Text = "กรกฎาคม" },
                new SelectListItem { Value = "8", Text = "สิงหาคม" },
                new SelectListItem { Value = "9", Text = "กันยายน" },
                new SelectListItem { Value = "10", Text = "ตุลาคม" },
                new SelectListItem { Value = "11", Text = "พฤศจิกายน" },
                new SelectListItem { Value = "12", Text = "ธันวาคม" }
            };

            var years = new List<SelectListItem>
            {
                new SelectListItem { Value = "2022", Text = "2022" },
                new SelectListItem { Value = "2023", Text = "2023" },
                new SelectListItem { Value = "2024", Text = "2024" }
            };

            return new FilterMViewModel
            {
                Branches = new SelectList(branches, "Value", "Text"),
                Months = new SelectList(months, "Value", "Text"),
                Years = new SelectList(years, "Value", "Text"),
                QueueData = new List<QueueView>() // แนะนำให้ตั้งค่าคุณสมบัตินี้เป็นลิสต์ว่าง
            };
        }



        [HttpGet]
        public ActionResult FilterM()
        {
            var model = PopulateDropdownLists();
            return View("Queue2", model);
        }



        [HttpPost]
        public ActionResult FilterM(string branchID, string startmonth, int? startyear, string endmonth, int? endyear, FilterMViewModel model)
        {
            model = PopulateDropdownLists();

            try
            {
                int startMonthNum = string.IsNullOrEmpty(startmonth) ? 1 : int.Parse(startmonth);
                int endMonthNum = string.IsNullOrEmpty(endmonth) ? 12 : int.Parse(endmonth);


                int selectedStartYear = startyear ?? DateTime.Now.Year;
                int selectedEndYear = endyear ?? DateTime.Now.Year;

                DateTime startDate = new DateTime(selectedStartYear, startMonthNum, 1);
                DateTime endDate = new DateTime(selectedEndYear, endMonthNum, DateTime.DaysInMonth(selectedEndYear, endMonthNum));


                var queryContext = _context.CompleteQ.Include(q => q.Queue_Branch).AsQueryable();


                if (!string.IsNullOrEmpty(branchID) && branchID != "All")
                {
                    queryContext = queryContext.Where(q => q.BranchID == branchID);
                }

                if (startyear.HasValue || endyear.HasValue || !string.IsNullOrEmpty(startmonth) || !string.IsNullOrEmpty(endmonth))
                {
                    queryContext = queryContext.Where(q => q.QPress >= startDate && q.QPress <= endDate);
                }

                var dataContext = queryContext.Select(q => new
                {
                    q.BranchID,
                    BranchName = q.Queue_Branch.BranchName,
                    QPress = q.QPress,
                    QEnd = q.QEnd,
                    QBegin = q.QBegin,
                    QNumber = q.QNumber
                }).OrderBy(q => q.QPress).AsNoTracking().ToList();

                var queryCDataContext = _CDataContext.CompleteQ.AsQueryable();

                if (!string.IsNullOrEmpty(branchID) && branchID != "All")
                {
                    queryCDataContext = queryCDataContext.Where(q => q.BranchID == branchID);
                }

                if (startyear.HasValue || endyear.HasValue || !string.IsNullOrEmpty(startmonth) || !string.IsNullOrEmpty(endmonth))
                {
                    queryCDataContext = queryCDataContext.Where(q => q.QPress >= startDate && q.QPress <= endDate);
                }

                var dataCDataContext = queryCDataContext.Select(q => new
                {
                    q.BranchID,
                    QPress = q.QPress,
                    QEnd = q.QEnd,
                    QBegin = q.QBegin,
                    QNumber = q.QNumber
                }).OrderBy(q => q.QPress).AsNoTracking().ToList();

                var branchNames = _context.Queue_Branch
                    .Where(b => dataCDataContext.Select(q => q.BranchID).Contains(b.BranchID))
                    .Select(b => new { b.BranchID, b.BranchName })
                    .ToList();


                model.QueueData = new List<QueueView>();

                var groupedDataContext = dataContext.GroupBy(q => new
                {
                    q.BranchID,
                    BranchName = q.BranchName,
                    Year = q.QPress.HasValue ? q.QPress.Value.Year : 0,
                    Month = q.QPress.HasValue ? q.QPress.Value.Month : 0
                })
                .Select(g => new QueueView
                {
                    BranchID = g.Key.BranchID,
                    BranchName = g.Key.BranchName,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    QNumberCount = g.Count(), 
                    AvgProcessingTime = TimeSpan.FromSeconds(g.Average(q => (q.QEnd.HasValue && q.QPress.HasValue) ? (q.QEnd.Value - q.QPress.Value).TotalSeconds : 0)).ToString(@"hh\:mm\:ss"), // แปลงเป็นรูปแบบเวลา
                    AvgWaitingTime = TimeSpan.FromSeconds(g.Average(q => (q.QBegin.HasValue && q.QPress.HasValue) ? (q.QBegin.Value - q.QPress.Value).TotalSeconds : 0)).ToString(@"hh\:mm\:ss"), // แปลงเป็นรูปแบบเวลา
                    ProcessingTimeAfterWaiting = TimeSpan.FromSeconds(
                        g.Average(q => (q.QEnd.HasValue && q.QPress.HasValue) ? (q.QEnd.Value - q.QPress.Value).TotalSeconds : 0) -
                        g.Average(q => (q.QBegin.HasValue && q.QPress.HasValue) ? (q.QBegin.Value - q.QPress.Value).TotalSeconds : 0)
                    ).ToString(@"hh\:mm\:ss")
                })
                .ToList();

                model.QueueData.AddRange(groupedDataContext);

                var groupedDataCDataContext = dataCDataContext.GroupBy(q => new
                {
                    q.BranchID,
                    Year = q.QPress.HasValue ? q.QPress.Value.Year : 0,
                    Month = q.QPress.HasValue ? q.QPress.Value.Month : 0
                })
                .Select(g => new QueueView
                {
                    BranchID = g.Key.BranchID,
                    BranchName = branchNames.FirstOrDefault(b => b.BranchID == g.Key.BranchID)?.BranchName ?? "N/A",
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    QNumberCount = g.Count(), 
                    AvgProcessingTime = TimeSpan.FromSeconds(g.Average(q => (q.QEnd.HasValue && q.QPress.HasValue) ? (q.QEnd.Value - q.QPress.Value).TotalSeconds : 0)).ToString(@"hh\:mm\:ss"), // แปลงเป็นรูปแบบเวลา
                    AvgWaitingTime = TimeSpan.FromSeconds(g.Average(q => (q.QBegin.HasValue && q.QPress.HasValue) ? (q.QBegin.Value - q.QPress.Value).TotalSeconds : 0)).ToString(@"hh\:mm\:ss"), // แปลงเป็นรูปแบบเวลา
                    ProcessingTimeAfterWaiting = TimeSpan.FromSeconds(
                        g.Average(q => (q.QEnd.HasValue && q.QPress.HasValue) ? (q.QEnd.Value - q.QPress.Value).TotalSeconds : 0) -
                        g.Average(q => (q.QBegin.HasValue && q.QPress.HasValue) ? (q.QBegin.Value - q.QPress.Value).TotalSeconds : 0)
                    ).ToString(@"hh\:mm\:ss") 
                })
                .ToList();

                model.QueueData.AddRange(groupedDataCDataContext);

                model.QueueData = model.QueueData.OrderBy(q => q.Year).ThenBy(q => q.Month).ToList();

                return View("Queue2", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "เกิดข้อผิดพลาด: " + ex.Message;
                model.QueueData = new List<QueueView>();
                return View("Queue2", model);
            }
        }
    }
}
