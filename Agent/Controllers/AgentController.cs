using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agent.Models;
using Microsoft.Extensions.Localization;
using Agent.Middlewares;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Agent.Data;

namespace Agent.Controllers
{
    //[MiddlewareFilter(typeof(CultureMiddleware))]
    public class AgentController : Controller
    {
        //private readonly IStringLocalizer _localizer;

        public UserManager<IdentityUser> _userManager;

        private readonly ApplicationDbContext _context;

        [BindProperty]
        public MachineLock machineLock { get; set; }

        public string role { get; set; }

        //public AgentController(IStringLocalizer<HomeController> localizer , UserManager<IdentityUser> userManager, ApplicationDbContext context)
        public AgentController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            //_localizer = localizer;
            _userManager = userManager;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //string ss = $"{_localizer["Hello"]}";
            System.Security.Claims.ClaimsPrincipal currentUser = this.User;           
            var user =  await _userManager.GetUserAsync(currentUser);
            
            if (user!=null)
                role = await _userManager.GetPhoneNumberAsync(user);
            return View();
            /*
            return Content($"CurrentCulture: {CultureInfo.CurrentCulture.Name}\r\n"
                         + $"CurrentUICulture: {CultureInfo.CurrentUICulture.Name}\r\n"
                         + $"{_localizer["Hello"]}");
            */
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(MachineLock machineLock )
        {
            /*
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            */
            machineLock.TypeUnlock = Request.Form["unlock_type"];
            if (User.Identity.IsAuthenticated && (User.IsInRole(RoleSeed.Admin.ToString()) || User.IsInRole(RoleSeed.Agent.ToString())))
            {

                MachineInfo machine = (from m in _context.MachineInfo where (m.MachineNo == machineLock.CodeMac) select m).FirstOrDefault();

                if (machine == null)
                    machineLock.CodeUnlock = "機器碼錯誤(Machine_Code_Error)";
                else
                {
                    if (User.Identity.Name == machine.AgentName || User.IsInRole(RoleSeed.Admin.ToString()))                    
                        machineLock.CodeUnlock = await doMethod(machineLock.CodeMac);                    
                    else
                        machineLock.CodeUnlock = "無權限(Permission_denied)";
                }
                return View(machineLock);
            }
            return View(machineLock);

        }

        

        private Task<string> doMethod(string _mac)
        {

            string code = "";
            


            try
            {
                string ss = machineLock.CodeMac.Substring(2);
                string _type =  Request.Form["unlock_type"];
                int mac = Convert.ToInt32(machineLock.CodeMac.Substring(2));
                int code1 = Convert.ToInt32(machineLock.Code1);
                int code2 = Convert.ToInt32(machineLock.Code2);
                int code3 = Convert.ToInt32(machineLock.Code3);
                int codeAns;
                double dd;
                if (_type == "Forever")
                    dd = Math.Pow(code1, 2) * Math.Pow(code3, 3) + Math.Pow(code3, 3) + 543;
                else
                    dd = Math.Pow(code1, 2) * Math.Pow(code2, 3) + Math.Pow(code2, 3) + 345;
                Int64 orig = (Int64)dd % (Int64)Math.Pow(10, 6);
                codeAns = (int)(orig) + mac;
                code = codeAns.ToString();
            }
            catch { }

            return Task.Run(() => { return code; });
        }
    }

    
}
