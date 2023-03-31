using System.Web.Mvc;

namespace Buoi6_TrenLop.Areas.Admin.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                // Chuyển hướng đến trang đăng nhập nếu không đăng nhập
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else if (filterContext.HttpContext.User.Identity.IsAuthenticated && !filterContext.HttpContext.User.IsInRole("Admin"))
            {
                // Chuyển hướng đến trang báo lỗi nếu người dùng đã đăng nhập nhưng không có quyền admin
                filterContext.Result = new ViewResult { ViewName = "~/Areas/Admin/Views/Shared/Error.cshtml" };
            }
        }
    }
}