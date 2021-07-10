using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Lanchat.Core.Extensions;
using Lanchat.Terminal.Properties;

namespace Lanchat.Terminal.Commands
{
    public class CommandsController
    {
        private readonly List<ICommand> commands = new();

        public CommandsController()
        {
            var ass = System.Reflection.Assembly.GetEntryAssembly();
            ass?.DefinedTypes.ForEach(x =>
            {
                if (x.ImplementedInterfaces.Contains(typeof(ICommand)))
                {
                    commands.Add(ass.CreateInstance(x.FullName!) as ICommand);
                } 
            });
        }

        public void ExecuteCommand(string[] args)
        {
            var commandAlias = args[0][1..];
            args = args.Skip(1).ToArray();
            var command = commands.FirstOrDefault(x => x.Alias == commandAlias);

            if (command == null)
            {
                Program.Window.TabsManager.WriteText(Resources._InvalidCommand);
                return;
            }

            if (args.Length < command.ArgsCount)
            {
                var help = Resources.ResourceManager.GetString($"Help_{commandAlias}", CultureInfo.CurrentCulture);
                if (help != null)
                {
                    Program.Window.TabsManager.WriteText(help);
                }

                return;
            }

            command.Execute(args);
        }
    }
}