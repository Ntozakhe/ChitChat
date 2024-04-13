using ChitChat.Data;
using ChitChat.Models;
using ChitChat.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChitChat.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailService;

        public CategoriesController(ApplicationDbContext context,
                                     UserManager<AppUser> userManager,
                                     IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;

        }

        // GET: Categories
        [Authorize]
        public async Task<IActionResult> Index(string swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            string appUserId = _userManager.GetUserId(User)!;

            var categories = await _context.Categories.Where(c => c.AppUserId == appUserId)
                                                .Include(c => c.AppUser)
                                                .ToListAsync();
            return View(categories);
        }

        [Authorize]
        public async Task<IActionResult> EmailCategory(int id)
        {
            string appUserId = _userManager.GetUserId(User)!;
            Category category = await _context.Categories
                                                .Include(c => c.Contacts)
                                                .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);
            List<string> emails = category.Contacts.Select(c => c.Email).ToList();

            EmailData emailData = new EmailData()
            {
                GroupName = category.Name,
                EmailAddress = String.Join(";", emails),
                Subject = $"Goup Message: {category.Name}"
            };
            EmailCategoryViewModel model = new EmailCategoryViewModel()
            {
                Contacts = category.Contacts.ToList(),
                EmailData = emailData
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EmailCategory(EmailCategoryViewModel ecvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //send the email
                    await _emailService.SendEmailAsync(ecvm.EmailData.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    return RedirectToAction("Index", "Categories", new { swalMessage = "Success: Email Sent!" });
                }
                catch
                {
                    return RedirectToAction("Index", "Categories", new { swalMessage = "Error: Email Send failed!" });
                    throw;
                }
            }


            return View(ecvm);
        }




        // GET: Categories/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AppUserId")] Category category)
        {
            ModelState.Remove("AppUserId");
            if (ModelState.IsValid)
            {
                string appUserId = _userManager.GetUserId(User)!;
                category.AppUserId = appUserId;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string appUserId = _userManager.GetUserId(User)!;

            var category = await _context.Categories.Where(c => c.Id == id && c.AppUserId == appUserId)
                                                     .FirstOrDefaultAsync();
            //if someone tries to enter an id thats not asscoiated with the app user and category,
            //it would return a not found
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,AppUserId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //make sure the incoming AppUserId is the currently logged in person
                    string appUserId = _userManager.GetUserId(User)!;
                    category.AppUserId = appUserId;//on save it would be applied to the correct user.
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string appUserId = _userManager.GetUserId(User)!;
            var category = await _context.Categories
                                        .FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string appUserId = _userManager.GetUserId(User)!;
            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.AppUserId == appUserId);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
