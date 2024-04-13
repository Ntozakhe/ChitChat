using ChitChat.Data;
using ChitChat.Models;
using ChitChat.Services.Interfacs;
using Microsoft.EntityFrameworkCore;

namespace ChitChat.Services
{
    public class AddressBookService : IAddressBookService
    {
        private readonly ApplicationDbContext _context;
        //we are injecting the database so we can use it here
        public AddressBookService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddContactToCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                //check to see if the category being passed in is in the contact already.
                if (!await IsContactInCategory(categoryId, contactId))
                {
                    //Then now we can add our contact to a category
                    //This is to know what id to add
                    Contact? contact = await _context.Contacts.FindAsync(contactId);
                    Category? category = await _context.Categories.FindAsync(categoryId);

                    if (contact != null && category != null)
                    {
                        //Then now we can add our contact to a category
                        category.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }
                }


            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ICollection<Category>> GetContactCategoriesAsync(int contactId)
        {
            try
            {
                Contact? contact = await _context.Contacts.Include(c => c.Categories)
                                                          .FirstOrDefaultAsync(c => c.Id == contactId);
                return contact!.Categories;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ICollection<int>> GetContactCategoryIdsAsAsync(int contactId)
        {
            try
            {
                //we want to pull back a list of category id's for the particular contact we're looking at.
                var contact = await _context.Contacts.Include(c => c.Categories)
                                                     .FirstOrDefaultAsync(c => c.Id == contactId);

                List<int> categoryIds = contact!.Categories.Select(c => c.Id).ToList();
                return categoryIds;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId)
        {
            List<Category> categories = new List<Category>();
            try
            {
                categories = await _context.Categories.Where(c => c.AppUserId == userId)
                                                      .OrderBy(c => c.Name)
                                                      .ToListAsync();
                //This is looking at the categories table in the db, but need the ones just for the user
                //we've gone to the databasse, we told the db which table we looking at, and filer by userId
            }
            catch
            {
                throw;
            }
            return categories;
        }

        public async Task<bool> IsContactInCategory(int categoryId, int contactId)
        {
            //check to see if the category being passed in is in the contact already.

            //we need to see if we can find the conatct first.
            Contact? contact = await _context.Contacts.FindAsync(contactId);

            //when we find it we can return the list of categories
            return await _context.Categories
                                 .Include(c => c.Contacts)
                                 .Where(c => c.Id == contactId && c.Contacts.Contains(contact))
                                 .AnyAsync(); //this returns true/false.
        }

        public async Task RemoveContactFromCategoryAsync(int categoryId, int contactId)
        {
            try
            {
                if (await IsContactInCategory(categoryId, contactId))
                {
                    Contact contact = await _context.Contacts.FindAsync(contactId);
                    Category category = await _context.Categories.FindAsync(categoryId);

                    if (contact != null && category != null)
                    {
                        category.Contacts.Remove(contact);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<Contact> SearchForContacts(string searchString, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
