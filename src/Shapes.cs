/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
using System;
using SwfDotnet.Format;
using SwfDotnet.Format.BasicTypes;
using SwfDotnet.Format.UtilTypes;

namespace SwfDotnet.Format.Shapes
{
	
	public class RecordArray: AB{
		IShapeData Parent;
		public RecordArray(IShapeData parent):base(-1){
			Parent=parent;
		}
		
		public void Add(object record)
		{	
			if(record is StyleChangeRecord){
				((StyleChangeRecord)record).Parent=this.Parent;
			}
			this.IBitables.Add((IBitable)record);
		}
		public void Insert(int Index, object record){
			if(record is StyleChangeRecord)
			{
				((StyleChangeRecord)record).Parent=this.Parent;
			}
			this.IBitables.Insert(Index,(IBitable)record);
		}
	}


	/// <summary>
	/// Defines a base shape with LineStyles and FillStyles
	/// </summary>
	public class ShapeWithStyle: ArrayData, IShapeData
	{	
		protected FillStyleArray FillStyles;
		protected LineStyleArray LineStyles= new LineStyleArray();
		public int nFillBits=0;
		public int nLineBits=0;
		public RecordArray Records;
		protected RECT _bounds;
		private bool hasBounds=false;  // si se han calculado ya los bounds
		
		// devuelve los boundaries abarcados por todos los records
		// gracias a la implementacion de IBoundsChanger en los records
		// y al calculador de boundaries Boundarier
		public RECT Bounds{
			get{
				if(hasBounds) return this._bounds; //No recalcular bounds
				this.Compile();
				Boundarier b = new Boundarier();
				for(int x=0; x<Records.IBitables.Count; x++){
					if(Records.IBitables[x] is IBoundsChanger){
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
		
		public int NumFillBits{
			get{return this.nFillBits;}
		}
		public int NumLineBits
		{
			get{return this.nLineBits;}
		}
		public ShapeWithStyle(){
			Records = new RecordArray(this);
			FillStyles= new FillStyleArray(this);
		}
		protected override void OnCompile()
		{
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
		}
	}
	

	
	
	
	//-End of Records
	public class EndShapeRecord: AB
	{
		public EndShapeRecord():base(1)
		{	this.Append(new UB(1, 0)); 
			this.Append(new UB(5, 0));
		}
	}
	
	//- Move/Style Changed record
	public class StyleChangeRecord: AB, IBoundsChanger
	{   
		public IShapeData Parent;
		int LineIndex=0;
		int FillIndex0=0;
		int FillIndex1=0;
		bool MoveTo=false;
		int MoveX=0, MoveY=0;
		
		protected override void OnCompile()
		{
			int LineStyle=0, FillStyle0=0, FillStyle1=0;
			
			if(LineIndex>0) LineStyle=1;
			if(FillIndex0>0) FillStyle0=1;
			if(FillIndex1>0) FillStyle1=1;
						
			this.Append(new UB(1, 0));  //-No edge -siempre0-
			this.Append(new UB(1, 0));
			this.Append(new UB(1, LineStyle)); //LineStyleChange
			this.Append(new UB(1, FillStyle0)); //FillStyleChange 0
			this.Append(new UB(1, FillStyle1)); //FillStyleChange 1
			this.Append(new UB(1,(Bit)MoveTo)); // MoveTo
			if(MoveTo)
			{
				BitCounter.Init();
				BitCounter.Push(SB.NumBits(MoveX));
				BitCounter.Push(SB.NumBits(MoveY));
				int maxB = BitCounter.Maxim;
			
				this.Append(new UB(5,maxB));	 //nMoveToBits
				this.Append(new SB(maxB,MoveX)); //MoveDeltaX
				this.Append(new SB(maxB,MoveY));  //MoveDeltaY
			}
			if(FillStyle0==1) this.Append(new UB(this.Parent.NumFillBits,FillIndex0)); 	
			if(FillStyle1==1) this.Append(new UB(this.Parent.NumFillBits,FillIndex1)); 	
			if(LineStyle==1) this.Append(new UB(this.Parent.NumLineBits,LineIndex)); 	
			
		}
		public StyleChangeRecord(int lineIndex, int fillIndex0, int fillIndex1):base(-1){
			this.LineIndex=lineIndex; this.FillIndex0=fillIndex0; this.FillIndex1=fillIndex1;
		}
		public StyleChangeRecord(int lineIndex):base(-1)
		{
			this.LineIndex=lineIndex; 
		}
		public StyleChangeRecord(int MoveToX, int MoveToY):base(-1){
			this.MoveX=MoveToX;
			this.MoveY=MoveToY;
			this.MoveTo=true;
		}
		public StyleChangeRecord(int MoveToX, int MoveToY,int lineIndex, int fillIndex0, int fillIndex1):base(-1)
		{
			this.LineIndex=lineIndex; this.FillIndex0=fillIndex0; this.FillIndex1=fillIndex1;
			this.MoveX=MoveToX;
			this.MoveY=MoveToY;
			this.MoveTo=true;
		}
		
		#region Miembros de IBoundsChanger

		public void UpdateBounds(Boundarier bounds)
		{
			if(this.MoveTo){
				bounds.NewPos(this.MoveX, this.MoveY);
			}
		}
		public void RecordToOrig(Boundarier bounds){
				this.MoveX-=bounds.difX;
				this.MoveY-=bounds.difY;
				if(!this.MoveTo)this.MoveTo=(bounds.difX!=0 || bounds.difY!=0);
		}

		#endregion
	}

	
	//-curved
	public class CurvedRecord: AB, IBoundsChanger
	{
		public int posX, posY;
		public int anchX, anchY;
		protected override void OnCompile()
		{
			this.Append(new UB(1, 1)); //edge seimpre1
			this.Append(new UB(1, 0)); //curved -siempre0-
			
			BitCounter.Init();
			BitCounter.Push(SB.NumBits(posX));
			BitCounter.Push(SB.NumBits(posY));
			BitCounter.Push(SB.NumBits(anchX));
			BitCounter.Push(SB.NumBits(anchY));
			int nBits = BitCounter.Maxim;
			this.Append(new UB(4, nBits-2)); 
			
			this.Append(new SB(nBits, posX)); 
			this.Append(new SB(nBits, posY)); 
			this.Append(new SB(nBits, anchX)); 
			this.Append(new SB(nBits, anchY)); 
		}

		public CurvedRecord(int cx, int cy, int ax, int ay):base(-1)
		{
			this.posX=cx; this.posY=cy; this.anchX=ax; this.anchY=ay;
		}
		public CurvedRecord(double cx, double cy, double ax, double ay):base(-1)
		{
			this.posX=(int)cx; this.posY=(int)cy; this.anchX=(int)ax; this.anchY=(int)ay;
		}
		#region Miembros de IBoundsChanger

		public void UpdateBounds(Boundarier bounds)
		{
			bounds.UpdatePos(this.posX, this.posY);
			bounds.UpdatePos(this.anchX, this.anchY);
		
		}
		public void RecordToOrig(Boundarier bounds){
			
		}

		#endregion
	}
	//-recta
	public class StraightRecord: AB, IBoundsChanger
	{
		bool isVert=false; // por defecto es horizontal
		int pos=0; // Si General=0 ser� la pos de vertical o horizontal
		bool General=false; // linea general (de cualquier tipo)
		int posX=0, posY=0; // si General=1 estas son las coordenadas		

		public StraightRecord(bool vertical, int delta):base(-1)
		{   
			isVert=vertical;
			pos=delta;
		}
		public StraightRecord(int incX, int incY):base(-1)
		{   
			this.General=true;
			this.posX=incX;
			this.posY=incY;
		}
		protected override void OnCompile()
		{
			int nBits;
			
			this.Append(new UB(1, 1)); //edge seimpre1
			this.Append(new UB(1, 1)); //recta -siempre1-
			
			if(General)
			{
				BitCounter.Init();
				BitCounter.Push(SB.NumBits(posX));
				BitCounter.Push(SB.NumBits(posY));
				nBits=BitCounter.Maxim;
			}
			else
			{
				nBits=SB.NumBits(pos);
			}
			
			this.Append(new UB(4, nBits-2)); //--para los delta sumar 11+2
			this.Append(new UB(1, (Bit)General)); // general line
			if(General)
			{
				this.Append(new SB(nBits, posX)); 
				this.Append(new SB(nBits, posY)); 
			}
			else
			{
				this.Append(new UB(1, (Bit)isVert)); // vertical line
				this.Append(new SB(nBits, pos)); //delta[X,Y]
			}
		}
		#region Miembros de IBoundsChanger

		public void UpdateBounds(Boundarier bounds)
		{	
			if(General)
			{
				bounds.UpdatePos(this.posX, this.posY);
			}
			else
			{
				if(this.isVert)
				{
					bounds.UpdatePos(0, this.pos);
				}
				else
				{
					bounds.UpdatePos(this.pos, 0);
				}
			}
		}
		
		public void RecordToOrig(Boundarier bounds){	}
		
		#endregion
	}
	
	/// <summary>
	/// Defines new line style 
	/// </summary>
	public class LineStyle: ArrayData
	{
		public int Width=0;
		public RGB Color;
		
		protected override void OnCompile(){
			this.Add(new UI16((Twip)Width));
			this.Add(Color);
		}
		public LineStyle(RGB color, int width)
		{	this.Width=width;
			this.Color=color;
			
		}
	}
	
	/// <summary>
	/// Array of LineStyles
	/// </summary>
	public class LineStyleArray: ArrayData
	{	private int _MaxLineWidth=0;
		public override int Add(object item){
			if(item is LineStyle){
				int lw =((LineStyle)item).Width;
				if(lw>this._MaxLineWidth)this._MaxLineWidth=lw;
				return base.Add(item);
			}
			return -1;
		}
		public int MaxLineWidth{
			get{return this._MaxLineWidth;}
		}
		protected override void OnCompile()
		{
			this.Insert(0,new UI8(this.Count));
		}
	}
	
	/// <summary>
	/// Defines any shape FillStyle
	/// </summary>
	public class FillStyle: ArrayData
	{
		
		public IShapeData Parent;
		private RGB _SolidColor;
		private Gradient _Gradient;FillStyleType _Type ;
		private Character _ImageFill;
		
		protected override void OnCompile()
		{    
					
			this.Add(new UI8((int)_Type));  
			
			if(_Type== FillStyleType.Solid) this.Add(this._SolidColor); //-Fill Color
			
			
			if(_Type== FillStyleType.LinearGradient || _Type== FillStyleType.RadialGradient)
			{
				Matrix mx = new Matrix();
				
				//- Tama�o del gradiente
				//- x =(32768 / dimension_del_shape  => Tama�o = 1/x
				double sx=1/((double)32768/this.Parent.Bounds.Width);
				double sy=1/((double)32768/this.Parent.Bounds.Height);
				sx*=((double)this._Gradient.PercentScaleX/100);
				sy*=((double)this._Gradient.PercentScaleY/100);
				mx.Scale(sx,sy);  
				if(this._Gradient.Rotation!=0) mx.Rotate(this._Gradient.Rotation);
				//- El gradiente por defecto hay que trasladarlo al centro del shape = centro del gradiente
				mx.Tanslate((int)(this.Parent.Bounds.Width/2)+this._Gradient.OffsetX ,(int)(this.Parent.Bounds.Height/2)+this._Gradient.OffsetY);
				this.Add(mx);
				this.Add(this._Gradient);
			}
			
			if(_Type==FillStyleType.TiledBitmap || _Type==FillStyleType.ClippedBitmap){
				Matrix mx = new Matrix();

				mx.Scale(20.0,20.0); //- tama�o real
				this.Add(new UI16(this._ImageFill.CharacterID));
				this.Add(mx);
				
			}
		}

		public FillStyle(RGB SolidFillColor)
		{  
			this._SolidColor=SolidFillColor;
			this._Type=FillStyleType.Solid;
		}
		public FillStyle(Gradient FillGradient)
		{
			this._Gradient=FillGradient;
			this._Type=(FillStyleType)FillGradient.Type;
		}
		public FillStyle(Character ImageFill, BitmapFill bitmapFill)
		{
			this._ImageFill=ImageFill;
			this._Type=(FillStyleType)bitmapFill;
		}
	}
	
	/// <summary>
	/// Type of FillStyle
	/// </summary>
	public enum FillStyleType
	{
		Solid=0, LinearGradient=16, RadialGradient=18, TiledBitmap=64, ClippedBitmap=65
	}
	
	/// <summary>
	/// Type of FillStyle when gradient fill
	/// </summary>
	public enum GradientType
	{
		LinearGradient=16, RadialGradient=18
	}
	
	/// <summary>
	/// Type of FillStyle when bitmap fill
	/// </summary>
	public enum BitmapFill{
		TiledBitmap=64, ClippedBitmap=65
	}
	
	/// <summary>
	/// Array of FillStyles used by shape
	/// </summary>
	public class FillStyleArray: ArrayData
	{
		IShapeData Parent;
		protected override void OnCompile()
		{
			this.Insert(0,new UI8(this.Count));   // count styles
		}
		public FillStyleArray(IShapeData shape)
		{
			this.Parent=shape;
		}
		public override int Add(object value)
		{
			((FillStyle)value).Parent=this.Parent;
			return base.Add(value);
		}


	}
	
	/// <summary>
	/// Defines a gradient between 0 and 8 colors
	/// </summary>
	public class Gradient: ArrayData
	{
	
		public int OffsetX=0, OffsetY=0; // moverser con respecto al centro del gradiente
		public int Rotation=0;			//angulo de rotacion
		public GradientType Type=GradientType.LinearGradient;
		public int PercentScaleX=100;
		public int PercentScaleY=100;
		protected override void OnCompile()
		{
			this.Insert(0,new UI8(this.Count));   // NumGradientes [1-8]
		}
		public Gradient(GradientType type){
			this.Type=type;
		}
		public Gradient(GradientType type, int Angle):this(type)
		{
			this.Rotation=Angle;
		}
		public Gradient(GradientType type, int offsetX, int offsetY):this(type)
		{
			this.OffsetX=offsetX;
			this.OffsetY=offsetY;
		}
		
		public void Scale(int PercentX, int PercentY){
			this.PercentScaleX=PercentX;
			this.PercentScaleY=PercentY;
		}
		
		public void MoveOffset(int offsetX, int offsetY){
			this.OffsetX=offsetX;
			this.OffsetY=offsetY;
		}
		
		public void AddColor(RGB Color, int Position){
			if(Position>255) Position=255;
			if(Position<0) Position=0;
			if(this.Count>7) return; // solo se admiten 8 gradientes
			this.Add(new GradRecord(Position, Color));
		}
	}

	/// <summary>
	/// Defines color position in the Gradient class
	/// </summary>
	class GradRecord: ArrayData
	{
		public GradRecord(int Ratio, RGB Color)
		{
			this.Add(new UI8(Ratio)); // entre 0-255
			this.Add(Color);
		}
	}


}
