using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Agent.Data;
using Agent.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using CsvHelper;
using System.Globalization;
using X.PagedList;
using Microsoft.AspNetCore.Identity;

namespace Agent.Controllers
{
    public class MachineController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private int pageSize = 10;

        
        public MachineController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Machined  
        public async Task<IActionResult> Index( string machineInfoAgentName, DateTime fromDate, DateTime toDate, int Page = 1)
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole(RoleSeed.Keeper.ToString()) || User.IsInRole(RoleSeed.Admin.ToString())))//確認身分
            {
                //return View(await _context.MachineInfo.ToListAsync());
                if (fromDate == new DateTime(1, 1, 1))
                {
                    fromDate = DateTime.Today.AddMonths(-1);
                }
                if (toDate == new DateTime(1, 1, 1))
                {
                    toDate = DateTime.Today.AddMonths(1);
                }

                
                IQueryable<string> agentQuery = from m in _context.MachineInfo
                                                orderby m.AgentName
                                                select m.AgentName;               

                var machines = from m in _context.MachineInfo where (m.DeliveryDate >= fromDate && m.DeliveryDate <= toDate) select m;


                //var machines = from m in _context.MachineInfo  select m;
                DateTime dt = new DateTime(1, 1, 1);



                if (!string.IsNullOrEmpty(machineInfoAgentName))
                {
                    machines = machines.Where(x => x.AgentName == machineInfoAgentName);
                }


                //var machineInfos = await machines.OrderBy(p => p.MachineNo).ToPagedListAsync(Page, pageSize);
                //return View(machineInfos);

                var machineInfoVM = new MachineInfoViewModel
                {
                    AgentNames = new SelectList(await agentQuery.Distinct().ToListAsync()),
                    //MachineInfos = await machines.ToListAsync()
                    MachineInfos = await machines.OrderBy(p => p.MachineNo).ToPagedListAsync(Page, pageSize),
                    MachineInfoAgentName = machineInfoAgentName,
                    FromDate = fromDate,
                    ToDate = toDate,
                };

                return View(machineInfoVM);
            }
            return View();
        }

        [HttpPost]
        //public async Task<IActionResult> Index(List<IFormFile> selFile)//變數名稱(selFile) 必須與 html name 一致 name="selFile"
        public async Task<IActionResult> Index(IFormFile selFile)//變數名稱(selFile) 必須與 html name 一致 name="selFile"
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole(RoleSeed.Keeper.ToString()) || User.IsInRole(RoleSeed.Admin.ToString())))//確認身分
            {

                List<MachineInfo> customers = new List<MachineInfo>();
                //if (selFile.Count == 1)
                //customers = await TaskDoMethod(selFile[0]);
                customers = await TaskDoMethod(selFile);

                if (customers.Count > 0)
                {
                    customers.Select(c => { c.Id = 0; return c; }).ToArray();
                    //vip Daizen PostgresSql 不能指定ID(需要ID==0)，否則自動編號會混亂，SQLite無此問題           
                    try
                    {
                        await _context.MachineInfo.AddRangeAsync(customers);
                        await _context.SaveChangesAsync();
                        ViewBag.Message = "OK";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = ex.Message + "\n" + ex.InnerException.Message;
                        return View(customers);
                    }

                }
                else
                {
                    ViewBag.Message = "NG";
                    return View(customers);
                }
            }
            return RedirectToAction(nameof(Index));
        }
        

        private Task<List<MachineInfo>> TaskDoMethod(IFormFile formFile)
        {
            return Task.Run(() =>
            {
                List<MachineInfo> customers = new List<MachineInfo>();

                if (formFile != null)
                {
                    string csvData = string.Empty;

                    using (var reader = new StreamReader(formFile.OpenReadStream()))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        try
                        {
                            customers = csv.GetRecords<MachineInfo>().ToList();
                            //csv.Context.RegisterClassMap<CustomerModelMap>(); //ok //use map (class in the CustomerModel.cs)                       
                            //customers = csv.GetRecords<CustomerModel>().ToList();
                        }
                        catch { };
                    }
                }
                return customers;
            });

        }

        // GET: Machine/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineInfo = await _context.MachineInfo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (machineInfo == null)
            {
                return NotFound();
            }

            return View(machineInfo);
        }

        // GET: Machine/Create
        public async Task<IActionResult> Create()
        {           
           
            List<string> lstAgent = new List<string>();           
           foreach (var m in await _userManager.Users.ToListAsync())
            {
                //if (_userManager.GetRolesAsync(m).Result.FirstOrDefault() == "Agent") //太慢
                if (m.UserName!="管理者" && m.UserName != "維護者" && m.UserName != "代理商") 
                    lstAgent.Add(m.UserName);
            }
            IQueryable<string> agentQuery = lstAgent.AsQueryable();
            MachineInfoCreate machineInfo = new MachineInfoCreate();
            machineInfo.AgentNames = new SelectList(agentQuery.Distinct().ToList());
            return View(machineInfo);
        }

        // POST: Machine/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MachineNo,AgentId,AgentName,DeliveryDate")] MachineInfoCreate machineInfoCreate)
        {

            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated && (User.IsInRole(RoleSeed.Keeper.ToString()) || User.IsInRole(RoleSeed.Admin.ToString())))//確認身分
                {
                    var _agentId = _userManager.Users.Where(w => w.UserName == machineInfoCreate.AgentName).Select(s => s.Email).FirstOrDefault();

                    MachineInfo machineInfo = new MachineInfo
                    {
                        Id = 0,
                        AgentId = _agentId,
                        AgentName = machineInfoCreate.AgentName,
                        MachineNo = machineInfoCreate.MachineNo,
                        DeliveryDate = machineInfoCreate.DeliveryDate
                    };

                    _context.Add(machineInfo);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(machineInfoCreate);
        }

        // GET: Machine/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineInfo = await _context.MachineInfo.FindAsync(id);
            if (machineInfo == null)
            {
                return NotFound();
            }
            return View(machineInfo);
        }

        // POST: Machine/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MachineNo,AgentId,AgentName,DeliveryDate")] MachineInfo machineInfo)
        {
            if (id != machineInfo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (User.Identity.IsAuthenticated && (User.IsInRole(RoleSeed.Keeper.ToString()) || User.IsInRole(RoleSeed.Admin.ToString())))//確認身分
                {
                    try
                    {
                        _context.Update(machineInfo);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!MachineInfoExists(machineInfo.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(machineInfo);
        }

        // GET: Machine/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var machineInfo = await _context.MachineInfo
                .FirstOrDefaultAsync(m => m.Id == id);
            if (machineInfo == null)
            {
                return NotFound();
            }

            return View(machineInfo);
        }

        // POST: Machine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.Identity.IsAuthenticated && (User.IsInRole(RoleSeed.Keeper.ToString()) || User.IsInRole(RoleSeed.Admin.ToString())))//確認身分
            {
                var machineInfo = await _context.MachineInfo.FindAsync(id);
                _context.MachineInfo.Remove(machineInfo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool MachineInfoExists(int id)
        {
            return _context.MachineInfo.Any(e => e.Id == id);
        }
    }
}
