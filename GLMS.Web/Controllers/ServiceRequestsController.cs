using System.Text.Json;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Web.Controllers;

public sealed class ServiceRequestsController(
    GlmsDbContext db,
    IExchangeRateProvider exchangeRateProvider) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var requests = await db.ServiceRequests
            .AsNoTracking()
            .Include(request => request.Contract)
            .ThenInclude(contract => contract!.Client)
            .OrderByDescending(request => request.CreatedUtc)
            .ToListAsync(cancellationToken);
        return View(requests);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ServiceRequestCreateViewModel();
        await PopulateContractsAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel model, CancellationToken cancellationToken)
    {
        var contract = await db.Contracts
            .SingleOrDefaultAsync(item => item.Id == model.ContractId, cancellationToken);
        if (contract is null || !ContractWorkflow.CanCreateServiceRequest(contract, DateTime.UtcNow))
        {
            ModelState.AddModelError(nameof(model.ContractId), "Choose a contract that is active and within its date range.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateContractsAsync(model, cancellationToken);
            return View(model);
        }

        decimal rate;
        try
        {
            rate = await exchangeRateProvider.GetUsdToZarRateAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException
            or TaskCanceledException
            or InvalidDataException
            or JsonException)
        {
            ModelState.AddModelError(string.Empty, "The currency service is unavailable. Please try again shortly.");
            await PopulateContractsAsync(model, cancellationToken);
            return View(model);
        }

        db.ServiceRequests.Add(new ServiceRequest
        {
            ContractId = contract!.Id,
            Description = model.Description.Trim(),
            UsdCost = model.UsdCost,
            ExchangeRate = rate,
            LocalCost = CurrencyConverter.ConvertUsdToZar(model.UsdCost, rate),
            CreatedUtc = DateTime.UtcNow,
            Status = ServiceRequestStatus.Submitted
        });
        await db.SaveChangesAsync(cancellationToken);

        TempData["Notice"] = "Service request added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> CurrentRate(CancellationToken cancellationToken)
    {
        try
        {
            return Json(new { rate = await exchangeRateProvider.GetUsdToZarRateAsync(cancellationToken) });
        }
        catch (Exception exception) when (exception is HttpRequestException
            or TaskCanceledException
            or InvalidDataException
            or JsonException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Currency service unavailable." });
        }
    }

    private async Task PopulateContractsAsync(ServiceRequestCreateViewModel model, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        model.Contracts = await db.Contracts
            .AsNoTracking()
            .Include(contract => contract.Client)
            .Where(contract => contract.Status == ContractStatus.Active
                && contract.StartDate <= today && contract.EndDate >= today)
            .OrderBy(contract => contract.Reference)
            .Select(contract => new SelectListItem($"{contract.Reference} · {contract.Client!.Name}", contract.Id.ToString()))
            .ToListAsync(cancellationToken);
    }
}