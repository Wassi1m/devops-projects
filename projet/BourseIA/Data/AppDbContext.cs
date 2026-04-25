using BourseIA.Models;
using Microsoft.EntityFrameworkCore;

namespace BourseIA.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<CourbeBoursiere> CourbesBousieres => Set<CourbeBoursiere>();
    public DbSet<ResultatAnalyse> ResultatsAnalyse => Set<ResultatAnalyse>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<MembreEquipe> MembresEquipe => Set<MembreEquipe>();
    public DbSet<IdeeInvestissement> IdeesInvestissement => Set<IdeeInvestissement>();
    public DbSet<MessageChat> MessagesChat => Set<MessageChat>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<PartageEquipe> PartagesEquipe => Set<PartageEquipe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurer l'auto-incrémentation pour PostgreSQL
        modelBuilder.Entity<Utilisateur>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<MessageChat>(entity =>
        {
            entity.HasOne(m => m.Expediteur)
                  .WithMany(u => u.MessagesEnvoyes)
                  .HasForeignKey(m => m.ExpediteurId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Destinataire)
                  .WithMany()
                  .HasForeignKey(m => m.DestinataireId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Team)
                  .WithMany(t => t.Messages)
                  .HasForeignKey(m => m.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasOne(t => t.Createur)
                  .WithMany()
                  .HasForeignKey(t => t.CreateurId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MembreEquipe>(entity =>
        {
            entity.HasOne(m => m.Utilisateur)
                  .WithMany(u => u.Equipes)
                  .HasForeignKey(m => m.UtilisateurId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Team)
                  .WithMany(t => t.Membres)
                  .HasForeignKey(m => m.TeamId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResultatAnalyse>(entity =>
        {
            entity.HasOne(r => r.CourbeBoursiere)
                  .WithOne(c => c.ResultatAnalyse)
                  .HasForeignKey<ResultatAnalyse>(r => r.CourbeBoursiereId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IdeeInvestissement>(entity =>
        {
            entity.HasOne(i => i.Utilisateur)
                  .WithMany(u => u.Idees)
                  .HasForeignKey(i => i.UtilisateurId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Team)
                  .WithMany(t => t.Idees)
                  .HasForeignKey(i => i.TeamId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.ResultatAnalyse)
                  .WithMany()
                  .HasForeignKey(i => i.ResultatAnalyseId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PartageEquipe>(entity =>
        {
            entity.HasOne(p => p.PartagePar)
                  .WithMany()
                  .HasForeignKey(p => p.PartageParId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.CourbeBoursiere)
                  .WithMany(c => c.Partages)
                  .HasForeignKey(p => p.CourbeBoursiereId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
