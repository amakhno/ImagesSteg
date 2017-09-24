using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Windows.Input;

namespace Images2
{
    static class Compressor
    {
        private static Collection<PSObject> results;

        public static void Compress(string imagePath)
        {
            string fullPath = System.IO.Path.GetFullPath(imagePath);
            RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();

            Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
            runspace.Open();

            RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);

            Pipeline pipeline = runspace.CreatePipeline();

            //Here's how you add a new script with arguments
            Command myCommand = new Command(@".\script.bat");
            CommandParameter testParam = new CommandParameter(fullPath);
            myCommand.Parameters.Add(testParam);

            pipeline.Commands.Add(myCommand);

            // Execute PowerShell script
            results = pipeline.Invoke();
        }
    }
}
