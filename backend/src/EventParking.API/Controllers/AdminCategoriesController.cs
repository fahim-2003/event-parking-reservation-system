using EventParking.API.DTOs.Categories;
using EventParking.API.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventParking.API.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Administrator")]
public sealed class AdminCategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public AdminCategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
[ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<ActionResult<CategoryResponse>> Create(
    [FromBody] CreateCategoryRequest request,
    CancellationToken cancellationToken)
{
    try
    {
        var category = await _categoryService.CreateAsync(
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            category);
    }
    catch (DbUpdateException)
    {
        return Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Category conflict.",
            detail: "A category with the same name already exists.");
    }
}
    [HttpPut("{categoryId:int}")]
[ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<ActionResult<CategoryResponse>> Update(
    int categoryId,
    [FromBody] UpdateCategoryRequest request,
    CancellationToken cancellationToken)
{
    try
    {
        var category = await _categoryService.UpdateAsync(
            categoryId,
            request,
            cancellationToken);

        if (category is null)
        {
            return NotFound();
        }

        return Ok(category);
    }
    catch (DbUpdateException)
    {
        return Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "Category conflict.",
            detail: "A category with the same name already exists.");
    }
}

    [HttpDelete("{categoryId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        int categoryId,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _categoryService.DeleteAsync(
                categoryId,
                cancellationToken);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Category conflict.",
                detail: "The category cannot be deleted because it is referenced by existing data.");
        }
    }
}