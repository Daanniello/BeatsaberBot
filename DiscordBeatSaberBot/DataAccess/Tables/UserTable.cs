using Microsoft.EntityFrameworkCore;

namespace DiscordBeatSaberBot.DataAccess.Tables
{
    public class UserTable : DbContext
    {
        public DbSet<Models.UserModel> Users { get; set; }
    }
}