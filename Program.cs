using Microsoft.IdentityModel.Tokens;
using NLog;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using NWConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using NLog.LayoutRenderers;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Reflection.Metadata;

// create instance of Logger
// NLog.Logger logger = UserInteractions.getLogger();
LoggerWithColors logger = new LoggerWithColors();

logger.Info("Main program is running and log manager is started, program is running on a " + (LoggerWithColors.IS_UNIX ? "" : "non-") + "unix-based device.\n");



logger.Info("Program started");


string[] MAIN_MENU_OPTIONS_IN_ORDER = { enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.DisplayEdit_Categories),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Category_and_Related_Products),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Categories_and_Their_Related_Products),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.DisplayEdit_Product),

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
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.DisplayEdit_Categories))
        {
            var query = db.Categories.OrderBy(p => p.CategoryName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine($"{item.CategoryName} - {item.Description}");
            }

            Console.ForegroundColor = UserInteractions.defaultColor;

            if(UserInteractions.UserCreatedBooleanObtainer("Would you like to edit a category", false)){
                Category selectedCategory = selectCategory("Select a category to edit");
                Console.Write("Category current name is \"");
                Console.ForegroundColor = UserInteractions.resultsColor;
                Console.Write($"{selectedCategory.CategoryName}");
                Console.ForegroundColor = UserInteractions.defaultColor;
                Console.Write("\".");
                string newCategoryName = UserInteractions.UserCreatedStringObtainer($"Enter a new name now or leave blank to keep existing", 0, false, false);
                string newCategoryDescription = UserInteractions.UserCreatedStringObtainer($"Enter a new description now or leave blank to keep existing", 0, false, false);
                db.EditCategory(selectedCategory, newCategoryName, newCategoryDescription);
            }

        }
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Category))
        {
            string newCategoryName = UserInteractions.UserCreatedStringObtainer("Please enter the name of the new category", 1, false, false);
            Category newCategory = new()
            {
                CategoryName = newCategoryName,
                Description = UserInteractions.UserCreatedStringObtainer($"Please enter the description of the new category \"{newCategoryName}\"", 1, false, false),
            };
            ValidationContext context = new ValidationContext(newCategory, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(newCategory, context, results, true);
            if (isValid)
            {
                // check for unique name
                if (db.Categories.Any(c => c.CategoryName == newCategory.CategoryName))
                {
                    // generate validation error
                    isValid = false;
                    results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                }
                else
                {
                    logger.Info("Validation passed");
                    db.AddCategory(newCategory);
                    logger.Info($"Category \"{newCategory.CategoryName}\" added.");
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
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Category_and_Related_Products))
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Category selectedCategory = selectCategory("Select the category whose products you want to display:");
            Console.Clear();
            logger.Info($"CategoryId {selectedCategory.CategoryId} selected");
            Console.WriteLine();
            Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == selectedCategory.CategoryId);
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
            displayCategoryProductsActives(db.Categories.Where(c => c.CategoryId == category.CategoryId).OrderBy(p => p.CategoryId));

        }
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Categories_and_Their_Related_Products))
        {
            displayCategoryProductsActives(db.Categories.OrderBy(c => c.CategoryId));
        }
        else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.DisplayEdit_Product))
        {
            string[] locateProductOptions = { "Find by category", "Search by name", "Search by id" };
            string locateMethod = UserInteractions.OptionsSelector(locateProductOptions, true);

            IQueryable<Product> products;
            if (locateMethod == locateProductOptions[0])
            {
                Category productCategory = selectCategory("Please select the product's category");
                products = db.Products.Where(p => p.CategoryId == productCategory.CategoryId);
            }else if(locateMethod == locateProductOptions[1]){
                string userInput = UserInteractions.UserCreatedStringObtainer("Please enter the search name of your product", 1, false, false);
                products = db.Products.Where(p => p.ProductName.Contains(userInput));
            }else{
                string userInput = UserInteractions.UserCreatedIntObtainer("Please enter the id to search for your product", db.Products.Min(p => p.ProductId), db.Products.Max(p => p.ProductId), true).ToString();//TODO: Combine min & max to be more efficient and do beforehand to avoid re-computing
                products = db.Products.Where(p => p.ProductId.ToString().Contains(userInput));
            }
            
            
            // String[] productNames = products.Select(p => p.ProductName).ToArray();
            Product selectedProduct = selectProduct("Pick the product you wish to access", products);
            // Console.WriteLine("");
            displayProduct(selectedProduct);

            if(UserInteractions.UserCreatedBooleanObtainer("Edit this product", false)){
                do{
                    //TODO: Add field data during picking?
                    String[] productFieldOptions = new string[]{"Name","Product Id","Supplier Id","Category Id","Quantity Per Unit","Unit Price","Units In Stock","Units On Order","Reorder Level","Discontinued"};
                    String[] productFields = new string[]{"ProductName","ProductId","SupplierId","CategoryId","QuantityPerUnit","UnitPrice","UnitsInStock","UnitsOnOrder","ReorderLevel","Discontinued"};

                    string selectedField = UserInteractions.OptionsSelector(productFieldOptions);
                    int selectedFieldIndex = Array.IndexOf(productFieldOptions, selectedField);
                    string selectedFieldProperty = productFields[selectedFieldIndex];
                    Type selectedProductType = selectedProduct.GetType();

                    logger.Info($"User selected product field \"{selectedField}\"");
                    var existingValueOrNull = selectedProduct.GetType().GetProperty(selectedFieldProperty).GetValue(selectedProduct, null);
                    string existingValue;
                    if(existingValueOrNull == null){
                        existingValue = "<not set>";//TODO: Add color
                    }else{
                        existingValue = existingValueOrNull.ToString();
                    }
                    Console.WriteLine($"The current \"{selectedField}\" is \"${existingValue}\"");//TODO: Add colors

                    switch(selectedField)
                    {
                        // TODO: MUST HAVE VALIDATION!!!!!
                        case "Name":
                        case "Quantity Per Unit":
                            string newValueString = UserInteractions.UserCreatedStringObtainer($"Please enter a new value for \"{selectedField}\"", 1, false, false);
                            selectedProductType.GetProperty(selectedFieldProperty).SetValue(selectedProduct, newValueString, null);
                            break;
                        case "ProductId":
                            int newValueInt = UserInteractions.UserCreatedIntObtainer($"Please enter a new value for \"{selectedField}\"",0,int.MaxValue,false );
                            selectedProductType.GetProperty(selectedFieldProperty).SetValue(selectedProduct, newValueInt, null);
                            break;
                        case "Supplier Id":
                            // Can be null
                            Supplier? choosenSupplier = selectSupplier("Please select a the new supplier, (or pick none if desired)", null, true);
                            int? newValueSupplierIdOrNull;
                            if(choosenSupplier == null){
                                newValueSupplierIdOrNull = null;
                            }else{
                                newValueSupplierIdOrNull = choosenSupplier.SupplierId;
                            }
                            selectedProductType.GetProperty(selectedFieldProperty).SetValue(selectedProduct, newValueSupplierIdOrNull, null);
                            break;
                        case "Category Id":
                            // Can be null
                            Category? choosenCategory = selectCategoryNull("Please select a the new category, (or pick none if desired)", null, true);
                            int? newValueCategoryIdOrNull;
                            if(choosenCategory == null){
                                newValueCategoryIdOrNull = null;
                            }else{
                                newValueCategoryIdOrNull = choosenCategory.CategoryId;
                            }
                            selectedProductType.GetProperty(selectedFieldProperty).SetValue(selectedProduct, newValueCategoryIdOrNull, null);
                            break;
                        case "Unit Price":
                            decimal? newValueDecimal = (decimal?) UserInteractions.UserCreatedDoubleObtainer($"Please enter a new value for \"{selectedField}\". Any value less than 0 will clear it", double.MinValue, double.MaxValue, false);
                            if(newValueDecimal < 0){
                                newValueDecimal = null;
                            }
                            selectedProductType.GetProperty(selectedFieldProperty).SetValue(selectedProduct, newValueDecimal, null);
                            break;
                        case "Units In Stock":
                        case "Units On Order":
                        case "Reorder Level":
                            short? newValueShort = (short) UserInteractions.UserCreatedIntObtainer($"Please enter a new value for \"{selectedField}\". Any value less than 0 will clear it",0,(int) short.MaxValue,false);
                            if(newValueShort < 0){
                                newValueDecimal = null;
                            }
                            selectedProductType.GetProperty(selectedFieldProperty).SetValue(selectedProduct, newValueShort, null);
                            break;
                        case "Discontinued":
                            bool newValueDiscontinued = UserInteractions.UserCreatedBooleanObtainer($"Pease enter if this product should be discontinued", false);
                            selectedProductType.GetProperty(selectedFieldProperty).SetValue(selectedProduct, newValueDiscontinued, null);
                            break;
                        default:
                            break;
                    }
                    db.SaveChanges();

                    displayProduct(selectedProduct);
                }while(UserInteractions.UserCreatedBooleanObtainer("Continue editing fields", true));
            }
            displayProduct(selectedProduct);

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
                UnitPrice = (decimal)UserInteractions.UserCreatedDoubleObtainer("Please enter the unit price per unit", 0, double.MaxValue, false, 0D, 2),
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
                // TODO: Add check for category?
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




void displayCategoryProductsActives(IOrderedQueryable<Category> categories){
    string[] productFilteringOptions = { "All products", "Discontinued products", "Active products" };
    string selectedProductFilter = UserInteractions.OptionsSelector(productFilteringOptions, true);

    categories = categories.Include("Products").OrderBy(p => p.CategoryId);

    foreach (Category category in categories)
    {
        Console.ForegroundColor = UserInteractions.resultsColor;
        Console.WriteLine($"\n{category.CategoryName}");
        foreach (Product product in category.Products)
        {
            bool discontinued = product.Discontinued;
            if(discontinued && (selectedProductFilter == productFilteringOptions[0] || selectedProductFilter == productFilteringOptions[1])){
                Console.ForegroundColor = UserInteractions.discontinuedColor;
                Console.WriteLine($"\t{product.ProductName}");
            }else if(selectedProductFilter == productFilteringOptions[0] || selectedProductFilter == productFilteringOptions[2]){
                Console.ForegroundColor = UserInteractions.resultsColor;
                Console.WriteLine($"\t{product.ProductName}");
            }
        }
    }

    Console.ForegroundColor = UserInteractions.defaultColor;
}




Category selectCategory(string selectionMessage)
{
    var db = new NWContext();
    var all = db.Categories.OrderBy(r => r.CategoryName).ToArray();
    string[] allKeys = new string[all.Count()];
    for (int i = 0; i < allKeys.Length; i++) { allKeys[i] = all[i].CategoryName; }
    string selectedNameKey = UserInteractions.OptionsSelector(allKeys, selectionMessage);
    foreach (var record in all)
    {
        if (record.CategoryName == selectedNameKey)
        {
            return record;
        }
    }
    throw new ArgumentException();
}

Product selectProduct(string selectionMessage, IQueryable<Product> products, bool showCategories = false)
{
    if (products == null)
    {
        products = new NWContext().Products.OrderBy(r => r.ProductName);
    }
    var selectableProducts = products.ToArray();
    string[] allKeys = new string[selectableProducts.Count()];
    for (int i = 0; i < allKeys.Length; i++) { allKeys[i] = selectableProducts[i].ProductName; }
    string selectedNameKey = UserInteractions.OptionsSelector(allKeys, selectionMessage);
    foreach (var record in selectableProducts)
    {
        if (record.ProductName == selectedNameKey)
        {
            return record;
        }
    }
    throw new ArgumentException();
}


Supplier? selectSupplier(string selectionMessage, IQueryable<Supplier> suppliers, bool addNone)
{
    string noSupplierCompanyName = "(None) NO SUPPLIER";
    if (suppliers == null)
    {
        suppliers = new NWContext().Suppliers.OrderBy(s => s.CompanyName);
    }
    // if(addNone){
    //     Supplier tempNoneSupplier = new Supplier
    //     {
    //         CompanyName = noSupplierCompanyName
    //     };
    //     // suppliers = suppliers.Prepend(tempNoneSupplier);
    // }
    
    Supplier[] selectableSuppliers = suppliers.ToArray();

    if(addNone){
        Supplier[] selectableSuppliersWithNone = new Supplier[suppliers.Count()+1];
        for(int i = 0; i < selectableSuppliers.Length; i++)
        {
            selectableSuppliersWithNone[i+1] = selectableSuppliers[i];
        }
        selectableSuppliersWithNone[0] = new Supplier{ CompanyName = noSupplierCompanyName };

        selectableSuppliers = selectableSuppliersWithNone;
    }

    string[] allKeys = new string[selectableSuppliers.Count()];
    for (int i = 0; i < allKeys.Length; i++) { allKeys[i] = selectableSuppliers[i].CompanyName; }
    string selectedNameKey = UserInteractions.OptionsSelector(allKeys, selectionMessage);
    foreach (var record in selectableSuppliers)
    {
        if (record.CompanyName == selectedNameKey)
        {
            if(string.Equals(selectedNameKey, noSupplierCompanyName)){
                return null;
            }else{
                return record;
            }
        }
    }
    return null;
}

Category? selectCategoryNull(string selectionMessage, IQueryable<Category> categories, bool addNone)
{
    string noCategoryName = "(None) NO CATEGORY";
    if (categories == null)
    {
        categories = new NWContext().Categories.OrderBy(c => c.CategoryName);
    }
    Category[] selectableCategories = categories.ToArray();

    if(addNone){
        Category[] selectableSuppliersWithNone = new Category[categories.Count()+1];
        for(int i = 0; i < selectableCategories.Length; i++)
        {
            selectableSuppliersWithNone[i+1] = selectableCategories[i];
        }
        selectableSuppliersWithNone[0] = new Category{ CategoryName = noCategoryName };

        selectableCategories = selectableSuppliersWithNone;
    }

    string[] allKeys = new string[selectableCategories.Count()];
    for (int i = 0; i < allKeys.Length; i++) { allKeys[i] = selectableCategories[i].CategoryName; }
    string selectedNameKey = UserInteractions.OptionsSelector(allKeys, selectionMessage);
    foreach (var record in selectableCategories)
    {
        if (record.CategoryName == selectedNameKey)
        {
            if(string.Equals(selectedNameKey, noCategoryName)){
                return null;
            }else{
                return record;
            }
        }
    }
    return null;
}

void displayProduct(Product product)
{

    // Console.WriteLine($"{product.Category.CategoryName}");//Crashes for some reason
    // Replace "QuantityPerUnit" with the longest string to be used.
    int indentLevel = "Quantity Per Unit".Length;

    displayField("Name", product.ProductName, indentLevel, false);//string
    displayField("Id", product.ProductId, indentLevel, false);//int
    displayField("Supplier Id", product.SupplierId, indentLevel, false);//int?
    displayField("Category Id", product.CategoryId, indentLevel, false);//int?
    displayField("Quantity Per Unit", product.QuantityPerUnit, indentLevel, false);//string
    displayField("Unit Price ($)", product.UnitPrice, indentLevel, false, 2);//decimal?
    displayField("Units In Stock", product.UnitsInStock, indentLevel, false);//short?
    displayField("Units On Order", product.UnitsOnOrder, indentLevel, false);//short?
    displayField("Reorder Level", product.ReorderLevel, indentLevel, false);//short?
    displayField("Discontinued", product.Discontinued, indentLevel, false);//bool

    // public virtual Category Category { get; set; }
    // public virtual Supplier Supplier { get; set; }
    // public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}

void displayField<T>(string recordName, T recordValue, int indentLevel, bool blankIfNull, int decimalPlaces = -1)
{
    if(indentLevel == -1){
        indentLevel = 0;
    }
    if(decimalPlaces < -1){
        logger.Warn("Display field decimal spaces should not be lower than -1. Argument problem caught before error. Handling...");
        decimalPlaces = -1;
    }
    ConsoleColor existingColor = Console.ForegroundColor;

    Console.ForegroundColor = UserInteractions.displayColor;
    // string labelLine = $"{recordName.PadRight(indentLevel)}";
    string labelLine = $"{recordName}";
    while(labelLine.Length < indentLevel){
        labelLine += UserInteractions.formattingRowLineHelper;
    }
    Console.Write($"{labelLine}:");
    Console.ForegroundColor = UserInteractions.defaultColor;

    if (recordValue == null)
    {
        Console.WriteLine();
    }
    else
    {
        if(recordValue is decimal && decimalPlaces != -1){
            string formattedValue = string.Format("{0:F"+decimalPlaces+"}", recordValue);//TODO: Make with with currencies instead of just floating points
            Console.Write($" {formattedValue}\n");
        }
        else
        {
            Console.Write($" {recordValue}\n");//TODO: Space consistency between?
        }
    }
    Console.ForegroundColor = existingColor;
}


string enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS mainMenuEnum)
{

    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.DisplayEdit_Categories => $"Display/Edit Categories", // on file (display max amount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT / 11:N0})"
        MAIN_MENU_OPTIONS.Add_Category => "Add Category",
        MAIN_MENU_OPTIONS.Display_Category_and_Related_Products => "Display a Category and it's related products",
        MAIN_MENU_OPTIONS.Display_All_Categories_and_Their_Related_Products => "Display all Categories and their related products",
        MAIN_MENU_OPTIONS.Add_Product => "Add a product",
        MAIN_MENU_OPTIONS.DisplayEdit_Product => "Display/Edit a specific product",
        _ => "ERROR_MAIN_MENU_OPTION_DOES_NOT_EXIST"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    DisplayEdit_Categories,
    Add_Category,
    Display_Category_and_Related_Products,
    Display_All_Categories_and_Their_Related_Products,
    Add_Product,
    DisplayEdit_Product
}
