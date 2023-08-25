using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;

namespace FlavorsOfMemoryLeak.Api.Controllers;

public class MyLeakSource
{
    static MyLeakSource inst;
    public List<byte[]> List { get; set; }
    public static MyLeakSource GetMySingleton()
    {
        if (inst == null)
        {
            inst = new MyLeakSource();
            inst.List = new List<byte[]>(5000);
        }
        return inst;
    }
}

[ApiController]
[Route("[controller]")]
public class LeakMemoryController : ControllerBase
{
    public const int MEGABYTE = 1048576;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="managedMemory"></param>
    /// <param name="BigBlock"></param>
    /// <param name="amountMB"></param>
    /// <returns></returns>
    [HttpGet("/leak")]
    public MemoryReport Leak(string managedOrUnamanagedMemory, string BigOrSmallBlock, int amountMB)
    {
        var rdn = Random.Shared;
        int buffSize;
        if (BigOrSmallBlock.ToLower() == "big")
        {
            buffSize = MEGABYTE / 2;
        }
        else
        {
            //10kb
            buffSize = 10240;
        }
        var iterations = (amountMB * MEGABYTE / buffSize) + 1;

        if (managedOrUnamanagedMemory.ToLower() == "unmanaged")
        {
            for (int c = 0; c < iterations; c++)
            {
                var ptr = Marshal.AllocHGlobal(buffSize);
                try
                {
                    Span<byte> span;
                    unsafe
                    {
                        span = new Span<byte>(ptr.ToPointer(), buffSize);
                    }
                    for (int i = 0; i < span.Length; i++)
                    {
                        span[i] = (byte)rdn.Next(0, 255);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
        else
        {
            for (int c = 0; c < iterations; c++)
            {
                byte[] buff = new byte[buffSize];
                for (int i = 0; i < buff.Length; i++)
                {
                    buff[i] = (byte)rdn.Next(0, 255);
                }
                MyLeakSource.GetMySingleton().List.Add(buff);
            }
        }

        return ReportMemory();
    }

    [HttpGet("/reportMemory")]
    public MemoryReport ReportMemory()
    {
        var process = Process.GetCurrentProcess();
        process.Refresh();
        MemoryReport report = new MemoryReport();
        report.ProcessId = process.Id;
        report.NonpagedSystemMemorySize64 = process.NonpagedSystemMemorySize64;
        report.PagedSystemMemorySize64 = process.PagedSystemMemorySize64;
        report.PagedMemorySize64 = process.PagedMemorySize64;
        report.PrivateMemorySize64 = process.PrivateMemorySize64;
        report.VirtualMemorySize64 = process.VirtualMemorySize64 / MEGABYTE;
        report.WorkingSet64 = process.WorkingSet64 / MEGABYTE;
        var info = GC.GetGCMemoryInfo(GCKind.Any);

        for (int i = 0; i < info.GenerationInfo.Length; i++)
        {
            report.GCGens.Add((i, info.GenerationInfo[0].SizeBeforeBytes));
        }
        report.Heap = info.HeapSizeBytes / (double)MEGABYTE;
        process.Dispose();

        return report;
    }
}

public class MemoryReport
{
    public long NonpagedSystemMemorySize64 { get; set; }
    public long PagedSystemMemorySize64 { get; internal set; }
    public long PagedMemorySize64 { get; internal set; }
    public long PrivateMemorySize64 { get; internal set; }
    public long VirtualMemorySize64 { get; internal set; }
    public long WorkingSet64 { get; internal set; }
    public int ProcessId { get; internal set; }
    public List<(int gen, long value)> GCGens { get; set; } = new List<(int gen, long value)>();
    public double Heap { get; internal set; }
}

