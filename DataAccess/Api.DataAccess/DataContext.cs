
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal; // Needed for DatePart methods
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Extensions;

namespace Api.DataAccess
{
    public class DataContext : DbContext
    {
        readonly IHttpContextAccessor accessor;
        public int DatePart(string datePartArg, DateTimeOffset? date) => throw new InvalidOperationException($"{nameof(DatePart)} cannot be called client side.");


        public DataContext(DbContextOptions<DataContext> options, IHttpContextAccessor accessor)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            this.accessor = accessor;
        }

        public DbSet<Categories> Categories { get; set; }
        public DbSet<CategoriesShared> CategoriesShared { get; set; }
        //public DbSet<CategoriesSharedPrivillege> CategoriesSharedPrivillege { get; set; }
        public DbSet<Attributes> Attributtes { get; set; }
        public DbSet<Documents> Documents { get; set; }
        public DbSet<DocumentAttributes> DocumentAttributes { get; set; }
        public DbSet<DocumentFiles> DocumentFiles { get; set; }
        public DbSet<DocumentShared> DocumentShared { get; set; }
        public DbSet<Approvals> Approvals { get; set; }
        public DbSet<ApprovalFlows> ApprovalFlows { get; set; }
        public DbSet<ApprovalActivities> ApprovalActivities { get; set; }
        public DbSet<AttributeCollections> AttributeCollections { get; set; }
        public DbSet<DocumentSharedPrivillege> DocumentSharedPrivillege { get; set; }
        public DbSet<Watermarks> Watermarks { get; set; }
        public DbSet<CategoriesFavorite> CategoriesFavorites { get; set; }
        public DbSet<DocumentFavorite> DocumentFavorites { get; set; }
        public DbSet<Notifications> Notifications { get; set; }

        public DbSet<ApplicationLog> ApplicationLog { get; set; }
        public DbSet<AuditTrail> AuditTrail { get; set; }
        public DbSet<TransactionLog> TransactionLog { get; set; }
        public DbSet<HistoryEmail> HistoryEmail { get; set; }
        public DbSet<HistoryAuditTrail> HistoryAuditTrail { get; set; }
        public DbSet<HistoryTransactionLog> HistoryTransactionLog { get; set; }

        public DbSet<Company> Company { get; set; }
        public DbSet<Language> Language { get; set; }
        public DbSet<TmDocumentType> DocumentTypes { get; set; }
        public DbSet<TmDocumentTypeAttributes> DocumentTypeAttributes { get; set; }
        public DbSet<TmAttributeSynonyms> AttributeSynonyms { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<RoleMatrix> RoleMatrix { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<UserMatrix> UserMatrix { get; set; }
        public DbSet<UserMedia> UserMedia { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        public DbSet<UserCompany> UserCompany { get; set; }
        public DbSet<Email> Email { get; set; }
        public DbSet<DocumentReminders> DocumentReminders { get; set; }
        public DbSet<DocumentLog> DocumentLog { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<UserGroup> UserGroup { get; set; }
        public DbSet<DocumentItemList> DocumentItemList { get; set; }
        public DbSet<ResetPasswordUserVerificationCode> ResetPasswordUserVerificationCode { get; set; }
        public DbSet<DocumentRelated> RelatedDocuments { get; set; }
        public DbSet<MigrationJob> MigrationJobs { get; set; }
        public DbSet<LoginActivityLog> LoginActivityLogs { get; set; }

        public DbSet<AgentPollingTaskDocument> AgentPollingTaskDocuments { get; set; }
        public DbSet<DocumentExtractedEntities> DocumentExtractedEntities { get; set; }

        public void ChangeTrackingBehavior(QueryTrackingBehavior behavior)
        {
            ChangeTracker.QueryTrackingBehavior = behavior;
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        {
            //builder.Properties<DateTime>().HaveColumnType("datetime");
            //builder.Properties<DateTime?>().HaveColumnType("datetime");
            builder.Properties<DateTime>().HaveColumnType("timestamp");//TIMESTAMP
            builder.Properties<DateTime?>().HaveColumnType("timestamp");//TIMESTAMP
        }

        //public int? DatePart(string datePartArg, DateTimeOffset? date) => throw new InvalidOperationException($"{nameof(DatePart)} cannot be called client side.");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder
            .HasDbFunction(() => PgSqlDbFunctions.DatePart(default!, default))
            .HasName("DATE_PART")
            .IsBuiltIn();
            modelBuilder.Entity<MigrationJob>(entity =>
            {
                // This tells EF: "Keep the model, but don't create/alter the table in migrations"
                entity.ToTable("MigrationJobs", t => t.ExcludeFromMigrations());
            });
            #region MASTER
            //modelBuilder.Entity<Company>()
            //    .ToTable("TblMsCompany")
            //    .HasKey(x => new { x.CompanyId });
            #endregion

            #region NAVIGATION PROPERTY
            //modelBuilder.Entity<UserRole>()
            //    .HasOne(s => s.User)
            //    .WithMany(ta => ta.UserRoles)
            //    .HasForeignKey(u => u.UserName)
            //    .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<UserRole>()
            //    .HasOne(s => s.Group)
            //    .WithMany(ta => ta.UserRoles)
            //    .HasForeignKey(u => u.RoleId)
            //    .OnDelete(DeleteBehavior.NoAction);

            #endregion

            //modelBuilder.Entity<Entity>().Property(s => s.RoleFeatureId).HasConversion<JsonPrimitiveTypeConverter>();
            //modelBuilder.Entity<Entity>()
            //    .Property(p => p.RoleFeatureId)
            //    .HasConversion(
            //        entity => JsonConvert.SerializeObject(entity),
            //        value => JsonConvert.DeserializeObject<List<string>>(value)
            //    );

            //modelBuilder
            //.Entity<Entity>()
            //.OwnsMany(role => role.RoleFeatureId, builder =>
            //{
            //    builder.ToJson(); 
            //});

            // modelBuilder.Entity<User>().HasKey(u => new { u.UserId, u.MailServer }); // Composite key

            modelBuilder.Entity<ApplicationLog>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()"); // Use PostgreSQL's uuid_generate_v4() for default value
            modelBuilder.Entity<AuditTrail>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Email>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<HistoryAuditTrail>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<HistoryEmail>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<HistoryTransactionLog>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<TransactionLog>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Notifications>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<AttributeCollections>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<AttributeCollections>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<CategoriesFavorite>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<DocumentFavorite>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<DocumentLog>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<LoginActivityLog>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");

            //modelBuilder.Entity<DocumentFiles>().Property(e => e.SearchVector).HasColumnType("tsvector")
            //.HasComputedColumnSql("to_tsvector('english', coalesce(\"DocumentFileContent\",''))", stored: true);
            modelBuilder.Entity<DocumentFiles>().HasGeneratedTsVectorColumn(
                   p => p.SearchVector,
                   "english",
                   p => new { p.DocumentFileContent } // Columns to include
                  )
                  .HasIndex(p => p.SearchVector) // Index is crucial for performance
                  .HasMethod("GIN"); // Use GIN index method

            //modelBuilder.Entity<DocumentFiles>().Property<string>("SearchVector").HasColumnType("tsvector").HasColumnName("SearchVector");
            //modelBuilder.Entity<Approvals>().Property(u => u.Id).HasDefaultValueSql("uuid_generate_v4()");


            modelBuilder.Entity<Categories>(a =>
            {
                a.HasOne(d => d.ParentCategory).WithMany(d => d.ChildCategories).HasForeignKey(d => d.ParentId).IsRequired(false)
                    .OnDelete(DeleteBehavior.NoAction);

                a.HasQueryFilter(p => p.IsActive);
            });

        }


        #region DELETION FLAGS
        public void MarkAsDeleted(object entity) => MarkDeletion(entity, true);

        public void MarkAsUndeleted(object entity) => MarkDeletion(entity, false);

        private void MarkDeletion(object entity, bool deleted)
        {
            var name = accessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(name))
                return;

            var type = entity.GetType();

            type.GetProperty(nameof(BaseEntityDefault.IsActive)).SetValue(entity, deleted);
            //type.GetProperty(nameof(BaseEntity.DeletedAt)).SetValue(entity, DateTime.Now);
            //type.GetProperty(nameof(BaseEntity.DeletedBy)).SetValue(entity, name);

            Attach(entity);

            //Entry(entity).Property(nameof(BaseEntityDefault.IsActive)).IsModified = false;
            Entry(entity).Property(nameof(BaseEntityDefault.IsActive)).IsModified = true;
            //Entry(entity).Property(nameof(BaseEntity.DeletedAt)).IsModified = true;
            //Entry(entity).Property(nameof(BaseEntity.DeletedBy)).IsModified = true;

            base.SaveChanges();
        }
        #endregion

        //private void SetBaseEntity()
        //{
        //    var name = accessor?.HttpContext?.User?.Identity?.Name ?? "0";
        //    int userId = int.Parse(name);

        //    foreach (var entityEntry in ChangeTracker.Entries())
        //    {
        //        var type = entityEntry.Entity.GetType();
        //        if (entityEntry.State == EntityState.Added)
        //        {
        //            type.GetProperty(nameof(BaseEntityDefault.InsertedAt))?.SetValue(entityEntry.Entity, DateTime.Now);
        //            type.GetProperty(nameof(BaseEntityDefault.InsertedBy))?.SetValue(entityEntry.Entity, userId);

        //            type.GetProperty(nameof(BaseEntityDefault.UpdatedAt))?.SetValue(entityEntry.Entity, DateTime.Now);
        //            type.GetProperty(nameof(BaseEntityDefault.UpdatedBy))?.SetValue(entityEntry.Entity, userId);
        //        }

        //        if (entityEntry.State == EntityState.Modified)
        //        {
        //            type.GetProperty(nameof(BaseEntityDefault.UpdatedAt))?.SetValue(entityEntry.Entity, DateTime.Now);
        //            type.GetProperty(nameof(BaseEntityDefault.UpdatedBy))?.SetValue(entityEntry.Entity, userId);
        //        }
        //    }
        //}

        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    SetBaseEntity();
        //    return await base.SaveChangesAsync(cancellationToken);
        //}
    }
}
