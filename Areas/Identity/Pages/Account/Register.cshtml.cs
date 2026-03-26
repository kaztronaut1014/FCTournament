// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using FCTournament.Models;
using FCTournament.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace FCTournament.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        private readonly IPlayerRepository _playerRepo;
        private readonly EFManagerRepository _managerRepo;
        private readonly EFOrganizerRepository _organizerRepo;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IPlayerRepository playerRepo,
            EFManagerRepository managerRepo,
            EFOrganizerRepository organizerRepo)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _playerRepo = playerRepo;
            _managerRepo = managerRepo;
            _organizerRepo = organizerRepo;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            public string FullName { get; set; }
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Vai trò")]
            public List<string> SelectedRoles { get; set; } = new List<string>();
        }
        public List<SelectListItem> RoleList { get; set; }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // 1. KIỂM TRA VÀ TẠO ROLE TỰ ĐỘNG (Cách này an toàn tuyệt đối, thiếu cái nào nó tạo cái đó)
            if (!await _roleManager.RoleExistsAsync(SD.Role_Admin)) await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
            if (!await _roleManager.RoleExistsAsync(SD.Role_Player)) await _roleManager.CreateAsync(new IdentityRole(SD.Role_Player));
            if (!await _roleManager.RoleExistsAsync(SD.Role_Organizer)) await _roleManager.CreateAsync(new IdentityRole(SD.Role_Organizer));
            if (!await _roleManager.RoleExistsAsync(SD.Role_Manager)) await _roleManager.CreateAsync(new IdentityRole(SD.Role_Manager));
            if (!await _roleManager.RoleExistsAsync(SD.Role_Referee)) await _roleManager.CreateAsync(new IdentityRole(SD.Role_Referee));
            if (!await _roleManager.RoleExistsAsync(SD.Role_Viewer)) await _roleManager.CreateAsync(new IdentityRole(SD.Role_Viewer));

            // 2. BỐC 4 ROLE RA GIAO DIỆN CHO NGƯỜI DÙNG CHỌN
            RoleList = _roleManager.Roles
                .Where(x => x.Name == SD.Role_Player ||
                            x.Name == SD.Role_Organizer ||
                            x.Name == SD.Role_Manager)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }).ToList();

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();

                user.FullName = Input.FullName;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var rolesToAdd = Input.SelectedRoles != null ? Input.SelectedRoles.Distinct().ToList() : new List<string>();

                    if (rolesToAdd.Any())
                    {
                        // 1. Gán quyền vào Identity (Bảng AspNetUserRoles)
                        await _userManager.AddToRolesAsync(user, rolesToAdd);

                        // 2. TẠO HỒ SƠ BẰNG REPOSITORY
                        foreach (var role in rolesToAdd)
                        {
                            if (role == SD.Role_Organizer)
                            {
                                // Gọi hàm từ EFOrganizerRepository
                                await _organizerRepo.AddOrganizerAsync(new Organizer
                                {
                                    ApplicationUserId = user.Id,
                                    SubscriptionId = 2 // Mặc định gói Free
                                });
                            }
                            else if (role == SD.Role_Player)
                            {
                                // Gọi hàm từ EFPlayerRepository
                                await _playerRepo.AddPlayerAsync(new Player
                                {
                                    ApplicationUserId = user.Id,
                                });
                            }
                            else if (role == SD.Role_Manager)
                            {
                                // Gọi hàm từ EFManagerRepository
                                await _managerRepo.AddManagerAsync(new Manager
                                {
                                    ApplicationUserId = user.Id,
                                });
                            }
                        }
                    }
                    else
                    {
                        // Không tick gì thì gán Viewer
                        await _userManager.AddToRoleAsync(user, SD.Role_Viewer);
                    }

                    // ... (Code sinh token và gửi mail bên dưới giữ nguyên) ...

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            RoleList = _roleManager.Roles
                .Where(x => x.Name == SD.Role_Player ||
                            x.Name == SD.Role_Organizer ||
                            x.Name == SD.Role_Manager)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                }).ToList();

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
