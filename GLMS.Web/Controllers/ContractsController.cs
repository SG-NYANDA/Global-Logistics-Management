using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers;

public sealed class ContractsController(
    GlmsDbContext db,
    AgreementFileStore agreementFileStore) : Controller
{
    public async Task<IActionResult> Index(
        ContractStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var query = db.Contracts.Include(contract => contract.Client).AsNoTracking();

        if (status == ContractStatus.Expired)
        {
            query = query.Where(contract => contract.Status == ContractStatus.Expired
                || (contract.Status == ContractStatus.Active && contract.EndDate < today));
        }
        else if (status == ContractStatus.Active)
        {
            query = query.Where(contract => contract.Status == ContractStatus.Active
                && contract.StartDate <= today && contract.EndDate >= today);
        }
        else if (status.HasValue)
        {
            query = query.Where(contract => contract.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(contract => contract.EndDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(contract => contract.StartDate <= toDate.Value.Date);
        }

        var model = new ContractIndexViewModel
        {
            Contracts = await query.OrderByDescending(contract => contract.StartDate).ToListAsync(cancellationToken),
            Status = status,
            FromDate = fromDate,
            ToDate = toDate
        };
        return View(model);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ContractCreateViewModel();
        await PopulateClientsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> Create(ContractCreateViewModel model, CancellationToken cancellationToken)
    {
        if (model.EndDate.Date < model.StartDate.Date)
        {
            ModelState.AddModelError(nameof(model.EndDate), "End date must be on or after the start date.");
        }

        if (!Enum.IsDefined(model.Status))
        {
            ModelState.AddModelError(nameof(model.Status), "Select a valid contract status.");
        }

        if (model.Agreement is null)
        {
            ModelState.AddModelError(nameof(model.Agreement), "Upload the signed PDF agreement.");
        }

        if (!await db.Clients.AnyAsync(client => client.Id == model.ClientId, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.ClientId), "Select an existing client.");
        }

        if (await db.Contracts.AnyAsync(contract => contract.Reference == model.Reference, cancellationToken))
        {
            ModelState.AddModelError(nameof(model.Reference), "That contract reference is already in use.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateClientsAsync(model, cancellationToken);
            return View(model);
        }

        string agreementFileName;
        try
        {
            agreementFileName = await agreementFileStore.SaveAsync(model.Agreement!, cancellationToken);
        }
        catch (InvalidDataException exception)
        {
            ModelState.AddModelError(nameof(model.Agreement), exception.Message);
            await PopulateClientsAsync(model, cancellationToken);
            return View(model);
        }

        db.Contracts.Add(new Contract
        {
            Reference = model.Reference.Trim(),
            ServiceLevel = model.ServiceLevel.Trim(),
            StartDate = model.StartDate.Date,
            EndDate = model.EndDate.Date,
            Status = model.Status,
            ClientId = model.ClientId,
            AgreementFileName = agreementFileName
        });

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            agreementFileStore.Delete(agreementFileName);
            ModelState.AddModelError(nameof(model.Reference), "The contract could not be saved. Check that its reference is unique.");
            await PopulateClientsAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Notice"] = "Contract added.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> DownloadAgreement(int id, CancellationToken cancellationToken)
    {
        var contract = await db.Contracts.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (contract is null)
        {
            return NotFound();
        }

        var path = agreementFileStore.FindPath(contract.AgreementFileName);
        return path is null ? NotFound() : PhysicalFile(path, "application/pdf", $"{contract.Reference}-agreement.pdf");
    }

    private async Task PopulateClientsAsync(ContractCreateViewModel model, CancellationToken cancellationToken)
    {
        model.Clients = await db.Clients
            .AsNoTracking()
            .OrderBy(client => client.Name)
            .Select(client => new SelectListItem(client.Name, client.Id.ToString()))
            .ToListAsync(cancellationToken);
    }
}