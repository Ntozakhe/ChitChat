using ChitChat.Data;
using ChitChat.Enums;
using ChitChat.Models;
using ChitChat.Models.ViewModels;
using ChitChat.Services.Interfacs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ChitChat.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IAddressBookService _addressBookService;
        private readonly IEmailSender _emailService;
        //we implement/inject the interface
        public ContactsController(ApplicationDbContext context,
                                  UserManager<AppUser> userManager,
                                  IImageService imageService,
                                  IAddressBookService addressBookService,
                                  IEmailSender emailService)
        {
            _context = context;
            _userManager = userManager;
            _imageService = imageService;
            _addressBookService = addressBookService;
            _emailService = emailService;
        }

        // GET: Contacts
        [Authorize]
        public IActionResult Index(int categoryId, string swalMessage = null)
        {
            ViewData["SwalMessage"] = swalMessage;
            var contacts = new List<Contact>(); //This is an empty list of contacts
            string appUserId = _userManager.GetUserId(User)!; //the person thats logged in.
            AppUser appUser = _context.Users
                                      .Include(c => c.Contacts)
                                      .ThenInclude(c => c.Categories)
                                      .FirstOrDefault(u => u.Id == appUserId)!;
            var catergories = appUser.Categories;
            if (categoryId == 0)
            {
                contacts = appUser.Contacts.OrderBy(c => c.LastName)
                                       .ThenBy(c => c.FirstName)
                                       .ToList();
            }
            else
            {
                contacts = appUser.Categories.FirstOrDefault(c => c.Id == categoryId)!
                                             .Contacts
                                             .OrderBy(c => c.LastName)
                                             .ThenBy(c => c.FirstName)
                                             .ToList();
            }
            ViewData["CategoryId"] = new SelectList(catergories, "Id", "Name", categoryId);
            return View(contacts);
        }


        [Authorize]
        public IActionResult SearchContacts(string searchString)
        {
            //lets grab our user Id
            string appUserId = _userManager.GetUserId(User)!;
            var contacts = new List<Contact>();
            //we want a list of contacts based on this logged in user.
            //Then filter by the search string

            //grab our contacts
            AppUser appUser = _context.Users
                        .Include(c => c.Contacts)
                        .ThenInclude(c => c.Categories)
                        .FirstOrDefault(u => u.Id == appUserId)!;

            if (String.IsNullOrEmpty(searchString))
            {
                contacts = appUser.Contacts
                                    .OrderBy(c => c.LastName)
                                    .ThenBy(c => c.FirstName)
                                     .ToList();
            }
            else
            {
                contacts = appUser.Contacts.Where(c => c.FullName!.ToLower().Contains(searchString.ToLower()))
                                   .OrderBy(c => c.LastName)
                                   .ThenBy(c => c.FirstName)
                                    .ToList();
            }
            ViewData["CategoryId"] = new SelectList(appUser.Categories, "Id", "Name", 0);
            return View(nameof(Index), contacts);
        }

        [Authorize]
        public async Task<IActionResult> EmailContact(int contactId)
        {
            string appuserId = _userManager.GetUserId(User)!;
            //current contact being passed in from the db
            Contact contact = await _context.Contacts.Where(c => c.Id == contactId && c.AppUserId == appuserId)
                                                     .FirstOrDefaultAsync();

            if (contact == null)
            {
                return NotFound();
            }

            EmailData emailData = new EmailData()
            {
                EmailAddress = contact.Email!,
                Firstname = contact.FirstName,
                LastName = contact.LastName
            };

            EmailContactViewModel model = new EmailContactViewModel()
            {
                Contact = contact,
                EmailData = emailData
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EmailContact(EmailContactViewModel ecvm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //send the email
                    await _emailService.SendEmailAsync(ecvm.EmailData.EmailAddress, ecvm.EmailData.Subject, ecvm.EmailData.Body);
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Success: Email Sent!" });
                }
                catch
                {
                    return RedirectToAction("Index", "Contacts", new { swalMessage = "Error: Email Send failed!" });
                    throw;
                }
            }


            return View(ecvm);
        }


        // GET: Contacts/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            string appUserId = _userManager.GetUserId(User)!;
            //This will get the loggedIn User and use that to
            //get a list of Categories. We'll then use that to push to our interface (GetUserCategoriesAsync).
            //Basically get the categories for the user.


            //This is a select list of users but we want a specific logged in user. So this is incorrect.
            ViewData["StateList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            //converts Enum-->States into a list.
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name");
            //this will push to the view all categores based on the logged in user.

            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber")] Contact contact, List<int> CategoryList)
        {
            ModelState.Remove("AppUserId");
            //Its required but the person doesnt type it into a field so its not in the incoming form.

            if (ModelState.IsValid)
            {
                contact.AppUserId = _userManager.GetUserId(User);
                //Current logged in user
                contact.Created = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);

                if (contact.BirthDate != null)
                {
                    contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                }

                //Now we working on the image below.
                if (contact.ImageFile != null)
                {
                    //we converting the file to byte array
                    contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                    //we storing the image type
                    contact.ImageType = contact.ImageFile.ContentType;
                }

                _context.Add(contact);
                await _context.SaveChangesAsync();
                //so before we redirect the action we need to:

                //Loop over all the selected Categories.
                foreach (int categoryId in CategoryList)
                {
                    //make sure to include CategoryList to your bind list.
                    //now for each one of these we want to add to the db
                    await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                }
                //Save each category selected to the CategoryContact table.

                //so on our contact input filed we have a virtual called Categories.



                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
            //return View(contact);
        }

        // GET: Contacts/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //the below code is incorrect because we're allowing each registered user to lookup their own contact,
            //its possible to put in an Id that doesnt belong to you and this line of code would return it.
            //So now we need to check if this contact belongs to this registred user.
            //var contact = await _context.Contacts.FindAsync(id);         

            string appUserId = _userManager.GetUserId(User)!;
            var contact = await _context.Contacts.Where(c => c.Id == id && c.AppUserId == appUserId)
                                                 .FirstOrDefaultAsync();
            //give me thse contacts where, if that contact doesnt  equal the logged in user 
            if (contact == null)
            {
                return NotFound();
            }
            ViewData["StateList"] = new SelectList(Enum.GetValues(typeof(States)).Cast<States>().ToList());
            ViewData["CategoryList"] = new MultiSelectList(await _addressBookService.GetUserCategoriesAsync(appUserId), "Id", "Name",
                await _addressBookService.GetContactCategoryIdsAsAsync(contact.Id));

            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AppUserId,FirstName,LastName,BirthDate,Address1,Address2,City,State,ZipCode,Email,PhoneNumber,Created, ImageFile,ImageData,ImageType")] Contact contact, List<int> CategoryList)
        {
            if (id != contact.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    contact.Created = DateTime.SpecifyKind(contact.Created, DateTimeKind.Utc);

                    if (contact.BirthDate != null)
                    {
                        contact.BirthDate = DateTime.SpecifyKind(contact.BirthDate.Value, DateTimeKind.Utc);
                    }
                    if (contact.ImageFile != null)
                    {
                        contact.ImageData = await _imageService.ConvertFileToByteArrayAsync(contact.ImageFile);
                        contact.ImageType = contact.ImageFile.ContentType;
                    }

                    _context.Update(contact);
                    await _context.SaveChangesAsync();

                    //Save our Categories
                    //remove the current categories
                    List<Category> oldCategories = (await _addressBookService.GetContactCategoriesAsync(contact.Id)).ToList();
                    foreach (var category in oldCategories)
                    {
                        await _addressBookService.RemoveContactFromCategoryAsync(category.Id, contact.Id);
                    }
                    //add the selected categories
                    foreach (int categoryId in CategoryList)
                    {
                        await _addressBookService.AddContactToCategoryAsync(categoryId, contact.Id);
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.Id))
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
            ViewData["AppUserId"] = new SelectList(_context.Users, "Id", "Id", contact.AppUserId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            string appUserId = _userManager.GetUserId(User)!;

            var contact = await _context.Contacts
                .FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == appUserId);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string appUserId = _userManager.GetUserId(User)!;

            var contact = await _context.Contacts.FirstOrDefaultAsync(m => m.Id == id && m.AppUserId == appUserId);

            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { swalMessage = "Success: Deleted!" });
        }

        private bool ContactExists(int id)
        {
            return _context.Contacts.Any(e => e.Id == id);
        }
    }
}
