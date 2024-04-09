using ChitChat.Models;

namespace ChitChat.Services.Interfacs
{
    public interface IAddressBookService
    {
        Task AddContactToCategoryAsync(int categoryId, int contactId);
        //when we save a contact, we want to add a category it belongs to.

        Task<bool> IsContactInCategory(int categoryId, int contactId);
        //We'll use thisone during the edit phase. When we present the list and check the ne we want.

        Task<IEnumerable<Category>> GetUserCategoriesAsync(string userId);
        //This will return a list of Categories that will bind to that drop down

        Task<ICollection<int>> GetContactCategoryIdsAsAsync(int contactId);
        //for given contact, return all of the category id's they're selected in.

        Task<ICollection<Category>> GetContactCategoriesAsync(int contactId);
        //This will return a collection of categories

        Task RemoveContactFromCategoryAsync(int categoryId, int contactId);

        IEnumerable<Contact> SearchForContacts(string searchString, string userId);
    }
}
