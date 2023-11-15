using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VSXFilterRepair
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //[STAThread]
        enum CommandOptions { FileRepair = 1, CheckValidity = 2, NumberOfOptions };
        static void Main(string[] args)
        {
            string fileName = "";
            CommandOptions selected_option = CommandOptions.FileRepair;

            if (args.Length >= 1)
            {
                fileName = args[0];
            }
            else
            {
                Console.WriteLine("****************Local Params Mode******************");
                CheckOption: Console.WriteLine("Select option:\n\t1. Repair VSX Filter File.\n\t2. Check the file validity.");
                try
                {
                    int input_number = Convert.ToInt16(Console.ReadLine());
                    if (input_number < (int)CommandOptions.NumberOfOptions)
                        selected_option = (CommandOptions)input_number;
                    else
                    {
                        Console.WriteLine("Input is not valid. Try again.\n\n");
                        goto CheckOption;
                    }

                }
                catch
                {
                    Console.WriteLine("Input is not valid. Try again.\n\n");
                    goto CheckOption;
                }
                Console.Write("VSX filter file name: ");
                fileName = Console.ReadLine().Replace("\"", "");
            }

            VSXFilterRepair repair = new VSXFilterRepair();
            switch (selected_option)
            {
                case CommandOptions.FileRepair:
                    {
                        if (fileName != "")
                        {
                            bool result = repair.DoRepair(fileName);
                            if (result)
                                Console.WriteLine("Process has been done successfully.");
                            else
                                Console.WriteLine("Process failed!");
                        }
                        else
                        {
                            Console.WriteLine("File name is not valid!");
                        }
                        break;
                    }
                case CommandOptions.CheckValidity:
                    {
                        repair.CheckFileValidity(fileName);
                        break;
                    }
            }

            Console.Write("Please press a key to exit.");
            Console.ReadKey();
        }
    }
}
