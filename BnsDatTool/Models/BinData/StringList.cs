using System;
using System.Collections.Generic;
using System.Text;

using BnsBinTool.Core.Models;

namespace Xylia.Preview.Data.Helper
{
	public sealed class StringList : List<string>
	{
		#region Constructor
		private readonly HashSet<string> ht = new(StringComparer.OrdinalIgnoreCase);

		public StringList(StringLookup stringLookup)
		{
			int start = 0, end = 0;
			var size = stringLookup.Data.Length;
			while (end < size)
			{
				if (stringLookup.Data[end] == 0 && stringLookup.Data[end + 1] == 0)
				{
					//复制字节集为新数据
					byte[] tmp = new byte[end - start];
					Array.Copy(stringLookup.Data, start, tmp, 0, end - start);

					//转换为文本
					string w = Encoding.Unicode.GetString(tmp);
					this.Add(w);

					start = end + 2;
				}

				end += 2;
			}
		}
		#endregion

		#region 公共接口Functions
		public new void Add(string item)
		{
			if (!this.ht.Contains(item))
				this.ht.Add(item);

			base.Add(item);
		}

		public new bool Remove(string item)
		{
			this.ht.Remove(item);
			return base.Remove(item);
		}

		public new void Clear()
		{
			base.Clear();
			this.ht.Clear();
		}

		public new bool Contains(string String) => this.ht.Contains(String);
		#endregion
	}
}