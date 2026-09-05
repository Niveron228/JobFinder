using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace JustJoinJobFinder.DB
{
    public class AppDbContex: DbContext
    {
        public DbSet<SentOffer> SentOffers { get; set;  }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "offers.db")}");
        }
    }

    public class SentOffer
    {
        public int Id { get; set; }
        public string? Slug { get; set; }
        public DateTime SentAt { get; set; }
    }
}
