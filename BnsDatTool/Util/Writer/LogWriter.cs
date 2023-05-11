using System;
using System.Diagnostics;
using System.IO;

public static class LogWriter
{
	public static string RecordPath = Xylia.Configure.PathDefine.MainFolder + $@"\log\{ DateTime.Now:yyMMdd}.rec";


	/// <summary>
	/// 创建日志
	/// </summary>
	/// <param name="Txt"></param>
	public static void CreateLog(this object Txt)
	{
		if (Txt is null || Txt.ToString() is null) return;


		try
		{
			File.AppendAllText(RecordPath, Txt.ToString() + '\n');
		}
		catch
		{

		}
	}


	/// <summary>
	/// 日志输出级别（0:不输出  1:只输出必要内容  2:完全输出)
	/// </summary>
	public static int LogLevel = 2;


	/// <summary>
	/// 写日志
	/// </summary>
	/// <param name="Txt"></param>
	/// <param name="Level">日志级别</param>
	/// <param name="HasTimeInfo"></param>
	public static void WriteLine(object Txt, int Level = 1, bool HasTimeInfo = true)
	{
		//如果日志定义级别大于等于当前消息级别，输出日志
		if (LogLevel < Level) return;

		Txt = (HasTimeInfo ? $"[{DateTime.Now}] " : null) + Txt;

		if (!DebugSwitch.HideDebugInfo)
			Trace.WriteLine(Txt);

		Txt.CreateLog();
	}
}