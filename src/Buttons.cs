/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
using System;
using SwfDotnet.Format.BasicTypes;
using SwfDotnet.Format.UtilTypes;

namespace SwfDotnet.Format.Buttons
{
	class ButtonRecord: ArrayData
	{
		public ButtonRecord(int CharacterID, bool hitTest, bool Down, bool Over, bool Up )
		{
			AB a = new AB(1);
			a.Append(new UB(4,0)); // reserved
			a.Append(new UB( 1,(Bit)hitTest)); // hitTest
			a.Append(new UB(1,(Bit)Down)); // Down
			a.Append(new UB(1,(Bit)Over)); // Over
			a.Append(new UB(1,(Bit)Up)); // UP
			this.Add(a);
			
			this.Add(new UI16(CharacterID)); //CharactetID
			this.Add(new UI16(1)); //Deph
			
			Matrix mx = new Matrix();
			mx.Tanslate(200,200);
			this.Add(mx);
			
		}
	}

}