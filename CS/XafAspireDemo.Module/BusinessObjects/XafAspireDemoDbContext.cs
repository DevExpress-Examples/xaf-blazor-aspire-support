using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.ExpressApp.EFCore.Updating;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XafAspireDemo.Module.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891/core-prerequisites-for-design-time-model-editor-with-entity-framework-core-data-model.
public class XafAspireDemoContextInitializer : DbContextTypesInfoInitializerBase
{
    protected override DbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<XafAspireDemoEFCoreDbContext>()
            .UseSqlServer(";") //.UseSqlite(";") wrong for a solution with SqLite, see https://isc.devexpress.com/internal/ticket/details/t1240173
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new XafAspireDemoEFCoreDbContext(optionsBuilder.Options);
    }
}

//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class XafAspireDemoDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<XafAspireDemoEFCoreDbContext>
{
    public XafAspireDemoEFCoreDbContext CreateDbContext(string[] args)
    {
        throw new InvalidOperationException(
            "Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception."
        );
        //var optionsBuilder = new DbContextOptionsBuilder<XafAspireDemoEFCoreDbContext>();
        //optionsBuilder.UseSqlServer("Integrated Security=SSPI;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=XafAspireDemo");
        //optionsBuilder.UseChangeTrackingProxies();
        //optionsBuilder.UseObjectSpaceLinkProxies();
        //return new XafAspireDemoEFCoreDbContext(optionsBuilder.Options);
    }
}

[TypesInfoInitializer(typeof(XafAspireDemoContextInitializer))]
public class XafAspireDemoEFCoreDbContext : DbContext
{
    public XafAspireDemoEFCoreDbContext(DbContextOptions<XafAspireDemoEFCoreDbContext> options)
        : base(options) { }

    //public DbSet<ModuleInfo> ModulesInfo { get; set; }

    public DbSet<DataItem> DataItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseDeferredDeletion(this);
        modelBuilder.SetOneToManyAssociationDeleteBehavior(
            DeleteBehavior.SetNull,
            DeleteBehavior.Cascade
        );
        modelBuilder.HasChangeTrackingStrategy(
            ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues
        );
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
    }
}
