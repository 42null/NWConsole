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
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Create_Post),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Posts),
                                        enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)};


string menuCheckCommand;

// MAIN MENU LOOP
// do
// {
//     menuCheckCommand = UserInteractions.OptionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

//     logger.Info($"User choice: \"{menuCheckCommand}\"");

//     if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit))
//     {
//         logger.Info("Program quitting...");
//     }
// } while (menuCheckCommand != enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)); //If user intends to exit the program



try
{
    var db = new NWContext();
    string choice;
    do
    {
        Console.WriteLine("1) Display Categories");
        Console.WriteLine("2) Add Category");
        Console.WriteLine("\"q\" to quit");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Option {choice} selected");
        if (choice == "1")
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
        else if (choice == "2")
        {
            Category category = new Category();
            Console.WriteLine("Enter Category Name:");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter the Category Description:");
            category.Description = Console.ReadLine();
            // TODO: save category to db
        }
        Console.WriteLine();
    } while (choice.ToLower() != "q");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}

logger.Info("Program ended");






// MAIN MENU LOOP
// do
// {
//     menuCheckCommand = UserInteractions.OptionsSelector(MAIN_MENU_OPTIONS_IN_ORDER);

//     logger.Info($"User choice: \"{menuCheckCommand}\"");

//     if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit))
//     {
//         logger.Info("Program quitting...");
//     }
//     else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_All_Blogs))
//     {
//         // Display all blogs
//         Console.WriteLine("All blogs located in the database (by name):\n");
//         try
//         {
//             var query = getAllBlogs();
//             Console.ForegroundColor = UserInteractions.resultsColor;

//             foreach (var item in query)
//             {
//                 Console.WriteLine(item.Name);
//             }
//             Console.ForegroundColor = UserInteractions.defaultColor;
//             int itemCount = query.Count();
//             Console.Write($"\nThere were ");
//             Console.ForegroundColor = UserInteractions.resultsColor;
//             Console.Write($"{itemCount} ");
//             Console.ForegroundColor = UserInteractions.defaultColor;
//             Console.Write($"blog{(itemCount==1?"":"s")}");
//             Console.ForegroundColor = UserInteractions.defaultColor;
//             Console.WriteLine(" located in the database.");
//         }
//         catch (Exception ex)
//         {
//             logger.Error(ex.Message);
//         }
//     }
//     else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Add_Blog))
//     {
//         // Create and save a new Blog
//         Console.Write("Enter a name for the new Blog: ");
//         var name = Console.ReadLine();
//         try
//         {
//             var blog = new Blog { Name = name };

//             var db = new BloggingContext();
//             db.AddBlog(blog);
//             logger.Info($"Blog added - {name}");

//             //TODO: Display all blogs?
//         }
//         catch (Exception ex)
//         {
//             logger.Error(ex.Message);
//         }
//     }
//     else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Create_Post))
//     {
//         Blog selectedBlog = selectBlog("Please select a blog for your new post: ");

//         string postTitle = UserInteractions.UserCreatedStringObtainer("Please enter the title of the new post", 1, false, false);
//         string postContent = UserInteractions.UserCreatedStringObtainer("Please enter the content for the new post", 1, false, false);
//         try
//         {
//             Post post = new Post {
//                 Title = postTitle,
//                 Content = postContent,
//                 BlogId = selectedBlog.BlogId
//             };

//             var db = new BloggingContext();
//             db.AddPost(post);
//             logger.Info($"Post added to blog \"{selectedBlog}\" - {postTitle}");

//             //TODO: Display all posts?
//         }
//         catch (Exception ex)
//         {
//             logger.Error(ex.Message);
//         }
//     }
//     else if (menuCheckCommand == enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Display_Posts))
//     {
//         Blog selectedBlog = selectBlog("Please select a blog whose posts you wish to view: ");
//         int postsCount;
//         try{
//             postsCount = selectedBlog.Posts.Count;
//         }catch(NullReferenceException error){
//             //Just make it equal to 0 as count fails if it is empty
//             postsCount = 0;
//         }

//         Console.Write("You have selected blog ");
//         Console.ForegroundColor = UserInteractions.resultsColor;
//         Console.Write($"{selectedBlog.Name}");
//         Console.ForegroundColor = UserInteractions.defaultColor;
//         Console.Write(" which contains ");
//         Console.ForegroundColor = UserInteractions.resultsColor;
//         Console.Write($"{postsCount}");
//         Console.ForegroundColor = UserInteractions.defaultColor;
//         Console.WriteLine($" post{(postsCount==1?"":"s")}.\n");

//         Console.ForegroundColor = UserInteractions.resultsColor;
//         if(postsCount > 0){
//             Console.WriteLine($"Blog: \"{selectedBlog.Name}\"");
//             foreach(Post post in selectedBlog.Posts){
//                 string titleLine = $"{selectedBlog.Name} - Post: ";
//                 string innerIndentLine = new string[titleLine.Length-8].Aggregate((c, n) => $"{c} ");

//                 Console.WriteLine($"{titleLine}{post.Title}");
//                 Console.WriteLine($"{innerIndentLine}Content: {post.Content}");
//                 Console.WriteLine();
//             }
//         }else{
//             Console.WriteLine($"Blog \"{selectedBlog.Name}\" does not contain any posts.");
//         }
//         Console.ForegroundColor = UserInteractions.defaultColor;

//     // Once the Blog is selected, all Posts related to the selected blog should be display as well as the number of Posts
//     // For each Post, display the Blog name, Post title and Post content

//     }

//     else
//     {
//         logger.Warn("That menu option is not available, please try again.");
//     }

// } while (menuCheckCommand != enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS.Exit)); //If user intends to exit the program

// logger.Info("Program ended");





// IOrderedQueryable<Blog> getAllBlogs(){
//     var db = new BloggingContext();
//     // Get all Blogs from the database
//     var query = db.Blogs.Include("Posts").OrderBy(b => b.Name);
//     // var query = db.Blogs.OrderBy(b => b.Name);

//     return query;
// }

// Blog selectBlog(string selectionMessage){
//     Blog[] allBlogs = getAllBlogs().ToArray();
//     string[] allBlogKeys = new string[allBlogs.Count()];

//     for(int i = 0; i < allBlogKeys.Length; i++){
//         allBlogKeys[i] = allBlogs[i].Name;
//     }

//     string selectedBlogNameKey = UserInteractions.OptionsSelector(allBlogKeys, selectionMessage);
//     Blog selectedBlog;

//     // Find the blog that matches the key
//     foreach(Blog blog in allBlogs)
//     {
//         if(blog.Name == selectedBlogNameKey){
//             selectedBlog = blog;
//             return blog;
//         }
//     }
//     return new Blog();
// }




string enumToStringMainMenuWorkaround(MAIN_MENU_OPTIONS mainMenuEnum)
{


    return mainMenuEnum switch
    {
        MAIN_MENU_OPTIONS.Exit => "Quit program",
        MAIN_MENU_OPTIONS.Display_Categories => $"Display all blogs", // on file (display max amount is {UserInteractions.PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT / 11:N0})"
        MAIN_MENU_OPTIONS.Add_Category => "Add Blog",
        MAIN_MENU_OPTIONS.Create_Post => "Create New Post",
        MAIN_MENU_OPTIONS.Display_Posts => "Display Posts",
        _ => "ERROR_MAIN_MENU_OPTION_DOES_NOT_EXIST"
    };
}

public enum MAIN_MENU_OPTIONS
{
    Exit,
    Display_Categories,
    Add_Category,
    Create_Post,
    Display_Posts
}
