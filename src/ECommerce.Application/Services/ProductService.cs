using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Application.Exceptions;
using ECommerce.Domain.Entities;
using FluentValidation;

namespace ECommerce.Application.Services;

public class ProductService(
    IProductRepository productRepository,
    IValidator<CreateProductRequest> createValidator,
    IValidator<UpdateProductRequest> updateValidator)
    : IProductService
{
    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var name = request.Name.Trim();

        if (await productRepository.ExistsByNameAsync(
                name,
                cancellationToken))
        {
            throw new ConflictException(
                "A product with this name already exists.");
        }

        var product = new Product
        {
            Name = name,
            Description = request.Description.Trim(),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true
        };

        await productRepository.AddAsync(
            product,
            cancellationToken);

        await productRepository.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(product);
    }

    public async Task<ProductResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(
                "Product not found.");
        }

        return MapToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var products = await productRepository.GetAllAsync(
            cancellationToken);

        return products
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<ProductResponse> UpdateAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var product = await productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(
                "Product not found.");
        }

        var name = request.Name.Trim();

        if (!string.Equals(
                product.Name,
                name,
                StringComparison.OrdinalIgnoreCase)
            && await productRepository.ExistsByNameAsync(
                name,
                cancellationToken))
        {
            throw new ConflictException(
                "A product with this name already exists.");
        }

        product.Name = name;
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.IsActive = request.IsActive;

        productRepository.Update(product);

        await productRepository.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(product);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await productRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (product is null)
        {
            throw new NotFoundException(
                "Product not found.");
        }

        productRepository.Delete(product);

        await productRepository.SaveChangesAsync(
            cancellationToken);
    }

    private static ProductResponse MapToResponse(
        Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}