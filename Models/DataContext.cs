using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBCore.Models;

namespace WEBTest.Models
{
    public class DataContext : DbContext,InterfaceDB
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            // ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
           
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Friend>().ToTable("Firend");
            base.OnModelCreating(modelBuilder);
        }

        public void add()
        {
            throw new NotImplementedException();
        }

        public DbSet<User> Users { get; set; }
        
        public DbSet<Friend> Firends { get; set; }
    }
}
