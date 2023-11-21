using Microsoft.IdentityModel.Tokens;
using NLog;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

// create instance of Logger
// NLog.Logger logger = UserInteractions.getLogger();
LoggerWithColors logger = new LoggerWithColors();

logger.Info("Main program is running and log manager is started, program is running on a " + (LoggerWithColors.IS_UNIX ? "" : "non-") + "unix-based device.\n");



logger.Info("Program started");


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Categories),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Category_and_Related_Products),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Categories_and_Their_Related_Products),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Category),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Product),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)};

try
{
    var db = new NWContext();
    string menuCheckCommand;
    // MAIN MENU LOOP
    do
    {
        menuCheckCommand = UserInteractions.OptionsSelector(MAIN_MENU_OPTIONS_IN_ORDER, true);

        logger.Info($"User choice: \"{menuCheckCommand}\"");

        if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit))
        {
            logger.Info("Program quitting...");
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Categories))
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Category))
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            // TODO: save category to db
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if (isValid)
            {
               // check for unique name
                if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    // TODO: save category to db
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Category_and_Related_Products)){
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.WriteLine("Select the category whose products you want to display:");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
            }
            Console.ForegroundColor = ConsoleColor.White;
            int id = int.Parse(Console.ReadLine());
            Console.Clear();
            logger.Info($"CategoryId {id} selected");
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            foreach (Product p in category.Products)
            {
                Console.WriteLine($"\t{p.ProductName}");
            }
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Categories_and_Their_Related_Products))
        {
            var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName}");
                foreach (Product p in item.Products)
                {
                    Console.WriteLine($"\t{p.ProductName}");
                }
            }
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Product))
        {
            Product product = new Product
            {
                ProductName = UserInteractions.UserCreatedStringObtainer("Please enter the name of the new product", 1, false, false),
                CategoryId = selectCategory("Please select the product's category").CategoryId,
                Discontinued = false, //New products should not start as discontinued
                // product.QuantityPerUnit = UserInteractions.UserCreatedIntObtainer("Please enter how many there are per unit", 1, int.MaxValue, false).ToString();
                QuantityPerUnit = UserInteractions.UserCreatedStringObtainer("Please enter how many there are per unit", 1, false, false),
                // QuantityPerUnit
            };

            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if (isValid)
            {
               // check for unique name
                if (db.Products.Any(r => r.ProductName == product.ProductName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "ProductName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddProduct(product);
                    logger.Info($"Product added to category \"{product.Category.CategoryName}\" - {product.ProductName}");
                }
            }
            if (!isValid)
            {
                foreach (var result in results)
                {
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
            }
        }
        else
        {
            logger.Warn("That menu option is not available, please try again.");
        }

    } while (menuCheckCommand != enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)); //If user intends to exit the program

}
catch (Exception ex)
{
    logger.Error(ex.Message);
}
logger.Info("Program ended");




Category selectCategory(string selectionMessage){
    var db = new NWContext();
    var all = db.Categories.OrderBy(r => r.CategoryName).ToArray();
    string[] allKeys = new string[all.Count()];
    for(int i = 0; i < allKeys.Length; i++){ allKeys[i] = all[i].CategoryName; }
    string selectedNameKey = UserInteractions.OptionsSelector(allKeys, selectionMessage);
    foreach(var record in all)
    {
        if(record.CategoryName == selectedNameKey){
            return record;
        }
    }
    throw new ArgumentException();
}
Product selectProduct(string selectionMessage){
    var db = new NWContext();
    var all = db.Products.OrderBy(r => r.ProductName).ToArray();
    string[] allKeys = new string[all.Count()];
    for(int i = 0; i < allKeys.Length; i++){ allKeys[i] = all[i].ProductName; }
    string selectedNameKey = UserInteractions.OptionsSelector(allKeys, selectionMessage);
    foreach(var record in all)
    {
        if(record.ProductName == selectedNameKey){
            return record;
        }
    }
    throw new ArgumentException();
}










string enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS mainMenuEnum)
{

    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.Display_Categories => $"Display Categories", // on file (display max amount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT / 11:N0})"
        MAIN_MENU_OPTIONS.Add_Category => "Add Category",
        MAIN_MENU_OPTIONS.Display_Category_and_Related_Products => "Display Category and related products",
        MAIN_MENU_OPTIONS.Display_All_Categories_and_Their_Related_Products => "Display all Categories and their related products",
        MAIN_MENU_OPTIONS.Add_Product => "Add a product",
        _ => "ERROR_MAIN_MENU_OPTION_DOES_NOT_EXIST"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    Display_Categories,
    Add_Category,
    Display_Category_and_Related_Products,
    Display_All_Categories_and_Their_Related_Products,
    Add_Product,
}
