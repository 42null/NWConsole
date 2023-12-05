using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.JSInterop.Infrastructure;
using NLog;
/**
    UserInteractions is a class to assist in getting input from the user (and sometimes displaying information) in other to standardize common requests
*/
public sealed class UserInteractions
{ //Sealed to prevent inheritance
    public const int PRINTOUT_RESULTS_MAX_TERMINAL_SPACE_HEIGHT = 1_000; //Tested, >~ 1,000 line before removal, use int.MaxValue for infinity, int's length is max for used lists
    public const char formattingRowLineHelper = '-';
    static LoggerWithColors loggerWithColors = new LoggerWithColors();


    public static ConsoleColor defaultColor = ConsoleColor.White;
    public static ConsoleColor userColor = ConsoleColor.Yellow;
    public static ConsoleColor seperatorColor = defaultColor;
    public static ConsoleColor selectDefaultOptionColor = ConsoleColor.Black;
    public static ConsoleColor selectDefaultOptionColorBackground = ConsoleColor.Gray;


    public static ConsoleColor infoColor = ConsoleColor.Cyan;
    public static ConsoleColor resultsColor = ConsoleColor.DarkMagenta; //ConsoleColor.Green
    public static ConsoleColor debugColor = ConsoleColor.Magenta;
    public static ConsoleColor traceColor = ConsoleColor.DarkMagenta;
    public static ConsoleColor warnColor = ConsoleColor.Red; //ConsoleColor.DarkYellow;
    public static ConsoleColor errorColor = ConsoleColor.Red;
    public static ConsoleColor fatalColor = ConsoleColor.DarkRed;

    public static ConsoleColor discontinuedColor = ConsoleColor.DarkGray;


    public static ConsoleColor displayColor = resultsColor;
    public static ConsoleColor addColor = ConsoleColor.Blue;
    public static ConsoleColor editColor = ConsoleColor.Green;
    public static ConsoleColor removeColor = ConsoleColor.Red;


    // public static NLog.Logger getLogger() { return logger; }

    public static string OptionsSelector(string[] options){
        return OptionsSelector(options,"Please select an option from the following...");
    }
    public static string OptionsSelector(string[] options, bool colorFirstWordsWithDefault){
        return OptionsSelector(options,"Please select an option from the following...", colorFirstWordsWithDefault);
    }
    public static string OptionsSelector(string[] options, string customSelectMessage){
        return OptionsSelector(options, customSelectMessage, false);
    }
    public static string OptionsSelector(string[] options, string customSelectMessage, bool colorFirstWordsWithDefault)
    {
        string userInput;
        int selectedNumber;
        bool userInputWasImproper = true;
        List<int> cleanedListIndexes = new List<int> { };
        string optionsTextAsStr = ""; //So only created once. Requires change if adjustable width is added

        for (int i = 0; i < options.Length; i++)
        {
            // options[i] = options[i].Trim();//Don't trim so when used, spaces can be used to do spacing
            if (options[i] != null && options[i].Replace(" ", "").Length > 0)
            {//Ensure that not empty or null
                cleanedListIndexes.Add(i);//Add index to list
                optionsTextAsStr = $"{optionsTextAsStr}\n{string.Format($" {{0,{options.Length.ToString().Length}}}) {{1}}", cleanedListIndexes.Count(), options[i])}";//Have to use this as it prevents the constants requirements.
            }
        }
        optionsTextAsStr = optionsTextAsStr.Substring(1); //Remove first \n

        // Separate from rest by adding a blank line
        Console.WriteLine();
        do
        {
            Console.WriteLine(customSelectMessage);
            if(colorFirstWordsWithDefault){
                // Console.BackgroundColor = ConsoleColor.DarkGreen;
                ConsoleColor existingColor = Console.ForegroundColor;

                string[] splitIntoLines = optionsTextAsStr.Split("\n");
                foreach(string line in splitIntoLines)
                {
                    int firstSpaceIndex = line.IndexOf(") ")+2;
                    int afterFirstIndex = line.Substring(firstSpaceIndex).IndexOf(" ");

                    string beforeWord = line.Substring(0,firstSpaceIndex);
                    Console.Write(beforeWord);

                    string firstWord = line.Substring(firstSpaceIndex,afterFirstIndex);
                    Boolean firstWordShown = false;
                    if(firstWord.ToLower() == "display"){
                        Console.ForegroundColor = displayColor;
                    }else if(firstWord.ToLower() == "add"){
                        Console.ForegroundColor = addColor;
                    }else if(firstWord.ToLower() == "edit"){
                        Console.ForegroundColor = editColor;
                    }else if(firstWord.ToLower() == "quit" || firstWord.ToLower() == "delete" || firstWord.ToLower() == "remove"){
                        Console.ForegroundColor = removeColor;

                    }else if(firstWord.Contains('/')){//TODO: Improve processing
                        int indexOfSeparator = firstWord.IndexOf('/');
                        Console.ForegroundColor = displayColor;
                        Console.Write($"{firstWord.Substring(0,indexOfSeparator)}");
                        Console.ForegroundColor = seperatorColor;
                        Console.Write("/");
                        Console.ForegroundColor = editColor;
                        Console.Write($"{firstWord.Substring(indexOfSeparator+1)}");
                        Console.ForegroundColor = defaultColor;
                        firstWordShown = true;
                    }

                    if(!firstWordShown){ Console.Write(firstWord); }

                    Console.ForegroundColor = existingColor;
                    Console.WriteLine(line.Substring(firstSpaceIndex+afterFirstIndex));
                }
            }else{
                Console.WriteLine(optionsTextAsStr);
            }
            Console.Write("Please enter an option from the list: ");
            Console.ForegroundColor = userColor;
            userInput = Console.ReadLine().Trim();
            Console.ForegroundColor = defaultColor;

            //TODO: Move to switch without breaks instead of ifs or if-else-s?
            if (!int.TryParse(userInput, out selectedNumber))
            {// User response was not a integer
                loggerWithColors.Warn("Your selector choice was not a integer, please try again.");
            }
            else if (selectedNumber < 1 || selectedNumber > cleanedListIndexes.Count()) //Is count because text input index starts at 1
            {// User response was out of bounds
                loggerWithColors.Warn($"Your selector choice was not within bounds, please try again. (Range is 1-{cleanedListIndexes.Count()})");
            }
            else
            {
                userInputWasImproper = false;
            }
        } while (userInputWasImproper);
        // Separate from rest by adding a blank line
        Console.WriteLine();
        return options[cleanedListIndexes[selectedNumber - 1]];
    }

    public static string UserCreatedStringObtainer(string message, int minimumCharactersAllowed, bool showMinimum, bool keepRaw)
    {
        if (minimumCharactersAllowed < 0)
        {
            minimumCharactersAllowed = 0;
        }
        string userInput = null;

        do
        {
            Console.Write($"\n{message}{(showMinimum ? $" (must contain at least {minimumCharactersAllowed} character{(minimumCharactersAllowed == 1 ? "" : "s")})" : "")}: ");
            userInput = getInputWithColor();
            if (!keepRaw)
            {
                userInput = userInput.Trim();
            }
            if (minimumCharactersAllowed > 0 && userInput.Length == 0)
            {
                userInput = null;
                loggerWithColors.Warn($"Entered input was blank, input not allowed to be empty, please try again.");
            }
            else if (userInput.Length < minimumCharactersAllowed)
            {
                userInput = null;
                loggerWithColors.Warn($"Entered input was too short, it must be at least {minimumCharactersAllowed} characters long, please try again.");
            }
        } while (userInput == null);

        return userInput;
    }

    // public static bool UserCreatedContinueObtainer(string message){
    //     return UserCreatedBooleanObtainer;
    // }

    public static bool UserCreatedBooleanObtainer(string message, bool defaultValue){
        string lowercaseOptionTrue = "y";
        string lowercaseOptionFalse = "n";

        ConsoleColor startingBackground = Console.BackgroundColor;

        // Console.Write($"\n{message}? ({(defaultValue?$"{lowercaseOptionTrue.ToUpper()}":$"{lowercaseOptionTrue}")}/{(defaultValue?$"{lowercaseOptionFalse}":$"{lowercaseOptionFalse.ToUpper()}")}): ");
        Console.Write($"\n{message}? (");
        if(defaultValue){
            Console.BackgroundColor = selectDefaultOptionColorBackground;
            Console.ForegroundColor = selectDefaultOptionColor;
            Console.Write($"{lowercaseOptionTrue.ToUpper()}");
            Console.ForegroundColor = defaultColor;
            Console.BackgroundColor = startingBackground;
        }else{
            Console.Write($"{lowercaseOptionTrue}");
        }
        Console.Write($"/");
        if(!defaultValue){
            Console.BackgroundColor = selectDefaultOptionColorBackground;
            Console.ForegroundColor = selectDefaultOptionColor;
            Console.Write($"{lowercaseOptionFalse.ToUpper()}");
            Console.ForegroundColor = defaultColor;
            Console.BackgroundColor = startingBackground;
        }else{
            Console.Write($"{lowercaseOptionFalse}");
        }
        Console.Write("): ");
        string userInput = getInputWithColor().Trim().ToLower();
        return userInput.Equals(lowercaseOptionTrue) || (userInput.Length==0?defaultValue:false);
    }


    public static int UserCreatedIntObtainer(string message, int minValue, int maxValue, bool showRange)
    {
        return userCreatedIntObtainer(message, minValue, maxValue, showRange, "");
    }
    public static int UserCreatedIntObtainer(string message, int minValue, int maxValue, bool showRange, int defaultValue)
    {
        return userCreatedIntObtainer(message, minValue, maxValue, showRange, defaultValue.ToString());
    }
    private static int userCreatedIntObtainer(string message, int minValue, int maxValue, bool showRange, string defaultValue)
    {
        string userInputRaw;
        int userChosenInteger;
        int defaultAsInt = 0;

        if (defaultValue != "" && !int.TryParse(defaultValue, out defaultAsInt))
        {
            loggerWithColors.Error($"Could not use default value of \"{defaultValue}\" as an int. Argument exception error!");
            defaultValue = "";
        }
        do
        {
            Console.Write($"\n{message}{(showRange ? $" ({minValue} to {maxValue})" : "")}{(defaultValue == "" ? "" : $" or leave blank to use \"{defaultValue}\"")}: ");
            Console.ForegroundColor = userColor;
            userInputRaw = Console.ReadLine().Trim();
            Console.ForegroundColor = defaultColor;
            if (int.TryParse(userInputRaw, out userChosenInteger) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
            {
                if (defaultValue != null && userInputRaw.Length == 0) //Was blank and allowed
                {
                    userChosenInteger = defaultAsInt;
                }
                else if (defaultValue == null && userInputRaw.Length == 0)
                {
                    loggerWithColors.Error("Your chosen integer was empty, please try again.");
                    userInputRaw = null; //Was blank and not allowed
                }
                else if (userChosenInteger < minValue)
                {
                    loggerWithColors.Error($"Your chosen integer choice was below \"{minValue}\", the range is ({minValue} to {maxValue}), please try again.");
                    userInputRaw = null; //Under min
                }
                else if (userChosenInteger > maxValue)
                {
                    loggerWithColors.Error($"Your chosen integer choice was above \"{maxValue}\", the range is ({minValue} to {maxValue}), please try again.");
                    userInputRaw = null; //Above max
                }
                else
                {
                }
            }
            else
            {
                //User response was not a integer
                loggerWithColors.Error("Your chosen id choice was not a possible integer, please try again.");
                userInputRaw = null; //Was not an integer
            }
        } while (userInputRaw == null);
        return userChosenInteger;
    }

    public static double UserCreatedDoubleObtainer(string message, double minValue, double maxValue, bool showRange)
    {
        return userCreatedDoubleObtainer(message, minValue, maxValue, showRange, "", -1);
    }
    public static double UserCreatedDoubleObtainer(string message, double minValue, double maxValue, bool showRange, int places)
    {
        return userCreatedDoubleObtainer(message, minValue, maxValue, showRange, "", places);
    }
    public static double UserCreatedDoubleObtainer(string message, double minValue, double maxValue, bool showRange, double defaultValue, int places)
    {
        return userCreatedDoubleObtainer(message, minValue, maxValue, showRange, defaultValue.ToString(), places);
    }

    public static double userCreatedDoubleObtainer(string message, double minValue, double maxValue, bool showRange, string defaultValue, int places)
    {//Use -1 for no places
        string userInputRaw = null;
        double userChosenDouble;
        double defaultAsDouble = 0;

        if (defaultValue != "" && !double.TryParse(defaultValue, out defaultAsDouble))
        {
            loggerWithColors.Error($"Could not use default value of \"{defaultValue}\" as a double. Argument exception error!");
            defaultValue = "";
        }
        do
        {
            Console.Write($"\n{message}{(showRange ? $" ({minValue} to {maxValue})" : "")}{(defaultValue == "" ? "" : $" or leave blank to use \"{defaultValue}\"")}: ");
            Console.ForegroundColor = userColor;
            userInputRaw = Console.ReadLine().Trim();
            Console.ForegroundColor = defaultColor;

            if (double.TryParse(userInputRaw, out userChosenDouble) || userInputRaw.Length == 0) //Duplicate .Length == 0 checking to have code in the same location
            {
                if (defaultValue != null && userInputRaw.Length == 0) //Was blank and allowed
                {
                    userChosenDouble = defaultAsDouble;
                }
                else if (defaultValue == null && userInputRaw.Length == 0)
                {
                    loggerWithColors.Error("Your chosen number was empty, please try again.");
                    userInputRaw = null; //Was blank and not allowed
                }
                else if (userChosenDouble < minValue)
                {
                    string formattedMinValue, formattedMaxValue;
                    if (places == -1)
                    {
                        formattedMinValue = minValue.ToString($"N{places}");
                        formattedMaxValue = maxValue.ToString($"N{places}");
                    }
                    else
                    {
                        formattedMinValue = minValue.ToString();
                        formattedMaxValue = maxValue.ToString();
                    }

                    loggerWithColors.Error($"Your chosen number choice was below \"{minValue}\", the range is ({formattedMinValue} to {formattedMaxValue}), please try again.");
                    userInputRaw = null; //Under min
                }
                else if (userChosenDouble > maxValue)
                {
                    loggerWithColors.Error($"Your chosen number choice was above \"{maxValue}\", the range is ({minValue} to {maxValue}), please try again.");
                    userInputRaw = null; //Above max
                }
                else if (places != -1) //No problems so far, check for placeValues
                {
                    int decimalPointIndex = userInputRaw.ToString().LastIndexOf("."); //Uses last to work backwards
                    //Decimal point was found and had too many
                    if (decimalPointIndex != -1 && userInputRaw.Length - 1 - decimalPointIndex > places)
                    {
                        loggerWithColors.Error($"Your chosen number had too many decimal place values, can only have max of {places} decimal places, please try again.");
                        userInputRaw = null;
                    }
                }
            }
            else
            {
                //User response was not a double
                loggerWithColors.Error("Your chosen id choice was not a possible number, please try again.");
                userInputRaw = null; //Was not an integer
            }
        } while (userInputRaw == null);
        return userChosenDouble;
    }

    public static string[] RepeatingOptionsSelector(string[] optionsToPickFrom)
    {
        string stopOption = "Done picking";
        string[] optionsToPickFromWithStop = new string[optionsToPickFrom.Length + 1];
        for (int i = 0; i < optionsToPickFrom.Length; i++)
        {
            optionsToPickFromWithStop[i] = optionsToPickFrom[i];
        }
        optionsToPickFromWithStop[optionsToPickFromWithStop.Length - 1] = stopOption;

        List<string> selectedOptions = new List<string>() { };

        string optionSelectedStr = "";

        do
        {
            optionSelectedStr = OptionsSelector(optionsToPickFromWithStop);
            for (int i = 0; i < optionsToPickFromWithStop.Length; i++)
            {
                if (optionSelectedStr == optionsToPickFromWithStop[i])
                {
                    if (optionSelectedStr == stopOption)//optionsToPickFromWithStop[optionsToPickFromWithStop.Length - 1])
                    { //Last item was added just above as not an add option, but to stop
                        optionSelectedStr = null; //Inform that do-while is over
                    }
                    else
                    {
                        selectedOptions.Add(optionSelectedStr);
                    }
                    optionsToPickFromWithStop[i] = null; //Blank options are removed from options selector
                    break;
                }
            }
        } while (optionSelectedStr != null); //Last index is done option

        return selectedOptions.ToArray();
    }



    private static string getInputWithColor(){
        Console.ForegroundColor = userColor;
        string enteredValue = Console.ReadLine().ToString();
        Console.ForegroundColor = defaultColor;   
        return enteredValue;
    }

    public static void messageAlert(string message)
    {
        ConsoleColor existingColor = Console.ForegroundColor;
        Console.ForegroundColor = errorColor;
        // loggerWithColors.Log();

        Console.ForegroundColor = existingColor;
    }



}