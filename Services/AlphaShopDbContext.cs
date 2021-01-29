using ArticoliWebService.Models;
using Microsoft.EntityFrameworkCore;


namespace ArticoliWebService.Services
{
    public class AlphaShopDbContext : DbContext
    {
        public AlphaShopDbContext(DbContextOptions<AlphaShopDbContext> options)
            : base(options)
        {
            
        }

        public virtual DbSet<Articoli> Articoli { get; set; }
        public virtual DbSet<Ean> Barcode {get; set; }
        public virtual DbSet<FamAssort> Famassort { get; set; }
        public virtual DbSet<Ingredienti> Ingredienti { get; set; }
        public virtual DbSet<Iva> Iva { get; set; }

        protected override void OnModelCreating (ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Articoli>()
                .HasKey(a => new { a.CodArt });

            //Relazione one to many (uno a molti) fra articoli e barcode
            modelBuilder.Entity<Ean>()
                .HasOne<Articoli>(s => s.articolo) //ad un articolo...
                .WithMany(g => g.barcode) //corrispondono molti barcode
                .HasForeignKey(s => s.CodArt); //la chiave esterna dell'entity barcode

            //Relazione one to one (uno a uno) fra articoli e ingredienti
            modelBuilder.Entity<Articoli>()
                .HasOne<Ingredienti>(s => s.ingredienti) //ad un ingrediente...
                .WithOne(g => g.articolo)  //corrisponde un articolo
                .HasForeignKey<Ingredienti>(s => s.CodArt);

            //Relazione one to many fra iva e articoli 
            modelBuilder.Entity<Articoli>()
                .HasOne<Iva>(s => s.iva) //ad una aliquota iva
                .WithMany(g => g.Articoli) // corrispondono molti articoli
                .HasForeignKey(s => s.IdIva);

            //Relaione one to many fra FamAssort e Articoli
            modelBuilder.Entity<Articoli>()
                .HasOne<FamAssort>(s => s.famAssort)
                .WithMany(g => g.Articoli)
                .HasForeignKey(s => s.IdFamAss);


        }

        
    }
}