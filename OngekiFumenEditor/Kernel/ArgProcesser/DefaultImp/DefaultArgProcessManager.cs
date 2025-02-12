﻿using OngekiFumenEditor.Utils;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OngekiFumenEditor.Kernel.ArgProcesser.DefaultImp
{
	[Export(typeof(IProgramArgProcessManager))]
	internal class DefaultArgProcessManager : IProgramArgProcessManager
	{
		public async Task ProcessArgs(string[] args)
		{
			void ErrorExit(string message)
			{
				MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Stop);
				Application.Current.Shutdown(-1);
			}

			if (args.LastOrDefault() is string filePath)
			{
				if (File.Exists(filePath))
				{
					Log.LogInfo($"arg.filePath: {filePath}");

					if (!await DocumentOpenHelper.TryOpenAsDocument(filePath))
						ErrorExit("提供的文件编辑器无法打开处理");
				}
				else
					ErrorExit("通过参数提供的文件不存在");
			}
		}
	}
}
