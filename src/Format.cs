/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
using System;
using SwfDotnet.Format.BasicTypes;
using SwfDotnet.Format.UtilTypes;
using SwfDotnet.Format.Tags;

namespace SwfDotnet.Format
{
	/// <summary>
	/// Any Character to place in a Movie Frame
	/// </summary>
	public class Character: BaseTag
	{
		public Character(int tagcode):base(tagcode){}
		public int CharacterID;
		public int Depth;
	}
	
	/// <summary>
	/// Any Font definition
	/// </summary>
	public class SwfFont: BaseTag
	{
		public SwfFont(int tagcode):base(tagcode){}
		public int FontID;
	}
	
	
	public interface IShapeData
	{
		 RECT Bounds {get;}
		 int NumFillBits{get;}
		 int NumLineBits{get;}
	}
	
	
	public interface IDepthChanger{
		int SetDepth(int depth);
	
	}
	
		
		
}