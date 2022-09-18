using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poltergeist.Automations.Macros;

namespace Poltergeist.Models;

public class MacroGroup
{
    public string Key { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<MacroBase> Macros { get; set; }
}
