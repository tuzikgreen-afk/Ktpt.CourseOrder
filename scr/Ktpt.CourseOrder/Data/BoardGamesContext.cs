using Microsoft.EntityFrameworkCore;
using Ktpt.CourseOrder.Models;

namespace Ktpt.CourseOrder.Data
{
    public partial class BoardGamesContext : DbContext
    {
        public BoardGamesContext() { }
        public BoardGamesContext(DbContextOptions<BoardGamesContext> options) : base(options) { }

        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Publisher> Publishers { get; set; } = null!;
        public virtual DbSet<Game> Games { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = @"Server=(LocalDb)\MSSQLLocalDB;Database=boardgames_shop;Trusted_Connection=True;TrustServerCertificate=True;";
                if (System.IO.File.Exists("connection.txt"))
                {
                    connectionString = System.IO.File.ReadAllText("connection.txt").Trim();
                }
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(e => e.RoleId).HasName("pk_roles");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.Property(e => e.RoleName).HasMaxLength(50).HasColumnName("role_name");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.UserId).HasName("pk_users");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Login).HasMaxLength(50).HasColumnName("login");
                entity.Property(e => e.PasswordHash).HasMaxLength(255).HasColumnName("password_hash");
                entity.Property(e => e.LastName).HasMaxLength(60).HasColumnName("last_name");
                entity.Property(e => e.FirstName).HasMaxLength(60).HasColumnName("first_name");
                entity.Property(e => e.MiddleName).HasMaxLength(60).HasColumnName("middle_name");
                entity.Property(e => e.RoleId).HasColumnName("role_id");
                entity.HasOne(e => e.Role).WithMany(r => r.Users).HasForeignKey(e => e.RoleId).HasConstraintName("fk_users_role");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(e => e.CategoryId).HasName("pk_categories");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.CategoryName).HasMaxLength(180).HasColumnName("category_name");
            });

            modelBuilder.Entity<Publisher>(entity =>
            {
                entity.ToTable("publishers");
                entity.HasKey(e => e.PublisherId).HasName("pk_publishers");
                entity.Property(e => e.PublisherId).HasColumnName("publisher_id");
                entity.Property(e => e.PublisherName).HasMaxLength(120).HasColumnName("publisher_name");
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("games");
                entity.HasKey(e => e.GameId).HasName("pk_games");
                entity.Property(e => e.GameId).HasColumnName("game_id");
                entity.Property(e => e.GameCode).HasMaxLength(20).HasColumnName("game_code");
                entity.Property(e => e.GameName).HasMaxLength(150).HasColumnName("game_name");
                entity.Property(e => e.CategoryId).HasColumnName("category_id");
                entity.Property(e => e.PublisherId).HasColumnName("publisher_id");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ImagePath).HasColumnName("image_path");
                entity.Property(e => e.BasePrice).HasColumnName("base_price");
                entity.Property(e => e.AvailableStock).HasColumnName("available_stock");
                entity.HasOne(e => e.Category).WithMany(c => c.Games).HasForeignKey(e => e.CategoryId).HasConstraintName("fk_games_category");
                entity.HasOne(e => e.Publisher).WithMany(p => p.Games).HasForeignKey(e => e.PublisherId).HasConstraintName("fk_games_publisher");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.OrderId).HasName("pk_orders");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.ClientId).HasColumnName("client_id");
                entity.Property(e => e.OrderDate).HasColumnName("order_date");
                entity.HasOne(e => e.Client).WithMany(c => c.Orders).HasForeignKey(e => e.ClientId).HasConstraintName("fk_orders_client");
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(e => e.OrderItemId).HasName("pk_order_items");
                entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.GameId).HasColumnName("game_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
                entity.HasOne(e => e.Order).WithMany(o => o.OrderItems).HasForeignKey(e => e.OrderId).HasConstraintName("fk_order_items_order");
                entity.HasOne(e => e.Game)
                      .WithMany(p => p.OrderItems) // <-- Явно указываем коллекцию из класса Game!
                      .HasForeignKey(e => e.GameId)
                      .HasConstraintName("fk_order_items_game");
            });
        }
    }
}