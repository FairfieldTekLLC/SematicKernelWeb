﻿using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SematicKernelWeb.SemanticKernel.Extensions;

public class TimeInformationPlugin
{
    [KernelFunction("get-time")]
    [Description("returns the current time.")]
    public async Task<string> GetTime()
    {
        return DateTime.Now.ToString("HH:mm:ss zz");
    }


    [KernelFunction("get-date")]
    [Description("returns the current date.")]
    public async Task<string> GetDate()
    {
        return DateTime.Now.ToString("HH:mm:ss zz");
    }
}