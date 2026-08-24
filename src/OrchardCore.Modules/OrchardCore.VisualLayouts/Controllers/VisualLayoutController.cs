using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.VisualLayouts.Models;
using OrchardCore.VisualLayouts.Services;
using OrchardCore.VisualLayouts.ViewModels;

namespace OrchardCore.VisualLayouts.Controllers;

[Admin("VisualLayouts/{action}/{name?}", "VisualLayouts.{action}")]
public sealed class VisualLayoutController : Controller
{
    private const string _optionsSearch = "Options.Search";

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IAuthorizationService _authorizationService;
    private readonly VisualLayoutsManager _visualLayoutsManager;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public VisualLayoutController(
        IAuthorizationService authorizationService,
        VisualLayoutsManager visualLayoutsManager,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        IStringLocalizer<VisualLayoutController> stringLocalizer,
        IHtmlLocalizer<VisualLayoutController> htmlLocalizer,
        INotifier notifier)
    {
        _authorizationService = authorizationService;
        _visualLayoutsManager = visualLayoutsManager;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("VisualLayouts", "VisualLayouts.Index")]
    public async Task<IActionResult> Index(VisualLayoutIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageVisualLayouts))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
        var visualLayoutsDocument = await _visualLayoutsManager.GetVisualLayoutsDocumentAsync();

        var visualLayouts = visualLayoutsDocument.VisualLayouts.ToList();

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            visualLayouts = visualLayouts.Where(x => x.Key.Contains(options.Search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var count = visualLayouts.Count;

        visualLayouts = visualLayouts.OrderBy(x => x.Key)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize).ToList();

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);
        var model = new VisualLayoutIndexViewModel
        {
            VisualLayouts = visualLayouts.Select(x => new VisualLayoutEntry { Name = x.Key, VisualLayout = x.Value }).ToList(),
            Options = options,
            Pager = pagerShape,
        };

        model.Options.VisualLayoutsBulkAction =
        [
            new SelectListItem(S["Delete"], nameof(VisualLayoutsBulkAction.Remove)),
        ];

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(VisualLayoutIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search },
        });

    public async Task<IActionResult> Create(string name = null, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageVisualLayouts))
        {
            return Forbid();
        }

        var model = new VisualLayoutViewModel
        {
            Name = name,
            State = SerializeVisualLayout(new VisualLayout()),
        };

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost, ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(VisualLayoutViewModel model, string submit, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageVisualLayouts))
        {
            return Forbid();
        }

        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            await ValidateModelAsync(model);
        }

        if (ModelState.IsValid)
        {
            var visualLayout = DeserializeVisualLayout(model.State);
            visualLayout.Description = model.Description;

            await _visualLayoutsManager.UpdateVisualLayoutAsync(model.Name, visualLayout);

            await _notifier.SuccessAsync(H["The \"{0}\" visual layout has been created.", model.Name]);

            if (submit == "SaveAndContinue")
            {
                return RedirectToAction(nameof(Edit), new { name = model.Name, returnUrl });
            }
            else
            {
                return RedirectToReturnUrlOrIndex(returnUrl);
            }
        }

        // If we got this far, something failed, redisplay form.
        return View(model);
    }

    public async Task<IActionResult> Edit(string name, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageVisualLayouts))
        {
            return Forbid();
        }

        var visualLayoutsDocument = await _visualLayoutsManager.GetVisualLayoutsDocumentAsync();

        if (!visualLayoutsDocument.VisualLayouts.TryGetValue(name, out var visualLayout))
        {
            return RedirectToAction(nameof(Create), new { name, returnUrl });
        }

        var model = new VisualLayoutViewModel
        {
            Name = name,
            Description = visualLayout.Description,
            State = SerializeVisualLayout(visualLayout),
        };

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string sourceName, VisualLayoutViewModel model, string submit, string returnUrl = null)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageVisualLayouts))
        {
            return Forbid();
        }

        var visualLayoutsDocument = await _visualLayoutsManager.LoadVisualLayoutsDocumentAsync();

        if (ModelState.IsValid)
        {
            await ValidateModelAsync(model, visualLayoutsDocument, sourceName);
        }

        if (!visualLayoutsDocument.VisualLayouts.ContainsKey(sourceName))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var visualLayout = DeserializeVisualLayout(model.State);
            visualLayout.Description = model.Description;

            await _visualLayoutsManager.RemoveVisualLayoutAsync(sourceName);

            await _visualLayoutsManager.UpdateVisualLayoutAsync(model.Name, visualLayout);

            if (submit != "SaveAndContinue")
            {
                return RedirectToReturnUrlOrIndex(returnUrl);
            }
        }

        // If we got this far, something failed, redisplay form.
        ViewData["ReturnUrl"] = returnUrl;

        // If the name was changed or removed, prevent a 404 or a failure on the next post.
        model.Name = sourceName;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageVisualLayouts))
        {
            return Forbid();
        }

        var visualLayoutsDocument = await _visualLayoutsManager.LoadVisualLayoutsDocumentAsync();

        if (!visualLayoutsDocument.VisualLayouts.ContainsKey(name))
        {
            return NotFound();
        }

        await _visualLayoutsManager.RemoveVisualLayoutAsync(name);

        await _notifier.SuccessAsync(H["Visual layout deleted successfully."]);

        return RedirectToReturnUrlOrIndex(returnUrl);
    }

    [HttpPost, ActionName("Index")]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> ListPost(VisualLayoutIndexOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageVisualLayouts))
        {
            return Forbid();
        }

        switch (options.BulkAction)
        {
            case VisualLayoutsBulkAction.None:
                break;
            case VisualLayoutsBulkAction.Remove:
                if (itemIds != null)
                {
                    var visualLayoutsDocument = await _visualLayoutsManager.LoadVisualLayoutsDocumentAsync();
                    var checkedItemIds = visualLayoutsDocument.VisualLayouts.Keys
                        .Intersect(itemIds, StringComparer.OrdinalIgnoreCase);

                    foreach (var id in checkedItemIds)
                    {
                        await _visualLayoutsManager.RemoveVisualLayoutAsync(id);
                    }

                    await _notifier.SuccessAsync(H["Visual layouts successfully removed."]);
                }

                break;
            default:
                return BadRequest();
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult RedirectToReturnUrlOrIndex(string returnUrl)
    {
        if ((string.IsNullOrEmpty(returnUrl) == false) && (Url.IsLocalUrl(returnUrl)))
        {
            return this.Redirect(returnUrl, true);
        }
        else
        {
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task ValidateModelAsync(VisualLayoutViewModel model, VisualLayoutsDocument visualLayoutsDocument = null, string sourceName = null)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(VisualLayoutViewModel.Name), S["The name is mandatory."]);
        }
        else
        {
            visualLayoutsDocument ??= await _visualLayoutsManager.GetVisualLayoutsDocumentAsync();

            if (!model.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase) &&
                visualLayoutsDocument.VisualLayouts.ContainsKey(model.Name))
            {
                ModelState.AddModelError(nameof(VisualLayoutViewModel.Name), S["A visual layout with the same name already exists."]);
            }
        }

        if (string.IsNullOrWhiteSpace(model.State))
        {
            ModelState.AddModelError(nameof(VisualLayoutViewModel.State), S["The layout state is mandatory."]);
            return;
        }

        try
        {
            _ = DeserializeVisualLayout(model.State);
        }
        catch (JsonException)
        {
            ModelState.AddModelError(nameof(VisualLayoutViewModel.State), S["The layout state is not a valid JSON document."]);
        }
    }

    private static string SerializeVisualLayout(VisualLayout visualLayout)
        => JsonSerializer.Serialize(visualLayout, _jsonSerializerOptions);

    private static VisualLayout DeserializeVisualLayout(string state)
        => JsonSerializer.Deserialize<VisualLayout>(state, _jsonSerializerOptions);
}
