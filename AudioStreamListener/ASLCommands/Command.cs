using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASLCommands
{
    public class Command
    {
        public CommandType CommandType { get; set; }
        public string Value { get; set; }
    }

    public enum CommandType
    {
        Status, 
        Color, 
        Port
    }

    public enum AslColor
    { 
        Red,
        Green,
        Blue
    }

    public enum AslStatus
    {
        Enabled,
        Disabled,
        Spoofed
    }
}
