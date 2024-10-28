using Microsoft.AspNetCore.Mvc;
using tutorial.Data;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using tutorial.Models;
using System.Globalization;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Net.Mime.MediaTypeNames;

namespace tutorial.Controllers
{
    public class CompleteQController : Controller
    {
        private readonly DataContext _context;

        public CompleteQController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public ActionResult Queue(int page = 1, int pageSize = 1000)
        {
            var branches = _context.Queue_Branch
                                   .Select(b => new SelectListItem
                                   {
                                       Value = b.BranchID.ToString(),
                                       Text = b.BranchID + " : " + b.BranchName
                                   }).ToList();
            ViewBag.BranchID = new SelectList(branches, "Value", "Text");

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
            ViewBag.StartMonth = new SelectList(months, "Value", "Text");
            ViewBag.EndMonth = new SelectList(months, "Value", "Text");

            var years = new List<SelectListItem>
        {
            new SelectListItem { Value = "2022", Text = "2022" },
            new SelectListItem { Value = "2023", Text = "2023" },
            new SelectListItem { Value = "2024", Text = "2024" }
        };
            ViewBag.StartYear = new SelectList(years, "Value", "Text");
            ViewBag.EndYear = new SelectList(years, "Value", "Text");

            try
            {

                var Maindata = _context.CompleteQ
           .Where(q => q.QNumber != null && q.QBegin.HasValue && q.QEnd.HasValue)
           .Select(q => new CompleteQ
           {
               QNumber = q.QNumber ?? "N/A",
               QPress = q.QPress.HasValue ? q.QPress.Value : DateTime.MinValue, 
               QBegin = q.QBegin.HasValue ? q.QBegin.Value : DateTime.MinValue, 
               QEnd = q.QEnd.HasValue ? q.QEnd.Value : DateTime.MinValue,      
               CounterID = q.CounterID ?? "N/A",
               UserID = q.UserID ?? "N/A",
               QStatus = q.QStatus ?? "N/A",
               BranchID = q.BranchID ?? "N/A",
               ServiceGroupID = q.ServiceGroupID ?? "N/A"
           })
           .OrderBy(q => q.QPress)
           .ToList();

                var formattedData= Maindata.Select(q => new CompleteQ
                {
                    QNumber = q.QNumber,
                    QPress = q.QPress,
                    QBegin = q.QBegin,
                    QEnd = q.QEnd,
                    CounterID = q.CounterID,
                    UserID = q.UserID,
                    QStatus = q.QStatus,
                    BranchID = q.BranchID,
                    ServiceGroupID = q.ServiceGroupID,
                    TimeToBeginFormatted = q.QBegin.HasValue && q.QPress.HasValue ?
    (q.QBegin.Value - q.QPress.Value).TotalSeconds >= 0 ?
    TimeSpan.FromSeconds((q.QBegin.Value - q.QPress.Value).TotalSeconds).ToString(@"hh\:mm\:ss") : "00:00:00" : "00:00:00",

                    TimeToEndFormatted = q.QEnd.HasValue && q.QPress.HasValue ?
    (q.QEnd.Value - q.QPress.Value).TotalSeconds >= 0 ?
    TimeSpan.FromSeconds((q.QEnd.Value - q.QPress.Value).TotalSeconds).ToString(@"hh\:mm\:ss") : "00:00:00" : "00:00:00"
                }).ToList();

                int totalRecords = Maindata.Count();
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View(formattedData);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "เกิดข้อผิดพลาด: " + ex.Message;
                return View("Queue", new List<CompleteQ>());
            }
        }



        [HttpPost]
        public ActionResult GetDataByDateRange(string branchID, string startmonth, string endmonth, int year,int startyear, int endyear, int page = 1, int pageSize = 1000)
        {
            try
            {
                var branches = _context.Queue_Branch
                                       .Select(b => new SelectListItem
                                       {
                                           Value = b.BranchID.ToString(),
                                           Text = b.BranchID + " : " + b.BranchName
                                       }).ToList();
                ViewBag.BranchID = new SelectList(branches, "Value", "Text");

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
                ViewBag.StartMonth = new SelectList(months, "Value", "Text");
                ViewBag.EndMonth = new SelectList(months, "Value", "Text");

                var years = new List<SelectListItem>
        {
            new SelectListItem { Value = "2022", Text = "2022" },
            new SelectListItem { Value = "2023", Text = "2023" },
            new SelectListItem { Value = "2024", Text = "2024" }
        };
                ViewBag.StartYear = new SelectList(years, "Value", "Text");
                ViewBag.EndYear = new SelectList(years, "Value", "Text");

                Debug.WriteLine($"BranchID: {branchID}, StartMonth: {startmonth}, EndMonth: {endmonth}, StartYear: {startyear}, EndYear: {endyear}");

                if (int.TryParse(startmonth, out int startMonthNum) && int.TryParse(endmonth, out int endMonthNum))
                {
                    DateTime startDate = new DateTime(startyear, startMonthNum, 1);
                    DateTime endDate = new DateTime(endyear, endMonthNum, 1).AddMonths(1).AddDays(-1);

                    Debug.WriteLine($"StartDate: {startDate}, EndDate: {endDate}");

                    var Maindata = _context.CompleteQ
                                   .Where(q => q.QPress >= startDate && q.QPress <= endDate &&
                                               q.BranchID.Trim() == branchID.Trim())
                                   .Select(q => new CompleteQ
                                   {
                                       QNumber = q.QNumber,
                                       QPress = q.QPress,
                                       QBegin = q.QBegin,
                                       QEnd = q.QEnd,
                                       CounterID = q.CounterID,
                                       UserID = q.UserID,
                                       QStatus = q.QStatus,
                                       BranchID = q.BranchID,
                                       ServiceGroupID = q.ServiceGroupID
                                   })
                                   .ToList();

                    var formattedData = Maindata.Select(q => new CompleteQ
                    {
                        QNumber = q.QNumber,
                        QPress = q.QPress,
                        QBegin = q.QBegin,
                        QEnd = q.QEnd,
                        CounterID = q.CounterID,
                        UserID = q.UserID,
                        QStatus = q.QStatus,
                        BranchID = q.BranchID,
                        ServiceGroupID = q.ServiceGroupID,
                        TimeToBeginFormatted = q.QBegin.HasValue && q.QPress.HasValue ?
                                               ((int)(q.QBegin.Value - q.QPress.Value).TotalMinutes).ToString("00") + ":" +
                                               (q.QBegin.Value - q.QPress.Value).Seconds.ToString("00") : "00:00",
                        TimeToEndFormatted = q.QEnd.HasValue && q.QPress.HasValue ?
                                             ((int)(q.QEnd.Value - q.QPress.Value).TotalMinutes).ToString("00") + ":" +
                                             (q.QEnd.Value - q.QPress.Value).Seconds.ToString("00") : "00:00"
                    }).ToList();

                    if (formattedData.Any())
                    {
                        return View("Queue", formattedData);
                    }
                    else
                    {
                        ViewBag.Message = $"ไม่พบข้อมูลในสาขา {branchID} ช่วงวันที่ {startDate:dd/MM/yyyy} ถึง {endDate:dd/MM/yyyy}";
                        return View("Queue", new List<CompleteQ>());
                    }
                }
                else
                {
                    ViewBag.Message = "เดือนไม่ถูกต้องหรือปีไม่ถูกต้อง";
                    return View("Queue", new List<CompleteQ>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "เกิดข้อผิดพลาด: " + ex.Message;
                return View("Queue", new List<CompleteQ>());
            }
        }
    }
}
