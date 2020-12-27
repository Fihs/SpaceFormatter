using SpaceFormatter.Core;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceFormatter.Console
{
    public class Program
    {
        #region Fields

        private static (int Left, int Top) curPos;

        #endregion Fields

        #region Methods

        private static int Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand("The SpaceFormatter utility is used to overwrite \"free\" disk space with random data.")
            {
                new Option<string>(new string[] { "-p", "--path" }, () => AppContext.BaseDirectory, "The path where temporary files will be created."),
                new Option<bool>(new string[] { "-c", "--clear" }, () => true, "Delete temporary data after formatting."),
                new Option<bool>(new string[] { "-r", "--random" }, () => false, "All files will use different data."),
                new Option<ulong>(new string[] { "-s", "--size" }, () => 10485760L, "The size of the temporary file in bytes."),
            };
            rootCommand.Handler = CommandHandler.Create<string, bool, bool, ulong>(Format);

            //Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }

        private static async Task Format(string path, bool clear, bool random, ulong size)
        {
            Formatter formatter = new Formatter();

            FormatterParameters parameters = new FormatterParameters(path, clear, random, size);
            IProgress<FormatterProgress> progress = new Progress<FormatterProgress>((e) => PrintProgress(e));

            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    cancellationTokenSource.Cancel();
                    e.Cancel = true;
                };

                System.Console.CursorVisible = false;
                curPos = System.Console.GetCursorPosition();
                await formatter.Format(parameters, progress, cancellationTokenSource.Token).ConfigureAwait(false);
                System.Console.CursorVisible = true;
            }
        }

        private static void PrintProgress(FormatterProgress e)
        {
            System.Console.SetCursorPosition(curPos.Left, curPos.Top);
            System.Console.WriteLine($"{e.Current}\\{e.Count}");
            System.Console.WriteLine(new string('█', (int)(e.Percentage * System.Console.WindowWidth)));
            System.Console.WriteLine($"Estimate time: {e.Etc}");
        }

        #endregion Methods
    }
}