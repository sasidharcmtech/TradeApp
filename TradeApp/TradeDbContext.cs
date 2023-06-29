using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeApp
{
    public class TradeDbContext : DbContext
    {
        public DbSet<TradeModel> Trade { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = @"C:\temp\trade.db";
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
