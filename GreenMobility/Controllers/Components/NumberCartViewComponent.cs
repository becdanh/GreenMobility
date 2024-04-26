using System;
using System.Collections.Generic;
using GreenMobility.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GreenMobility.Extension;


namespace GreenMobility.Controllers.Components
{
    public class NumberCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItemVM>>("RentalCart");
            return View(cart);
        }
    }
}
