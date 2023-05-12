using System.IO;
using System.Text;

using NPOI.XSSF;

using static Xylia.bns.Modules.DataFormat.Dat.BXML_CONTENT;

namespace Xylia.bns.Modules.DataFormat.Dat
{
	/// <summary>
	/// 文件头信息
	/// </summary>
	public class BPKG_FTE
	{
		#region Fields
		public string FilePath;

		public byte Unknown_001;
		public byte Unknown_002;

		public bool IsCompressed;
		public bool IsEncrypted;


		public int FileDataOffset;        // (relative) offset
		public int FileDataSizeSheared;   // without padding for AES
		public int FileDataSizeStored;
		public int FileDataSizeUnpacked;

		public byte[] Padding;
		#endregion

		#region Constructor
		public BPKG_FTE()
		{

		}

		public BPKG_FTE(BinaryReader reader, bool is64, int OffsetGlobal = 0)
		{
			int FilePathLength = (int)(is64 ? reader.ReadInt64() : reader.ReadInt32());

			this.FilePath = Encoding.Unicode.GetString(reader.ReadBytes(FilePathLength * 2));
			this.Unknown_001 = reader.ReadByte();
			this.IsCompressed = reader.ReadByte() == 1;
			this.IsEncrypted = reader.ReadByte() == 1;
			this.Unknown_002 = reader.ReadByte();
			this.FileDataSizeUnpacked = (int)(is64 ? reader.ReadInt64() : reader.ReadInt32());
			this.FileDataSizeSheared = (int)(is64 ? reader.ReadInt64() : reader.ReadInt32());
			this.FileDataSizeStored = (int)(is64 ? reader.ReadInt64() : reader.ReadInt32());
			this.FileDataOffset = (int)(is64 ? reader.ReadInt64() : reader.ReadInt32()) + OffsetGlobal;
			this.Padding = reader.ReadBytes(60);
		}
		#endregion



		#region DATA
		public KeyInfo KeyInfo;


		public byte[] Data;


		BXML_CONTENT _xml;

		public BXML_CONTENT Xml
		{
			private set => _xml = value;
			get
			{
				if (_xml is null) this.Decrypt();

				return _xml;
			}
		}

	

		public void Decrypt()
		{
			byte[] buffer_unpacked = BNSDat.Unpack(Data, this.FileDataSizeStored, this.FileDataSizeSheared, this.FileDataSizeUnpacked, this.IsEncrypted, this.IsCompressed, KeyInfo);

			if (this.FilePath.EndsWith(".xml") || this.FilePath.EndsWith(".x16"))
			{
				Xml = new BXML_CONTENT(KeyInfo.XOR_KEY);
				Xml.Read(new MemoryStream(buffer_unpacked), BXML_TYPE.BXML_BINARY);

				var oStream = new MemoryStream();
				Xml.Write(oStream, BXML_TYPE.BXML_PLAIN);
				this.Data = oStream.ToArray();
			}
			else this.Data = buffer_unpacked;
		}
		#endregion
	}
}