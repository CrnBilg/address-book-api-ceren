using AddressBook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AddressBook.Api.Data;

public sealed class AddressBookDbContext : DbContext
{
    public AddressBookDbContext(DbContextOptions<AddressBookDbContext> options)
        : base(options)
    {
    }

    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(contact => contact.Id);
            entity.Property(contact => contact.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(contact => contact.LastName).IsRequired().HasMaxLength(100);
            entity.Property(contact => contact.Email).HasMaxLength(254);
            entity.Property(contact => contact.PhoneNumber).HasMaxLength(40);
            entity.Property(contact => contact.Company).HasMaxLength(120);
            entity.Property(contact => contact.Notes).HasMaxLength(1000);
        });
    }
}
