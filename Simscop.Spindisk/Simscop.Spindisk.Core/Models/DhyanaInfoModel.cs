using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Simscop.Spindisk.Core.Models;

/// <summary>
/// 
/// </summary>
public class DhyanaInfoModel
{
    public string Name { get; set; } = "NULL";

    public string Bus { get; set; } = "USB ?";

    public string ApiVersion { get; set; } = "-1";

    public string FirmwareVersion { get; set; } = "-1";

    public string FpgaVersion { get; set; } = "-1";

    public string DriveVersion { get; set; } = "-1";
}