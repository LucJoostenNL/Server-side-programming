using Client.Extentsions.Dish;
using Client.Models.Account;
using Domain;
using DomainServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Client.Extentsions.Meal;
using System.Globalization;
using Client.Models.Order;
using Newtonsoft.Json.Schema;

namespace Client.Controllers
{
    public class OrderController : Controller
    {
        private readonly IMealService _mealService;
        private readonly IDishService _dishService;
        private Cart _cart;
        private string _startDate = "";

        public OrderController(IMealService service, IDishService dishService, Cart cartService)
        {
            _mealService = service;
            _dishService = dishService;
            _cart = cartService;
        }


        public IActionResult Checkout()
        {
            return View();
        }

        /** WERKT **/
        public IActionResult ChooseWeek() => View();

        /** WERKT **/
        [HttpPost]
        public IActionResult ChooseWeek(ChooseWeekViewModel model)
        {
            Debug.WriteLine("dates ------------> " + model.Start);
            _cart.EndDate = model.End.ToString();
            _cart.StartDate = model.Start.ToString();
            Debug.WriteLine(_cart.StartDate);
            _cart.Save();
            TempData["start"] = model.Start.ToString();
            if (!User.Identity.IsAuthenticated)
            {
                TempData["start"] = model.Start;
                TempData["end"] = model.End;
                return RedirectToAction("Order", "Order");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        /** WERKT **/
        public IActionResult Order()
        {
            //// Fetching Dishes into local JArray
            //JArray dishArray = await DishMethods.GetDishes();
            //// Converting JArray items to Collection object of given type
            //List<Dish> allDishes = dishArray.ToObject<List<Dish>>();

            var allDishes = _dishService.GetDishes();

            Dictionary<int, List<Meal>> dict = new Dictionary<int, List<Meal>>();

            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek))) {
                dict.Add((int) day, new List<Meal>());
            }

            /*if (_startDate == "") {
                if (TempData["start"] != null) {
                    _startDate = TempData["start"].ToString();
                } else { _startDate = DateTime.Now.ToString(); }
            }*/
            Debug.WriteLine("CART ----------------->  "+ _cart);

            _startDate = DateTime.Now.ToString();
            TempData["start"] = _cart.StartDate;

            DateTime startDate = DateTime.Parse(_startDate);

            //var meals = MealMethods.GetAllWeekMeals(startDate);

            var meals = GetAllWeekMeals(startDate);

            foreach (var meal in meals)
            {
                var day = meal.DateValid.DayOfWeek;
                dict[(int)day].Add(meal);
            }

            List<MealDishes> mealDishes = new List<MealDishes>();

            var dishes = _mealService.MealDish.ToList();


            foreach (var item in dishes)
            {
                mealDishes.Add(item);
            }

            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.MealDishes = mealDishes;
                ViewBag.Dishes = allDishes;
                ViewBag.Dictionary = dict;
                ViewBag.StartDate = startDate.ToString();
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpPost]
        public IActionResult Order(OrderMealViewModel model)
        {
            // posting new order and redirect to order detail with details of the meal dishes when state is valid
            RedirectToAction("OrderDetail", "Order");
            return View(model);
        }

        public IEnumerable<Meal> GetAllWeekMeals(DateTime date)
        {
            return _mealService.Meal.Where(m => Week(m.DateValid) == Week(date));
        }

        public int Week(DateTime date)
        {
            GregorianCalendar cal = new GregorianCalendar(GregorianCalendarTypes.Localized);
            return cal.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }

        public IActionResult AddToCart(int id, DayOfWeek day) {
            Debug.WriteLine("ID -------------------------------->>>> " + id + " Day -> " + day);
            return RedirectToAction("Order");
        }
    }

   
}