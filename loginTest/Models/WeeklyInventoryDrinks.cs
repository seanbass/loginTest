using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;

namespace InventoryDash.Models
{
    public enum meal
    {
        breakfast, lunch, both
    }

    public class Sandwich
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double? Price { get; set; }
        public meal Meal { get; set; }
        [Display(Name = "Ingredients")]
        public virtual ICollection<Ingredient> Ingredients { get; set; }

        public string GetIngredients(Sandwich sandwich)
        {
            StringBuilder ingredientList = new StringBuilder();
            foreach (Ingredient i in sandwich.Ingredients)
            {
                string ingredientName = i.Name;
                ingredientList.Append(ingredientName);
                ingredientList.Append(", ");
            }
            int index = ingredientList.Length;
            if (index >= 2)
            {
                ingredientList.Remove(index - 2, 2);
            }
            return ingredientList.ToString();
        }
    }
}