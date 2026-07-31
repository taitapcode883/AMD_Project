using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using PasteService.Controllers;
using PasteService.Data;
using PasteService.Models;
using Xunit;

namespace PasteService.Tests;

public class PasteControllerTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static void SetUser(PasteController controller, int? userId)
    {
        ClaimsPrincipal principal;

        if (userId is null)
        {
            // Không có claim + không có authenticationType => IsAuthenticated == false
            principal = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString())
            };

            // Truyền "TestAuth" làm authenticationType => IsAuthenticated == true
            principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    // ---------- PostPaste ----------

    [Fact]
    public async Task PostPaste_EmptyContent_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var request = new CreatePasteRequest
        {
            Content = "   ",
            Language = "text",
            Visibility = "public",
            Expiry = "never"
        };

        var result = await controller.PostPaste(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostPaste_ContentOverSizeLimit_ReturnsBadRequest()
    {
        using var context = CreateContext();
        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var request = new CreatePasteRequest
        {
            Content = new string('a', 500 * 1024 + 1),
            Language = "text",
            Visibility = "public",
            Expiry = "never"
        };

        var result = await controller.PostPaste(request);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task PostPaste_ValidAnonymousPaste_CreatesPasteWithNoOwner()
    {
        using var context = CreateContext();
        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var request = new CreatePasteRequest
        {
            Content = "Console.WriteLine(\"hi\");",
            Language = "csharp",
            Visibility = "public",
            Expiry = "1h"
        };

        var result = await controller.PostPaste(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var paste = Assert.IsType<Paste>(created.Value);

        Assert.False(string.IsNullOrEmpty(paste.Code));
        Assert.Null(paste.OwnerId);
        Assert.NotNull(paste.ExpiresAt);
        Assert.True(paste.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(1, await context.Pastes.CountAsync());
    }

    [Fact]
    public async Task PostPaste_AuthenticatedUser_SetsOwnerId()
    {
        using var context = CreateContext();
        var controller = new PasteController(context);
        SetUser(controller, userId: 42);

        var request = new CreatePasteRequest
        {
            Content = "some content",
            Language = "text",
            Visibility = "private",
            Expiry = "never"
        };

        var result = await controller.PostPaste(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var paste = Assert.IsType<Paste>(created.Value);

        Assert.Equal(42, paste.OwnerId);
        Assert.Null(paste.ExpiresAt); // "never" => không có hạn
    }

    // ---------- GetPaste ----------

    [Fact]
    public async Task GetPaste_CodeNotFound_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var result = await controller.GetPaste("doesNotExist");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetPaste_Expired_ReturnsNotFound()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "expired1",
            Content = "old",
            Language = "text",
            Visibility = "public",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // đã hết hạn
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var result = await controller.GetPaste("expired1");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetPaste_PrivateWithoutAuth_ReturnsUnauthorized()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "priv1",
            Content = "secret",
            Language = "text",
            Visibility = "private",
            CreatedAt = DateTime.UtcNow,
            OwnerId = 1
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var result = await controller.GetPaste("priv1");

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetPaste_PrivateWrongOwner_ReturnsForbid()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "priv2",
            Content = "secret",
            Language = "text",
            Visibility = "private",
            CreatedAt = DateTime.UtcNow,
            OwnerId = 1
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: 2); // khác chủ sở hữu

        var result = await controller.GetPaste("priv2");

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetPaste_PrivateCorrectOwner_ReturnsPasteAndIncrementsViewCount()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "priv3",
            Content = "secret",
            Language = "text",
            Visibility = "private",
            CreatedAt = DateTime.UtcNow,
            OwnerId = 1,
            ViewCount = 5
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: 1);

        var result = await controller.GetPaste("priv3");

        var paste = Assert.IsType<Paste>(result.Value);
        Assert.Equal(6, paste.ViewCount);
    }

    [Fact]
    public async Task GetPaste_PublicPaste_AnyoneCanViewAndViewCountIncrements()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "pub1",
            Content = "hello",
            Language = "text",
            Visibility = "public",
            CreatedAt = DateTime.UtcNow,
            ViewCount = 0
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var result = await controller.GetPaste("pub1");

        var paste = Assert.IsType<Paste>(result.Value);
        Assert.Equal(1, paste.ViewCount);
    }

    // ---------- GetMine ----------

    [Fact]
    public async Task GetMine_ReturnsOnlyCurrentUsersPastes()
    {
        using var context = CreateContext();
        context.Pastes.AddRange(
            new Paste { Code = "a1", Content = "x", Language = "text", Visibility = "private", CreatedAt = DateTime.UtcNow, OwnerId = 1 },
            new Paste { Code = "a2", Content = "y", Language = "text", Visibility = "public", CreatedAt = DateTime.UtcNow, OwnerId = 1 },
            new Paste { Code = "b1", Content = "z", Language = "text", Visibility = "public", CreatedAt = DateTime.UtcNow, OwnerId = 2 }
        );
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: 1);

        var result = await controller.GetMine();

        var pastes = Assert.IsAssignableFrom<IEnumerable<Paste>>(result.Value);
        Assert.Equal(2, pastes.Count());
        Assert.All(pastes, p => Assert.Equal(1, p.OwnerId));
    }

    // ---------- DeletePaste ----------

    [Fact]
    public async Task DeletePaste_CodeNotFound_ReturnsNotFound()
    {
        using var context = CreateContext();
        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var result = await controller.DeletePaste("missing");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletePaste_OwnedPasteWithoutAuth_ReturnsUnauthorized()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "owned1",
            Content = "x",
            Language = "text",
            Visibility = "private",
            CreatedAt = DateTime.UtcNow,
            OwnerId = 1
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var result = await controller.DeletePaste("owned1");

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task DeletePaste_OwnedPasteWrongOwner_ReturnsForbid()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "owned2",
            Content = "x",
            Language = "text",
            Visibility = "private",
            CreatedAt = DateTime.UtcNow,
            OwnerId = 1
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: 2);

        var result = await controller.DeletePaste("owned2");

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeletePaste_CorrectOwner_RemovesPasteAndReturnsNoContent()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "owned3",
            Content = "x",
            Language = "text",
            Visibility = "private",
            CreatedAt = DateTime.UtcNow,
            OwnerId = 1
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: 1);

        var result = await controller.DeletePaste("owned3");

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await context.Pastes.CountAsync());
    }

    [Fact]
    public async Task DeletePaste_AnonymousPaste_AnyoneCanDelete()
    {
        using var context = CreateContext();
        context.Pastes.Add(new Paste
        {
            Code = "anon1",
            Content = "x",
            Language = "text",
            Visibility = "public",
            CreatedAt = DateTime.UtcNow,
            OwnerId = null
        });
        await context.SaveChangesAsync();

        var controller = new PasteController(context);
        SetUser(controller, userId: null);

        var result = await controller.DeletePaste("anon1");

        Assert.IsType<NoContentResult>(result);
    }
}
