using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GameLoggd.Models;
using System.Security.Claims;

namespace GameLoggd.Controllers
{
    public class CasinoController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public CasinoController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("/casino")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewBag.Credits = user.Credits;
                
                var now = DateTime.UtcNow;
                var lastBonus = user.LastDailyBonus;
                
                // Bonus available if last bonus was before today (UTC)
                // Using simple 24h day check: if Date is different
                bool isBonusAvailable = lastBonus == null || lastBonus.Value.Date < now.Date;
                
                ViewBag.BonusAvailable = isBonusAvailable;
                
                if (!isBonusAvailable)
                {
                    // Time until midnight UTC
                    var tomorrow = now.Date.AddDays(1);
                    var timeUntil = tomorrow - now;
                    ViewBag.NextBonusTime = timeUntil;
                }
            }
            else
            {
                 ViewBag.Credits = 0;
                 ViewBag.BonusAvailable = false;
            }

            return View();
        }

        [HttpPost("/casino/claim-bonus")]
        public async Task<IActionResult> ClaimBonus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var now = DateTime.UtcNow;
            if (user.LastDailyBonus != null && user.LastDailyBonus.Value.Date >= now.Date)
            {
                return BadRequest("Bonus already claimed for today.");
            }

            user.Credits += 1000;
            user.LastDailyBonus = now;
            await _userManager.UpdateAsync(user);

            return Json(new { success = true, credits = user.Credits });
        }
        
        [HttpGet("/casino/balance")]
        public async Task<IActionResult> GetBalance()
        {
             var user = await _userManager.GetUserAsync(User);
             if (user == null) return Unauthorized();
             return Json(new { credits = user.Credits });
        }
    }
}
