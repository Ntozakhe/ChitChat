using ChitChat.Data;
using Microsoft.EntityFrameworkCore;

namespace ChitChat.Helpers
{
    public static class DataHelper
    {
        public static async Task ManageDataAsync(IServiceProvider svcProvider)
        {
            //get na instance of the db application context
            var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();

            //Tmigration: thisnis equivalent to update database
            await dbContextSvc.Database.MigrateAsync();
        }
    }
}
