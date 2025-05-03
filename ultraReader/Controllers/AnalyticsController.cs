using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ultraReader.Services;

namespace ultraReader.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IWebtoonService _webtoonService;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            IWebtoonService webtoonService)
        {
            _analyticsService = analyticsService;
            _webtoonService = webtoonService;
        }

        public async Task<IActionResult> Index()
        {
            var userActivitySummary = await _analyticsService.GetAverageUserActivityAsync();
            ViewBag.UserActivitySummary = userActivitySummary;
            
            var dailyActivity = await _analyticsService.GetDailyActivityAsync();
            ViewBag.DailyActivity = dailyActivity;
            
            var readingTimesByHour = await _analyticsService.GetReadingTimesByHourAsync();
            ViewBag.ReadingTimesByHour = readingTimesByHour;
            
            return View();
        }

        public async Task<IActionResult> PopulerWebtoonlar()
        {
            ViewBag.MostViewed = await _analyticsService.GetMostViewedWebtoonsAsync();
            ViewBag.MostFavorited = await _analyticsService.GetMostFavoritedWebtoonsAsync();
            ViewBag.MostCompleted = await _analyticsService.GetMostCompletedWebtoonsAsync();
            ViewBag.HighestRated = await _analyticsService.GetHighestRatedWebtoonsAsync();
            
            return View();
        }

        public async Task<IActionResult> TurAnalizleri()
        {
            ViewBag.WebtoonsByGenre = await _analyticsService.GetWebtoonsByGenreAsync();
            ViewBag.GenreTrends = await _analyticsService.GetGenreTrendsAsync();
            
            return View();
        }

        public async Task<IActionResult> KullaniciDavranislari()
        {
            var activeUsers = await _analyticsService.GetAverageUserActivityAsync();
            ViewBag.ActiveUsers = activeUsers;
            
            var readingTimesByHour = await _analyticsService.GetReadingTimesByHourAsync();
            ViewBag.ReadingTimesByHour = readingTimesByHour;
            
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> KullaniciDetay(string userId)
        {
            var userActivity = await _analyticsService.GetUserActivityAsync(userId);
            if (userActivity == null)
            {
                return NotFound();
            }
            
            return View(userActivity);
        }

        [HttpGet]
        public async Task<JsonResult> GetDailyActivityData()
        {
            var dailyActivity = await _analyticsService.GetDailyActivityAsync();
            return Json(dailyActivity);
        }

        [HttpGet]
        public async Task<JsonResult> GetWebtoonsByGenreData()
        {
            var webtoonsByGenre = await _analyticsService.GetWebtoonsByGenreAsync();
            return Json(webtoonsByGenre);
        }

        [HttpGet]
        public async Task<JsonResult> GetGenreTrendsData()
        {
            var genreTrends = await _analyticsService.GetGenreTrendsAsync();
            return Json(genreTrends);
        }

        [HttpGet]
        public async Task<JsonResult> GetReadingTimesByHourData()
        {
            var readingTimesByHour = await _analyticsService.GetReadingTimesByHourAsync();
            return Json(readingTimesByHour);
        }
    }
} 