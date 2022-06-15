/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
 
using System;
using SwfDotnet.Format;
using SwfDotnet.Format.BasicTypes;
using SwfDotnet.Format.Shapes;
using SwfDotnet.Format.Buttons;
using SwfDotnet.Format.ActionScript;
using SwfDotnet.Format.UtilTypes;
using System.IO;

namespace SwfDotnet.Format.Tags
{

	public class TagBgColor: BaseTag
	{
		public TagBgColor(RGB Color):base(9)
		{
			this.Add(Color);
		}
	}
	
	public class TagRemoveObject: BaseTag
	{
		public TagRemoveObject(int CharacterID, int Deph):base(5)
		{
			this.Add(new  UI16(CharacterID));
			this.Add(new  UI16(Deph));
		}
	}
	
	public class EnableDebugger2: BaseTag
	{
		public EnableDebugger2():base(64)
		{
			this.Add(new STRING("", true));
		}
	}
	
	public class TagRemoveObject2: BaseTag
	{	Character _character;
	
		protected override void OnCompile()
		{	this.Add(new  UI16(this._character.Depth));
			base.OnCompile ();
		}
		
		public TagRemoveObject2(Character Character):base(28)
		{	
			this._character = Character;
			
		}
	}

	public class TagShowFrame: BaseTag	
	{
		protected override void OnCompile()
		{
			base.OnCompile ();
		}

		public TagShowFrame():base(1)
		{}
	}
	
	public class TagEnd: BaseTag	
	{
		public TagEnd():base(0)
		{}
	}
	
	public class TagDoAction: BaseTag	
	{
		public TagDoAction(Script ActioScript):base(12)
		{
			this.Add(ActioScript);
			this.Add(new UI8(0));
		}
	}


	public class TagDefineButton: Character
	{	public Script Script= new Script();
	
		private Character _Hit,  _Down,_Over, _Up;
		
		public Character Hit
		{
			get{return this._Hit;}
			set{this._Hit=value;}
		}
		public Character Down
		{
			get{return this._Down;}
			set{this._Down=value;}
		}
		public Character Over
		{
			get{return this._Over;}
			set{this._Over=value;}
		}
		public Character Up
		{
			get{return this._Up;}
			set{this._Up=value;}
		}
		
		protected override void OnCompile()
		{
			this.Add(new  UI16(this.CharacterID));
			
			if(this._Hit!=null) this.Add(new ButtonRecord(this._Hit.CharacterID, true, false, false, false) );
			if(this._Down!=null) this.Add(new ButtonRecord(this._Down.CharacterID, false, true, false, false) );
			if(this._Over!=null) this.Add(new ButtonRecord(this._Over.CharacterID, false, false, true, false) );
			if(this._Up!=null) this.Add(new ButtonRecord(this._Up.CharacterID, false, false, false, true) );

			this.Add(new UI8(0));
			
			Script AS= new Script();
			
			AS.DeclareDictionary(new string[]{"Variable", "2", "Z"});
			
			//  P = Z
			AS.SetVar("P", AS.GetVar("Z"));
			
			// Z =                  P           +         lo
			AS.SetVar("Z", AS.Sum(AS.GetVar("P"), AS.GetVar("lo")));
			
			/*
			AS.Push("Z");
				AS.Push("Variable");
				AS.GetVar();
			AS.SetVar();
			
			AS.Push("Z");		//Z=	
				
				AS.Push("Z");   // |
				AS.GetVar();	// | Z++
				AS.Inc();		// |
			AS.SetVar();
			
			AS.Push(0);
			AS.Push(2);
			AS.GetVar();
			AS.SetVar();
	*/
			if(Script.Count>0){
				this.Add(Script);
				this.Add(new UI8(0));
			}
			base.OnCompile ();
		}

		public TagDefineButton():base(7){	}
	}

	public class TagDefineShape: Character
	{	private ShapeWithStyle Shape;
		protected override void OnCompile()
		{this.Add(new UI16(this.CharacterID));  //shapeID
			this.Add(this.Shape.Bounds);
			this.Add(this.Shape);
			base.OnCompile ();
		}

		public TagDefineShape(ShapeWithStyle shape):base(2)
		{	Shape=shape; 
		}
	}
	
	
	//- Convinacion de TagDefineShape y ShapeWithStyle para ahorrar codigo
	public class DefineShape: Character, IShapeData
	{
		protected FillStyleArray FillStyles;
		protected LineStyleArray LineStyles= new LineStyleArray();
		protected int nFillBits=0;
		protected int nLineBits=0;
		public RecordArray Records;
		protected RECT _bounds;
		private bool hasBounds=false;  // si se han calculado ya los bounds
		
		// devuelve los boundaries abarcados por todos los records
		// gracias a la implementacion de IBoundsChanger en los records
		// y al calculador de boundaries Boundarier
		public RECT Bounds
		{
			get
			{
				if(hasBounds) return this._bounds; //No recalcular bounds
				this.Compile();
				Boundarier b = new Boundarier();
				for(int x=0; x<Records.IBitables.Count; x++)
				{
					if(Records.IBitables[x] is IBoundsChanger)
					{
						IBoundsChanger ib = (IBoundsChanger)Records.IBitables[x];
						ib.UpdateBounds(b);
					}
				}
				//-Lo ajustamos dependiendo de los anchos de l�nea que hayan intervenido
				b.OpenRect((Twip)(this.LineStyles.MaxLineWidth/2.0));
				
				// tenemos los bounds del Shape, los situamos en el 0,0
				b.MoveToOrig();
				// hacemos que se reajusten todos los Records
				for(int x=0; x<Records.IBitables.Count; x++)
				{
					if(Records.IBitables[x] is IBoundsChanger)
					{
						IBoundsChanger ib = (IBoundsChanger)Records.IBitables[x];
						ib.RecordToOrig(b);
					}
				}
				this._bounds=b.Bounds;
				hasBounds=true;
				return this._bounds;
			}
		}
		
		public int NumFillBits
		{
			get{return this.nFillBits;}
		}
		public int NumLineBits
		{
			get{return this.nLineBits;}
		}
		
		protected override void OnCompile()
		{
			//--- la parte del TagdefineShape
			this.Add(new UI16(this.CharacterID));  //shapeID
			this.Add(this.Bounds);
			//-----
			
			this.Add(this.FillStyles);
			this.Add(this.LineStyles);
			AB a = new AB(1) ;
			
			if(this.FillStyles.Count>0) this.nFillBits = UB.NumBits(this.FillStyles.Count);
			if(this.LineStyles.Count>0) this.nLineBits= UB.NumBits(this.LineStyles.Count);
			
			
			a.Append(new UB(4, nFillBits));	//numFillIndexesBits
			a.Append(new UB(4, nLineBits));  //numLineIndexesbits
			this.Add(a);
			this.Records.Add(new EndShapeRecord());
			this.Add(Records);
			
						
			//this.Add(this.Shape);
			base.OnCompile ();
		}

		public DefineShape():base(2)
		{
			Records = new RecordArray(this);
			FillStyles= new FillStyleArray(this);
		}
	}
	
	public class TagPlaceObject2: BaseTag, IDepthChanger
	{	
		Matrix _matrix;
		//int _charID=-1;			//default -1 (no charID)
		Character _character;
		int _deph=1;
		public string Name;		//caracter nombrado
		
		public CXFORM ColorTransform;
		public int Ratio;   // de 0-65535 para Morphs
		
		protected override void OnCompile()
		{		bool _hasMatrix=false;
				bool _hasName=false;
				bool _hasCharId=false;
				bool _hasColorMatrix=false;
				bool _hasRatio=false;
				
			
			if(this._matrix!=null) _hasMatrix=true;
			if(this._character!=null)  _hasCharId=true;
			if(this.Name!=null)   _hasName=true;
			if(this.ColorTransform!=null) _hasColorMatrix=true;
			
			AB a = new AB(1) ;
			a.Append(new UB(2, 0));			// Reservados
			a.Append(new UB(1, (Bit)_hasName));			// Has Name
			a.Append(new UB(1, (Bit)_hasRatio));			// Has Ratio (usado cuando es un shape de DefineMorphShape)
			a.Append(new UB(1, (Bit)_hasColorMatrix));			// Has Color Matrix
			a.Append(new UB(1, (Bit)_hasMatrix)); // Has Matrix
			a.Append(new UB(1, (Bit)_hasCharId)); // Has CharacterID
			a.Append(new UB(1, 0));
			
			this.Add(a);
			this.Add(new UI16(this._character.Depth));				// deph
			
			if(_hasCharId)	this.Add(new UI16(this._character.CharacterID));		// charID
			if(_hasMatrix)	this.Add(this._matrix);
			if(_hasColorMatrix)	this.Add(this.ColorTransform);
			if(_hasRatio) this.Add(new UI16(this.Ratio));
			if(_hasName)  this.Add(new STRING(this.Name));
			
			base.OnCompile ();
		}
		
		
		
		public TagPlaceObject2(Character character,int Deph, Matrix TransformMatrix):base(26)
		{	
			this._matrix= TransformMatrix; 
			this._character=character;
			this._deph=Deph;
			
		}
		#region Miembros de IDepthChanger

		public int SetDepth(int depth)
		{
			this._character.Depth=depth;
			return 1;
		}

		#endregion
	}
	
	public class TagDefineFont2: SwfFont
	{
		public string FontName;
		public Bit FontBold=false;
		public Bit FontItalic=false;
		protected override void OnCompile()
		{
				this.Add(new  UI16(this.FontID));
			AB a = new AB(2) ;
			
			a.Append(new UB(1, 0));			// Has Metric info
			a.Append(new UB(1, 0));			// JIS encoding
			a.Append(new UB(1, 0));			// Unicode
			a.Append(new UB(1, 0));			// ANSI encoding
			a.Append(new UB(1,0)); // Uses 32 bits offsets
			a.Append(new UB(1,1)); // siempre 1
			a.Append(new UB(1, 0)); //WideCodes
			a.Append(new UB(1, this.FontBold)); //Bold
			a.Append(new UB(8, this.FontItalic)); //Reserved
			
			this.Add(a);
			
			this.Add(new UI8(FontName.Length));  // Length of namefont
			this.Add(new STRING(FontName, false));
			this.Add(new UI16(0));   // Num of glyphs
			
			//this.Add(new UI16(0));   // FontCodeOffset
			
			/* font offset table */
			
			/* if hasLayout*/
			//this.Add(new SI16(15));	//font ascent
			//this.Add(new UI16(0));	//font descent
			//this.Add(new UI16(0));	//font leading height

			//this.Add(new UI16(0));   // Num of glyphs
			base.OnCompile ();
		}

		public TagDefineFont2( string fontName):base(48)
		{	
			this.FontName=fontName;
		}
	}
	
	public class TagDefineEditText: Character
	{	public string VarName=string.Empty;
		public string Text=string.Empty;
		public bool Selectable=false;
		public bool HasBorder=false;
		public bool ReadOnly=true;
		public bool WordWrap=false;
		public bool Multitile=false;
		public bool PasswordFiled=false;
		
		public RGB TextColor = new RGB(0,0,0);
		
		private RECT _Bounds;
		
		private int FontID;
		
		public int Height=200;
		protected override void OnCompile()
		{	
			this.Add(new  UI16(this.CharacterID)); 
			this.Add(_Bounds);
			AB a = new AB(2) ;
			
			a.Append(new UB(1, 1));			// Has DefaultText
			a.Append(new UB(1, (Bit)WordWrap));			// wordWrap
			a.Append(new UB(1, (Bit)Multitile));			// Multiline
			a.Append(new UB(1, (Bit)PasswordFiled));			// Password
			a.Append(new UB(1,(Bit)ReadOnly));  // ReadOnly
			a.Append(new UB(1, 1));  // Has color
			a.Append(new UB(1, 0)); // Has Max Length
			a.Append(new UB(1, 1)); // Has Font
			a.Append(new UB(2, 0)); // Reserved
			a.Append(new UB(1, 0));  // Has Layout, margins, ...
			a.Append(new UB(1, (Bit)(!this.Selectable))); // NoSelectable
			a.Append(new UB(1, (Bit)HasBorder)); // TexBoxBorder
			a.Append(new UB(2, 0)); // Reserved
			a.Append(new UB(1, 0)); // Use Outlines
									
			this.Add(a);
			
			
			
			//if(hasFont)
			this.Add(new UI16(FontID));
			
			this.Add(new UI16(Height)); //height
			
			RGBA color= new RGBA(this.TextColor.Red, this.TextColor.Green, this.TextColor.Blue, 0);
			
			this.Add(color);
		
			
			this.Add(new STRING(this.VarName));
			this.Add(new STRING(this.Text));
		
			base.OnCompile ();
		}

		public TagDefineEditText(SwfFont font, int height, RECT Bounds, string text):base(37)
		{	this._Bounds=Bounds;
			this.Height=height;
			this.FontID=font.FontID;
			this.Text=text;
			
		
		}
		
	}
	
	public class TagDefineBitsJPEG2:Character{
		public string ImageSrc;
		protected override void OnCompile()
		{	this.Add(new  UI16(this.CharacterID));
			FileStream stream =  File.Open(this.ImageSrc,FileMode.Open, FileAccess.Read);
			this.Add(new UI8(0xFF));
			this.Add(new UI8(0xD8));
			this.Add(new UI8(0xFF));
			this.Add(new UI8(0xD9));
			for(int f=0; f< stream.Length;f++){
				this.Add(new UI8(stream.ReadByte()));
			}
			
			base.OnCompile ();
		}
		public TagDefineBitsJPEG2(string imageSrc):base(21){
			this.ImageSrc=imageSrc;
		}
		

	
	}
}