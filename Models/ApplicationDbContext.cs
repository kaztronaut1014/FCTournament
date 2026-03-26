using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Format> Formats { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchDetail> MatchDetails { get; set; }
        public DbSet<MatchReferee> MatchReferees { get; set; }
        public DbSet<MatchSquad> MatchSquads { get; set; }
        public DbSet<MatchStats> MatchStats { get; set; }
        public DbSet<Milestone> Milestones { get; set; }
        public DbSet<Organizer> Organizers { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerPerformance> PlayerPerformances { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<Referee> Referees { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentTeam> TournamentTeams { get; set; }
        public DbSet<Type> Types { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<BillDetails> BillDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // BẮT BUỘC: Giữ lại dòng này để Identity hoạt động 
            base.OnModelCreating(modelBuilder);

            // PHƯƠNG ÁN TỔNG LỰC: Tắt Cascade Delete cho tất cả các bảng trong dự án
            // Cách này sẽ quét mọi mối quan hệ và chuyển chúng về NoAction
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // 1. Cấu hình quan hệ 1-1 cho MatchStats (EF Core bắt buộc cấu hình riêng) [cite: 7]
            // Cấu hình quan hệ 1-N cho MatchStats (1 Trận đấu có 2 bản ghi MatchStats cho Home và Away)
            modelBuilder.Entity<Match>()
                .HasMany(m => m.MatchStats) // Sửa HasOne thành HasMany
                .WithOne(ms => ms.Match)
                .HasForeignKey(ms => ms.MatchId); // Bỏ dấu <MatchStats> ở đây đi

            // 2. Cấu hình Soft Delete cho các bảng chính
            modelBuilder.Entity<Tournament>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Team>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Match>().HasQueryFilter(m => !m.IsDeleted);
        }
    }
}
