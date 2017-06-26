using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using InventoryDash.DAL;
using InventoryDash.Models;
using System.Dynamic;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Web.UI.WebControls;
using System.Globalization;

namespace InventoryDash.Controllers
{
    public class InventoryController : Controller
    {
        private InventoryContext db = new InventoryContext();

        // GET: Weekly Inventory Data Entry
        public ActionResult Index()
        {

            int weekOfYear = GetCurrentWeekOfYear();
            ViewData["weekOfYear"] = weekOfYear;
            ViewData["startingYear"] = 2016;

            var sandwichesViewModel = (from s in db.Sandwiches
                                       from so in db.WeeklyInventorySandwiches
                                       .Where(orders => s.ID == orders.SandwichId)
                                       .DefaultIfEmpty()
                                       select new WeeklyInventorySandwichesViewModel
                                       {
                                           ID = so.ID,
                                           WeekId = so.WeekId,
                                           SandwichId = s.ID,
                                           QuantityToGo = so.QuantityToGo,
                                           QuantityDineIn = so.QuantityDineIn,
                                           MealId = so.MealId,
                                           Cost = so.Cost,
                                           Income = so.Income,
                                           Name = s.Name,
                                           Price = s.Price,
                                           Meal = s.Meal
                                       }).ToList();


            return View(sandwichesViewModel);
        }

        // POST: Index page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "ID, WeekId, SandwichId, MealId, QuantityToGo, QuantityDineIn, Cost, Income")] WeeklyInventorySandwiches[] weeklyInventorySandwiches)
        {
            int weekOfYear = weeklyInventorySandwiches[0].WeekId;
            //Cycle through the array of Sandwich information and decide if an entry needs to be made.            

            for (int i = 0; i < weeklyInventorySandwiches.Count(); i++)
            {
                if (weeklyInventorySandwiches[i].ID != 0 || weeklyInventorySandwiches[i].QuantityDineIn != 0 || weeklyInventorySandwiches[i].QuantityToGo != 0)
                {
                    //Some quantity information was provided
                    //Calculate the cost and income values
                    weeklyInventorySandwiches[i].Cost = Convert.ToDecimal(CalculateSandwichCost(weeklyInventorySandwiches[i].SandwichId));
                    weeklyInventorySandwiches[i].Income = Convert.ToDecimal(CalculateSandwichIncome(weeklyInventorySandwiches[i].SandwichId));


                    //Determine if a record already exists - 

                    if (weeklyInventorySandwiches[i].ID != 0)
                    {
                        //Yes, then update the record.
                        var sandwichIdToQuery = weeklyInventorySandwiches[i].ID;
                        var record = db.WeeklyInventorySandwiches.SingleOrDefault(x => x.ID == sandwichIdToQuery);
                        record.QuantityDineIn = weeklyInventorySandwiches[i].QuantityDineIn;
                        record.QuantityToGo = weeklyInventorySandwiches[i].QuantityToGo;
                        record.Cost = weeklyInventorySandwiches[i].Cost;
                        record.Income = weeklyInventorySandwiches[i].Income;
                        record.MealId = weeklyInventorySandwiches[i].MealId;
                        record.WeekId = weeklyInventorySandwiches[i].WeekId;
                        db.SaveChanges();
                    }
                    else
                    {
                        //No, add the record.
                        db.WeeklyInventorySandwiches.Add(weeklyInventorySandwiches[i]);
                        db.SaveChanges();
                    }
                }
            }
            //Ensure sandwiches available in multiple meals will show on the list
            //If a weekly inventory record is created for one meal and not the others, then
            // unless records are created for the other meals, then the sandwich will no longer
            // be displayed in the other meal's lists.
            //Get the list of sandwiches marked for both meals
            // For each sandwich, see if there are weeklyInventorySandwich entries for both
            //  meals. If there is not an entry for one of the meals, add it with 0 quantities.
            var sandwichesForBoth = (from s in db.Sandwiches
                                     where s.Meal == InventoryDash.Models.meal.both
                                     select s).ToList();

            foreach (var a in sandwichesForBoth)
            {
                var result = db.WeeklyInventorySandwiches.SingleOrDefault(x => x.SandwichId == a.ID && x.MealId == InventoryDash.Models.meal.breakfast && x.WeekId == weekOfYear);
                if (result == null)
                { // There are no records in breakfast, so add one
                    WeeklyInventorySandwiches newRecord = new WeeklyInventorySandwiches() { Cost = 0, Income = 0, MealId = InventoryDash.Models.meal.breakfast, QuantityDineIn = 0, QuantityToGo = 0, SandwichId = a.ID, WeekId = weekOfYear };
                    db.WeeklyInventorySandwiches.Add(newRecord);
                    db.SaveChanges();
                }

                result = db.WeeklyInventorySandwiches.SingleOrDefault(x => x.SandwichId == a.ID && x.MealId == InventoryDash.Models.meal.lunch && x.WeekId == weekOfYear);
                if (result == null)
                { // There are no records in breakfast, so add one
                    WeeklyInventorySandwiches newRecord = new WeeklyInventorySandwiches() { Cost = 0, Income = 0, MealId = InventoryDash.Models.meal.lunch, QuantityDineIn = 0, QuantityToGo = 0, SandwichId = a.ID, WeekId = weekOfYear };
                    db.WeeklyInventorySandwiches.Add(newRecord);
                    db.SaveChanges();
                }

            }


            return RedirectToAction("Index");
        }

        private double? CalculateSandwichIncome(int sandwichId)
        {
            //Use the sandwich ID to query the Sandwich model to get the current price
            var sandwich = from s in db.Sandwiches
                           where s.ID == sandwichId
                           select s;
            foreach (var a in sandwich)
            {
                return a.Price;
            }
            return null;
        }

        private double? CalculateSandwichCost(int sandwichId)
        {
            //Use the sandwich ID to query the Ingredients to Sandwich join table for a list
            // of ingredients in the sandwich

            //For each ingredient ID, get the cost of the ingredient
            //Total up the costs and send it back.

            //Debugging:
            return 5.50;
        }

        public int GetCurrentWeekOfYear()
        {
            //Getting the week number
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            DateTime date1 = DateTime.Today;
            System.Globalization.Calendar cal = dfi.Calendar;

            int weekOfYear = cal.GetWeekOfYear(date1, dfi.CalendarWeekRule, dfi.FirstDayOfWeek);

            return weekOfYear;
        }
    }
}