using Microsoft.AspNetCore.Mvc;

namespace SYJBD.Views.Shared.Components.Pager
{
    public class PagerViewComponent : ViewComponent
    {
        // recibe cualquier PagedResult<T> como object
        public IViewComponentResult Invoke(object model)
            => View("Default", model);
    }
}
