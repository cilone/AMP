using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AnyListen.AppCommunication.Commands;
using AnyListen.ViewModels;

namespace AnyListen.AppCommunication
{
    public class Commander
    {
        private static List<CommandBase> _commands;
 
        public static void ExecuteCommand(string line, StreamProvider streams)
        {
            if (string.IsNullOrEmpty(line)) return;
            if (_commands == null)
            {
                _commands = new List<CommandBase> {new FileTransferCommand(), new PlaylistCommand()};
            }

            var command = _commands.First(x => Regex.IsMatch(line, x.RegexPattern));
            if (command != null) command.Execute(line, streams, MainViewModel.Instance.MusicManager);
        }
    }
}
