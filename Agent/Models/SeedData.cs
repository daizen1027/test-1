using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace Agent.Models
{
    public enum RoleSeed
    {
        Agent,
        Keeper,
        Admin,
    }

    public static class SeedData
    {
        public static void SeedRoles(RoleManager<IdentityRole> _roleManager)//不能非同步，創立帳號時，若角色們未建立好，會異常
        {
            if (_roleManager.Roles.Any())
            {
                return;   // DB has been seeded
            }

            var lst = Enum.GetValues(typeof(RoleSeed)).Cast<RoleSeed>().ToList();

            string rr;
            foreach (var r in lst)
            {
                rr = r.ToString();
                //_roleManager.CreateAsync(new IdentityRole(rr));
                _roleManager.CreateAsync(new IdentityRole(rr)).Wait();


            }

            //IdentityRole role = new IdentityRole("Admin");
            //IdentityResult resultRole = _roleManager.CreateAsync(role).Result;
        }
        public static void SeedUsers(UserManager<IdentityUser> _userManager)
        {            

            if (_userManager.Users.Any())
            {
                return;   // DB has been seeded
            }

            List<SeedAccount> accounts = new List<SeedAccount>();
            accounts.Add(new SeedAccount("Admin", "管理者", RoleSeed.Admin, "Admin@"));
            accounts.Add(new SeedAccount("Keeper", "維護者", RoleSeed.Keeper, "Keeper@"));
            accounts.Add(new SeedAccount("Agent", "代理商", RoleSeed.Agent, "Agent@"));


            foreach (var act in accounts)
            {
                IdentityUser user = new IdentityUser();
                user.Email = act.UserId; //id
                user.UserName = act.UserName; //username
                //user.PhoneNumber = RoleSeed.Admin.ToString(); //role
                var RoleName = act.Role.ToString();
                var password = act.Password;

                
                IdentityResult result = _userManager.CreateAsync(user, password).Result;


                if (result.Succeeded)
                {
                    //GenerateEmailConfirmationTokenAsync不知如何不使用非同步，暫時關閉帳號確認 //options.SignIn.RequireConfirmedAccount = false;
                    //var _code = await _userManager.GenerateEmailConfirmationTokenAsync(user);//產生確認信件的Token OK                    
                    //_userManager.ConfirmEmailAsync(user, _code).Wait();  //使用Token確認信件 OK

                    _userManager.AddToRoleAsync(user, RoleName).Wait();

                    
                }


            }
        }

    }
    public class SeedAccount
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public RoleSeed Role { get; set; }

        public string Password { get; set; }

        public SeedAccount()
        {            
        }
        public SeedAccount(string userId,string username, RoleSeed role, string password)
        {
            UserId = userId;
            UserName = username;
            Role = role;
            Password = password;
        }

    }
}
