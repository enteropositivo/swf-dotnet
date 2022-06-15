/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
using System;
using SwfDotnet.Format.BasicTypes;
using SwfDotnet.Format.UtilTypes;

namespace SwfDotnet.Format.Tags
{

	//- Tipo de TAG   [TIPO][LENGTH_DATOS_DEL_TAG]
	class RecordHeader: ArrayData
	{
		public RecordHeader(int tipo, int length)
		{
			AB _rh = new AB(2, true);
			_rh.Append(new UB(10,tipo));
			if(length>62)
			{
				_rh.Append(new Bits("111111"));
				this.Add(_rh);
				this.Add(new UI32(length));

			}
			else
			{
				_rh.Append(new UB(6,length));
				this.Add(_rh);
			}			
		}
     
	}

	//- Tag base, tipo corta y larga
	public class BaseTag : ArrayData
	{
		private RecordHeader _TagHeader ;
		private int _TagID ;
		public BaseTag(int tagid)
		{
			this._TagID=tagid;
		}
		protected override void OnCompile()
		{	
			//- Le inserto la cabecera la cabecera
			this._TagHeader = new RecordHeader(this._TagID, this.Length);
			this.Insert(0, this._TagHeader);
			base.OnCompile ();
		}
	}
}