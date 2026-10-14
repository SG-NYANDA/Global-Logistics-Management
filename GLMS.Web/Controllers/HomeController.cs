using System.Diagnostics;
using GLMS.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Models;

namespace GLMS.Web.Controllers;

public class HomeController(GlmsDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var model = new DashboardViewModel
        {
            ClientCount = await db.Clients.CountAsync(cancellationToken),
            ActiveContractCount = await db.Contracts.CountAsync(contract => contract.Status == ContractStatus.Active
                && contract.StartDate <= today && contract.EndDate >= today, cancellationToken),
            OpenRequestCount = await db.ServiceRequests.CountAsync(request => request.Status == ServiceRequestStatus.Submitted
                || request.Status == ServiceRequestStatus.InProgress, cancellationToken),
            RecentRequests = await db.ServiceRequests
                .AsNoTracking()
                .Include(request => request.Contract)
                .ThenInclude(contract => contract!.Client)
                .OrderByDescending(request => request.CreatedUtc)
                .Take(6)
                .ToListAsync(cancellationToken)
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
