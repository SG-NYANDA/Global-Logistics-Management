using GLMS.Web.Data;
using GLMS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers;

public sealed class ClientsController(GlmsDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var clients = await db.Clients
            .AsNoTracking()
            .Include(client => client.Contracts)
            .OrderBy(client => client.Name)
            .ToListAsync(cancellationToken);
        return View(clients);
    }

    public IActionResult Create() => View(new ClientCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        db.Clients.Add(new Client
        {
            Name = model.Name.Trim(),
            ContactDetails = model.ContactDetails.Trim(),
            Region = model.Region.Trim()
        });
        await db.SaveChangesAsync(cancellationToken);
        TempData["Notice"] = "Client added.";
        return RedirectToAction(nameof(Index));
    }
}