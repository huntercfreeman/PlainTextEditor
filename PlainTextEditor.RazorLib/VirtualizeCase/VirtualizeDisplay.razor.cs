using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace PlainTextEditor.RazorLib.VirtualizeCase;

public partial class VirtualizeDisplay : ComponentBase
{
    [Parameter]
    public int MyProperty { get; set; }
}