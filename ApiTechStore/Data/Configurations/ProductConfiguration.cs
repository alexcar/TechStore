using ApiTechStore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiTechStore.Data.Configurations;

/// <summary>Mapeia <see cref="Product"/> para a tabela dbo.Product criada pelos scripts do SqlTechStore.</summary>
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .HasColumnName("ID");

        builder.Property(product => product.CategoryId)
            .HasColumnName("CategoryID");

        builder.Property(product => product.Name)
            .HasMaxLength(Product.NameMaxLength)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(product => product.Price)
            .HasPrecision(10, 2);

        builder.HasIndex(product => product.Name)
            .IsUnique()
            .HasDatabaseName("UQ_Product_Name");

        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .HasConstraintName("FK_Product_Category")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
