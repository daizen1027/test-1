using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;//ToListAsync
using Agent.Models;

namespace Agent.Controllers
{
    public class AccountController : Controller
    {      
        private readonly UserManager<IdentityUser> _userManager;
        
        public AccountController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {            
            var users = await _userManager.Users.ToListAsync();
            var userViewModel = new List<PersonInfo>();
            foreach (var user in users)
            {
                var _person = new PersonInfo();
                _person.UserId = user.Email;
                _person.UserName = user.UserName;                
                _person.Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
                userViewModel.Add(_person);
            }

            
            return View(userViewModel);
        }

        /*
        [HttpPost]
        public async Task<IActionResult> AddRole(string roleName)
        {
            if (roleName != null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
            }
            return RedirectToAction("Index");
        }
        */

        public async Task<IActionResult> Delete(string id)//?
        {
            if (id == null || id=="")
            {
                return NotFound();
            }
            var user = await _userManager.FindByEmailAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            PersonInfo _person = new PersonInfo();
            _person.UserId = user.Email;
            _person.UserName = user.UserName;
            _person.Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
            return View(_person);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            //var movie = await _context.Movie.FindAsync(id);
            //_context.Movie.Remove(movie);
            //await _context.SaveChangesAsync();

            if (User.Identity.IsAuthenticated && (User.IsInRole(RoleSeed.Admin.ToString())))//確認身分
            {
                var user = await _userManager.FindByEmailAsync(id);
                if (user == null)
                {
                    return NotFound($"Unable to load user with ID .");
                }

                var result = await _userManager.DeleteAsync(user);
                var userId = await _userManager.GetUserIdAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Unexpected error occurred deleting user with ID '{userId}'.");
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
