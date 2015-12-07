using System;
using System.IO;
using CommandLine;
using CommandLine.Text;
using System.Text;

namespace VVVV.Core
{
    public class CommandLineArguments
    {
        //a file to open on startup
        [Option('o', "open", Required = false, HelpText = "File to open on startup")]
        public string InputFile { get; set; }

        [Option('e', "throwexceptions", Required = false, HelpText = "If true, vvvv will stop on exceptions")]
        public bool ThrowExceptions { get; set; }

        [Option('l', "local", Required = false, HelpText = "If true, vvvv will not try to comunicate with a runtime")]
        public bool Local { get; set; }

        //help string
        [HelpOption(HelpText = "Dispaly this help screen")]
        public string GetUsage()
        {

            var help = new HelpText("Could not parse commandline arguments, use the following options:");
            help.AdditionalNewLineAfterOption = true;
            help.AddOptions(this);

            return help;
        }

        //parse the commandline and args.txt
        public void Parse()
        {
            var parser = new Parser((parserSettings) => parserSettings.HelpWriter = Console.Out);

            if (File.Exists("Args.txt"))
            {
            	try //parse Args.txt
            	{
            		var streamReader = new StreamReader(Path.Combine(Environment.CurrentDirectory, "Args.txt"));
            		var text = streamReader.ReadToEnd();
            		var args = text.Split('\n');
            		streamReader.Close();
            		if (!parser.ParseArguments(args, this))
            		{
            			Console.WriteLine("Args.txt has syntax error(s):");
            			foreach (var arg in args)
            			{
            				Console.WriteLine(arg);
            			}
            			Console.WriteLine();
            		}
            	}
            	catch (Exception e)
            	{
            		Console.WriteLine("Could not parse Args.txt: " + e.Message);
            	}
            }

            //parse commandline
            if (!parser.ParseArguments(Environment.GetCommandLineArgs(), this))
            {
                Console.WriteLine("Console command line has syntax error(s):");
                foreach (var arg in Environment.GetCommandLineArgs())
                {
                    Console.WriteLine(arg);
                }
                Console.WriteLine();
            }
        }

    }
}
