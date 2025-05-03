using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ultraReader.Services;
using System.Linq;

namespace ultraReader.Controllers
{
    public class WebtoonListController : Controller
    {
        private readonly IWebtoonService _webtoonService;

        public WebtoonListController(IWebtoonService webtoonService)
        {
            _webtoonService = webtoonService;
        }

        public async Task<IActionResult> Index()
        {
            var webtoons = await _webtoonService.GetAllWebtoonsAsync();
            return View(webtoons);
        }
    }
} 