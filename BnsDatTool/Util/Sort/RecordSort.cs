using System;
using System.Collections.Generic;
using System.Linq;

using Xylia.bns.Modules.DataFormat.Analyse.Record;

namespace Xylia.bns.Util.Sort
{
	/// <summary>
	/// 记录器顺序排序
	/// </summary>
	public class RecordSort : IComparer<RecordDef>
	{
		public int Compare(RecordDef x, RecordDef y)
		{
			if (x == null || y == null) throw new ArgumentException("无效参数");

			//根据记录器是否对客户端生效进行分类处理
			if (x.Client && y.Client)
			{
				bool EmptyX = x.Filter.Count == 0;
				bool EmptyY = y.Filter.Count == 0;

				//先根据类型排序，相同类型根据起始位置排序
				if (!EmptyX && EmptyY) return 1;
				else if (EmptyX && !EmptyY) return -1;
				else if (!EmptyX && !EmptyY)
				{
					if (x.Filter.Min() != y.Filter.Min()) return x.Filter.Min() - y.Filter.Min();
				}
					
				return x.TableIndex - y.TableIndex;
			}

			//将客户端属性放在前面
			else if (!x.Client && y.Client) return 1;
			else if (x.Client && !y.Client) return -1;
			else return 0;
		}
	}
}