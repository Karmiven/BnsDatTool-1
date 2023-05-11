using System;
using System.Collections.Generic;
using System.Xml;

namespace Xylia.bns.Util.Sort
{
	/// <summary>
	/// Xml属性排序
	/// </summary>
	public class XmlAttributeSort : IComparer<XmlAttribute>
	{
		/// <summary>
		/// 是否是序列化配置文件
		/// </summary>
		public bool IsConfig = false;

		public int Compare(XmlAttribute x, XmlAttribute y)
		{
			if (x is null || y is null) throw new ArgumentException("无效参数");

			string AliasX = NameEx.NameConvert(x.Name, IsConfig);
			string AliasY = NameEx.NameConvert(y.Name, IsConfig);
			return Xylia.Sort.Method.StrCompare(AliasX, AliasY);
		}
	}
}
