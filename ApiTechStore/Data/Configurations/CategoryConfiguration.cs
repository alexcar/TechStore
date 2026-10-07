using ApiTechStore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiTechStore.Data.Configurations;

/// <summary>Mapeia <see cref="Category"/> para a tabela dbo.Category criada pelos scripts do SqlTechStore.</summary>
internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Category");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id)
            .HasColumnName("ID");

        builder.Property(category => category.Name)
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();
    }
}
