using Microsoft.EntityFrameworkCore;

namespace SematicKernelWeb.Models;

public partial class NewsReaderContext : DbContext
{
    public NewsReaderContext()
    {
    }

    public NewsReaderContext(DbContextOptions<NewsReaderContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<Conversationtype> Conversationtypes { get; set; }

    public virtual DbSet<Entry> Entries { get; set; }

    public virtual DbSet<Fetcheddoc> Fetcheddocs { get; set; }

    public virtual DbSet<Km5b340423C5a14f4599f317d3e1de1fbb> Km5b340423C5a14f4599f317d3e1de1fbbs { get; set; }

    public virtual DbSet<Km72f2d72e1b854271Bb3b75ab49d44720> Km72f2d72e1b854271Bb3b75ab49d44720s { get; set; }

    public virtual DbSet<KmA37769a7E62f47368fd6Acd2e1141032> KmA37769a7E62f47368fd6Acd2e1141032s { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Securityobject> Securityobjects { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https: //go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql(
            "Host=hqpostgre02;Database=BFSSematicKernel;Username=postgres;Password=Buttdance3#");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Pkconversationid).HasName("pk_conversation");

            entity.ToTable("conversation");

            entity.Property(e => e.Pkconversationid)
                .ValueGeneratedNever()
                .HasColumnName("pkconversationid");
            entity.Property(e => e.Createdat)
                .HasColumnType("timestamp(3) without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Fkparentid).HasColumnName("fkparentid");
            entity.Property(e => e.Fksecurityobjectowner).HasColumnName("fksecurityobjectowner");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.FksecurityobjectownerNavigation).WithMany(p => p.Conversations)
                .HasForeignKey(d => d.Fksecurityobjectowner)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_conversation_securityobjects");
        });

        modelBuilder.Entity<Conversationtype>(entity =>
        {
            entity.HasKey(e => e.Pkconversationtypeid).HasName("pk_conversationtypes");

            entity.ToTable("conversationtypes");

            entity.Property(e => e.Pkconversationtypeid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("pkconversationtypeid");
        });

        modelBuilder.Entity<Entry>(entity =>
        {
            entity.HasKey(e => e.Pkentryid).HasName("pk_entry");

            entity.ToTable("entry");

            entity.Property(e => e.Pkentryid)
                .ValueGeneratedNever()
                .HasColumnName("pkentryid");
            entity.Property(e => e.Filedata).HasColumnName("filedata");
            entity.Property(e => e.Fkconversationid).HasColumnName("fkconversationid");
            entity.Property(e => e.Fkconversationtypeid).HasColumnName("fkconversationtypeid");
            entity.Property(e => e.Fkroleid).HasColumnName("fkroleid");
            entity.Property(e => e.Ishidden).HasColumnName("ishidden");
            entity.Property(e => e.Numberofresults).HasColumnName("numberofresults");
            entity.Property(e => e.Resulttext).HasColumnName("resulttext");
            entity.Property(e => e.Sequence).HasColumnName("sequence");
            entity.Property(e => e.Text).HasColumnName("text");

            entity.HasOne(d => d.Fkconversation).WithMany(p => p.Entries)
                .HasForeignKey(d => d.Fkconversationid)
                .HasConstraintName("fk_entry_conversation");

            entity.HasOne(d => d.Fkconversationtype).WithMany(p => p.Entries)
                .HasForeignKey(d => d.Fkconversationtypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_entry_conversationtypes");

            entity.HasOne(d => d.Fkrole).WithMany(p => p.Entries)
                .HasForeignKey(d => d.Fkroleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_entry_role");
        });

        modelBuilder.Entity<Fetcheddoc>(entity =>
        {
            entity.HasKey(e => e.Pkfetchdocid).HasName("pk_fetcheddocs");

            entity.ToTable("fetcheddocs");

            entity.Property(e => e.Pkfetchdocid)
                .ValueGeneratedNever()
                .HasColumnName("pkfetchdocid");
            entity.Property(e => e.Body).HasColumnName("body");
            entity.Property(e => e.Fkentryid).HasColumnName("fkentryid");
            entity.Property(e => e.Memorykey).HasColumnName("memorykey");
            entity.Property(e => e.Summary).HasColumnName("summary");
            entity.Property(e => e.Uri).HasColumnName("uri");

            entity.HasOne(d => d.Fkentry).WithMany(p => p.Fetcheddocs)
                .HasForeignKey(d => d.Fkentryid)
                .HasConstraintName("fk_fetcheddocs_entry");
        });

        modelBuilder.Entity<Km5b340423C5a14f4599f317d3e1de1fbb>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("km-5b340423-c5a1-4f45-99f3-17d3e1de1fbb_pkey");

            entity.ToTable("km-5b340423-c5a1-4f45-99f3-17d3e1de1fbb");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasDefaultValueSql("''::text")
                .HasColumnName("content");
            entity.Property(e => e.Payload)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.Tags)
                .HasDefaultValueSql("'{}'::text[]")
                .HasColumnName("tags");
        });

        modelBuilder.Entity<Km72f2d72e1b854271Bb3b75ab49d44720>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("km-72f2d72e-1b85-4271-bb3b-75ab49d44720_pkey");

            entity.ToTable("km-72f2d72e-1b85-4271-bb3b-75ab49d44720");

            entity.HasIndex(e => e.Tags, "idx_tags").HasMethod("gin");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasDefaultValueSql("''::text")
                .HasColumnName("content");
            entity.Property(e => e.Payload)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.Tags)
                .HasDefaultValueSql("'{}'::text[]")
                .HasColumnName("tags");
        });

        modelBuilder.Entity<KmA37769a7E62f47368fd6Acd2e1141032>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("km-a37769a7-e62f-4736-8fd6-acd2e1141032_pkey");

            entity.ToTable("km-a37769a7-e62f-4736-8fd6-acd2e1141032");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasDefaultValueSql("''::text")
                .HasColumnName("content");
            entity.Property(e => e.Payload)
                .HasDefaultValueSql("'{}'::jsonb")
                .HasColumnType("jsonb")
                .HasColumnName("payload");
            entity.Property(e => e.Tags)
                .HasDefaultValueSql("'{}'::text[]")
                .HasColumnName("tags");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Pkroleid).HasName("pk_role");

            entity.ToTable("role");

            entity.Property(e => e.Pkroleid)
                .UseIdentityAlwaysColumn()
                .HasColumnName("pkroleid");
        });

        modelBuilder.Entity<Securityobject>(entity =>
        {
            entity.HasKey(e => e.Activedirectoryid).HasName("pk_securityobjects");

            entity.ToTable("securityobjects");

            entity.Property(e => e.Activedirectoryid)
                .ValueGeneratedNever()
                .HasColumnName("activedirectoryid");
            entity.Property(e => e.Emailaddress).HasColumnName("emailaddress");
            entity.Property(e => e.Forename).HasColumnName("forename");
            entity.Property(e => e.Fullname).HasColumnName("fullname");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Isgroup).HasColumnName("isgroup");
            entity.Property(e => e.Pass).HasColumnName("pass");
            entity.Property(e => e.Surname).HasColumnName("surname");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}