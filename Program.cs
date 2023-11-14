using Microsoft.IdentityModel.Tokens;
using NLog;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// create instance of Logger
// NLog.Logger logger = UserInteractions.getLogger();
LoggerWithColors logger = new LoggerWithColors();

logger.Info("Main program is running and log manager is started, program is running on a " + (LoggerWithColors.IS_UNIX ? "" : "non-") + "unix-based device.\n");



logger.Info("Program started");


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Categories),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Category),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)};



try
{
    var db = new NWContext();
    string menuCheckCommand;
    // MAIN MENU LOOP
    do
    {
        menuCheckCommand = UserInteractions.OptionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

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




string enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS mainMenuEnum)
{

    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.Display_Categories => $"Display Categories", // on file (display max amount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT / 11:N0})"
        MAIN_MENU_OPTIONS.Add_Category => "Add Category",
        _ => "ERROR_MAIN_MENU_OPTION_DOES_NOT_EXIST"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    Display_Categories,
    Add_Category,
}
