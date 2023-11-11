using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Auth;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Net.Mail;
using System.IO;
using FactoryManagement.ModelView.HR;
using FactoryManagement.Helpers;
using FactoryManagement.Filters;

namespace FactoryManagement.Controllers
{
    [Authorize]

    public class AccountController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion

        #region Login methods
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            try
            {
                if (this.Request.IsAuthenticated)
                {
                    return this.RedirectToLocal(returnUrl);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return this.View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(UserLoginModelView model, string returnUrl)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (this.db.UserLogins.Where(m => m.UserName == model.UserName && m.Password == model.Password).Any())
                    {

                        var userLoginInfo = this.db.UserLogins.Where(m => m.UserName == model.UserName && m.Password == model.Password).FirstOrDefault();
                        userLoginInfo.HasResetPassword = false;
                        userLoginInfo.IsFirstLoginAfterReset = true;
                        int cuurencyId = this.db.FactoryInformations.Select(m => m.CurrencyId).FirstOrDefault();
                        string cuurencySymbol = this.db.CurrencyLists.Where(m => m.CurrencyId == cuurencyId).Select(m => m.CurrencySymbol).FirstOrDefault();

                        this.SignInUser(userLoginInfo, true);
                        HttpCookie cookie = new HttpCookie("CookieAdminInfo");
                        cookie.Values["userid"] = userLoginInfo.UserId.ToString();
                        cookie.Values["currencyId"] = cuurencyId.ToString();
                        cookie.Values["currencySymbol"] = cuurencySymbol;


                        if (userLoginInfo.UserId > 0)
                        {
                            var userInfo = this.db.View_UserLists.Where(m => m.UserId == userLoginInfo.UserId).FirstOrDefault();
                            cookie.Values["userfirstname"] = userInfo.FirstName.ToString();
                            cookie.Values["fulname"] = userInfo.UserName.ToString();
                            cookie.Values["userpic"] = userInfo.Picture.ToString();
                            cookie.Values["username"] = userLoginInfo.UserName.ToString();
                            cookie.Values["desig"] = userInfo.DesignationName.ToString();
                            cookie.Values["roleid"] = Convert.ToInt32(userInfo.RoleId).ToString();
                            cookie.Values["empid"] = (userInfo.EmpId).ToString();
                        }
                        else
                        {
                            cookie.Values["userfirstname"] = "Super";
                            cookie.Values["userpic"] = "admin.jpg";
                            cookie.Values["username"] = "SuperAdmin";
                            cookie.Values["roleid"] = "1";
                            cookie.Values["fulname"] = "";
                            cookie.Values["desig"] = "";
                            cookie.Values["empid"] = "";
                        }
                        cookie.Expires = DateTime.Now.AddYears(1);
                        Response.Cookies.Add(cookie);

                        int localTime = 0;
                        var cookieLT = HttpContext.Request.Cookies["CookieCostLT"];
                        if (cookieLT != null)
                        {
                            localTime = Convert.ToInt32(cookieLT.Value);
                        }
                        ConfigurationManager.AppSettings["localTime"] = localTime.ToString();
                        db.Entry(userLoginInfo).State = EntityState.Modified;
                        db.SaveChanges();
                        return this.RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }
            return this.View(model);
        }
        #endregion

        #region Log Out method.
        public ActionResult LogOff()
        {
            try
            {
                var c = new HttpCookie("CookieAdminInfo");
                var l = new HttpCookie("CookieCostLT");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);

                l.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(l);

                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                authenticationManager.SignOut();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return this.RedirectToAction("Login", "Account");
        }
        #endregion


        #region Helpers
        #region Sign In method.
        private void SignInUser(UserLogin username, bool isPersistent)
        {
            var claims = new List<Claim>();
            try
            {
                claims.Add(new Claim(ClaimTypes.Name, username.UserName));
                var claimIdenties = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
                var ctx = Request.GetOwinContext();
                var authenticationManager = ctx.Authentication;
                authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, claimIdenties);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #region Redirect to local method.
        private ActionResult RedirectToLocal(string returnUrl)
        {
            try
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    return this.Redirect(returnUrl);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return this.RedirectToAction("Dashboard", "Home");
        }
        #endregion
        #endregion



        #region Forgot Password
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        #endregion

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        
        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region  password change
        [AllowAnonymous]
        public ActionResult PasswordRecovery()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult PasswordRecovery(UserInformationModelView model)
        {
            View_UserLists info = db.View_UserLists.Where(m => m.EmailAddress == model.EmailAddress).FirstOrDefault();
            string url = "";
            string domain = Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host + (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
            url = domain + "/Account/SetNewPassword?q=" + MyExtensions.Encrypt("UserId=" + info.UserId);
            //@Html.EncodedParam("DisplayEmployeeDetails","HumanResource",new {UserId=item.UserId,isDisplay=false },null)
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("\\Images\\E-mailTemplate.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{ApplicantFullName}", info.UserName);
            body = body.Replace("{Url}", url);
            string mailAddress = info.EmailAddress;
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
            SmtpClient smtp = new SmtpClient();

            // mail.To.Add("testoffice4560@gmail.com");
            mail.To.Add(new MailAddress(mailAddress));
            mail.From = new MailAddress("info@issuetoday.org", "CostERP", System.Text.Encoding.UTF8);
            mail.Subject = "To Reset Password ";
            mail.SubjectEncoding = System.Text.Encoding.UTF8;
            mail.Body = body;
            mail.BodyEncoding = System.Text.Encoding.UTF8;
            mail.IsBodyHtml = true;
            mail.Priority = System.Net.Mail.MailPriority.High;

            SmtpClient client = new SmtpClient();
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("info@issuetoday.org", "45604560");
            client.Host = "cascara.arvixe.com";
            client.EnableSsl = false;
            client.Port = 587;
            client.Timeout = 18000000;
            try
            {
                client.Send(mail);
            }
            catch
            {
                client.Port = 25;
                client.Timeout = 18000000;
                try
                {
                    client.Send(mail);
                }
                catch
                {

                }
            }
            return RedirectToAction("PasswordChangeConfirmation");
        }


        [AllowAnonymous]
        public ActionResult PasswordChangeConfirmation()
        {
            return View();
        }
        [AllowAnonymous]
        [EncryptedActionParameter]
        public ActionResult SetNewPassword(long UserId)
        {
            UserLoginModelView model = new UserLoginModelView();
            model.UserId = UserId;
            return View(model);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SetNewPassword(UserLoginModelView model)
        {
            UserLogin login = db.UserLogins.Where(m => m.UserId == model.UserId).FirstOrDefault();
            login.Password = model.Password;
            login.UpdatedDate = now;
            login.UpdatedBy = model.UserId;
            login.HasResetPassword = true;
            login.IsFirstLoginAfterReset = false;
            db.UserLogins.Attach(login);
            db.Entry(login).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Login");
        }
        #endregion


        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        //private ActionResult RedirectToLocal(string returnUrl)
        //{
        //    if (Url.IsLocalUrl(returnUrl))
        //    {
        //        return Redirect(returnUrl);
        //    }
        //    return RedirectToAction("Index", "Home");
        //}

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}