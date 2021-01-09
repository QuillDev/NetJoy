using System;
using NetJoy.Core.Utils.General;

namespace NetJoy.Core.Utils
{
    public static class Prompts
    {
        public static bool YesNoPrompt(string question)
        {
            do
            {
                //Ask the question
                Logger.Log($"{question} (y/n)");
                
                //read the next line from the console
                var input = Console.ReadLine();
                
                //if the input is null, skip
                if (input == null)
                {
                    Logger.Clear();
                    Logger.LogError("Invalid Input Entered, Please try again!");
                    continue;
                }
                
                //set input to lower case
                input = input.ToLowerInvariant();
                
                //if they entered yes, return true
                if (input.Equals("y"))
                {
                    return true;
                }
                
                //if they entered no, return false
                if(input.Equals("n"))
                {
                    return false;
                }
            } while (true);
        }
    }
}