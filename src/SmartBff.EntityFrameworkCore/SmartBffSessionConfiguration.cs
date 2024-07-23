using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SmartBff.EntityFrameworkCore;



public interface ISmartBffSessionContext
{
    DbSet<SmartBffSession> SmartBffSessions { get; } 
}

public sealed class SmartBffSessionConfiguration : IEntityTypeConfiguration<SmartBffSession>
{
    public void Configure(EntityTypeBuilder<SmartBffSession> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.HasKey(token => token.Id);

        builder.Property(token => token.ConcurrencyToken)
            .HasMaxLength(50)
            .IsConcurrencyToken();

        builder.Property(token => token.Id)
            .ValueGeneratedOnAdd();

        builder.Property(token => token.Type)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(token => token.Name)
            .HasMaxLength(100);

        builder.Property(token => token.RegistrationId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(token => token.CreatedOn)
            .IsRequired();

        builder.Property(token => token.Payload)
            .IsRequired();

        builder.ToTable("SmartBffSessions");
    }
}