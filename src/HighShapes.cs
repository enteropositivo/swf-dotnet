/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
using System;

using SwfDotnet.Format.Shapes;
using SwfDotnet.Format.BasicTypes;
using SwfDotnet.Format.Tags;

namespace SwfDotnet.Shapes
{
	public class Rectangle: DefineShape
	{
		int cx, cy;
		LineStyle _lineStyle;
		FillStyle _fillStyle;
		int Curve=800;  
		 
		public FillStyle FillStyle
		{
			set
			{
				this._fillStyle=value;
			}
			get
			{
				return this._fillStyle;
			}
		}
		public LineStyle LineStyle
		{
			set
			{
				this._lineStyle=value;
			}
			get
			{
				return this._lineStyle;
			}
		}
		protected override void OnCompile()
		{	
			//- de momento todos los shapes hay que situarlos en el 0,0 para no tener 
			//- problemas con los gradients y matrix
			if(this._lineStyle!=null)  this.LineStyles.Add(this._lineStyle);
			if(this._fillStyle!=null)  this.FillStyles.Add(this._fillStyle);
			
			this.Records.Add(new StyleChangeRecord (Curve,Curve,1, 1, 0)); 
		
			this.Records.Add(new  StraightRecord(false,cx-(Curve*2)));
			if(Curve>0) this.Records.Add(new CurvedRecord(Curve, 0, 0, Curve));
			
			this.Records.Add(new  StraightRecord(true, cy-(Curve*2)));
			if(Curve>0) this.Records.Add(new CurvedRecord(0, Curve, -Curve, 0));
			
			this.Records.Add(new  StraightRecord(false,-(cx-(Curve*2))));
			if(Curve>0) this.Records.Add(new CurvedRecord(-Curve, 0, 0, -Curve));
			
			this.Records.Add(new  StraightRecord(true, -(cy-(Curve*2))));
			if(Curve>0) this.Records.Add(new CurvedRecord(0, -Curve, Curve, 0));
		
			base.OnCompile ();
		}

		public Rectangle(int width, int height, int roundCornerRadius)
		{
			cx=width; cy=height; this.Curve=roundCornerRadius;
		}
	
	}
	
	/// <summary>
	/// Free LineTool for drawing shapes.
	/// NOTE: Has 1 default Line and 1 default Fill, so if you add your styles use SetStyle(index>1, index>1)
	/// </summary>
	public class LineTool: DefineShape/*ShapeWithStyle*/{
		protected override void OnCompile()
		{	//this.Records.Insert(0, new StyleChangeRecord(0,0,1, 1,0));
			base.OnCompile ();
		}
		
		public new LineStyleArray LineStyles{
			get{return base.LineStyles;}
		}
		public new FillStyleArray FillStyles
		{
			get{return base.FillStyles;}
		}
		
		
		public void SetStyle(int LineIndex, int StyleIndex){
			this.Records.Add( new StyleChangeRecord(LineIndex, StyleIndex, 0));
		}
	
		
		public void HorizontalTo(int DeltaX)
		{
			this.Records.Add( new StraightRecord(false, DeltaX));
		}
		public void VerticalTo(int DeltaY)
		{
			this.Records.Add( new StraightRecord(true, DeltaY));
		}
		public void LineTo(int DeltaX, int DeltaY)
		{
			this.Records.Add( new StraightRecord(DeltaX, DeltaY));
		}
		public void CurveTo(int ADeltaX, int ADeltaY, int BDeltaX, int BDeltaY)
		{
			this.Records.Add( new CurvedRecord(ADeltaX, ADeltaY, BDeltaX, BDeltaY ));
		}
		public LineTool(){
			this.LineStyles.Add(new LineStyle(new RGB(0,0,0),1));
			this.FillStyles.Add(new FillStyle(new RGB(120,120,120)));
		}
	}
	
	public class Ellipse: DefineShape /*ShapeWithStyle*/
	{
		int cx, cy;
		LineStyle _lineStyle;
		FillStyle _fillStyle;
		 
		public FillStyle FillStyle
		{
			set
			{
				this._fillStyle=value;
			}
			get
			{
				return this._fillStyle;
			}
		}
		public LineStyle LineStyle
		{
			set
			{
				this._lineStyle=value;
			}
			get
			{
				return this._lineStyle;
			}
		}
		protected override void OnCompile()
		{	
			//- de momento todos los shapes hay que situarlos en el 0,0 para no tener 
			//- problemas con los gradients y matrix
			if(this._lineStyle!=null)  this.LineStyles.Add(this._lineStyle);
			if(this._fillStyle!=null)  this.FillStyles.Add(this._fillStyle);
			
			this.Records.Add(new StyleChangeRecord (0,0,1, 1, 0)); 
			this.Records.Add(new  CurvedRecord(-cx*0.146, cy*0.146, -cx*0.206, 0));
			this.Records.Add(new  CurvedRecord(-cx*0.206, 0, -cx * 0.146, -cy*0.146));
			this.Records.Add(new  CurvedRecord(-cx*0.146, -cy * 0.146, 0, -cy *0.206));
			this.Records.Add(new  CurvedRecord(0,-cy*0.206, cx * 0.146, -cy *0.146));
			this.Records.Add(new  CurvedRecord(cx*0.146, -cy * 0.146, cx *0.206, 0));		
			this.Records.Add(new  CurvedRecord(cx*0.206, 0, cx  * 0.146, cy *0.146));		
			this.Records.Add(new  CurvedRecord(cx*0.146, cy * 0.146, 0, cy *0.206));		
			this.Records.Add(new  CurvedRecord(0, cy * 0.206, -cx * 0.146, cy *0.146));
			
			base.OnCompile ();
		}

		public Ellipse(int width, int height)
		{
				cx=width; cy=height;
		}
	
	}	
		
}