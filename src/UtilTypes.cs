/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/

using System;
using System.Collections;
using SwfDotnet.Format.BasicTypes;

namespace SwfDotnet.Format.UtilTypes
{

	
	// Almacena en serie un lista de datos IBytes
	public class ArrayData : /*ArrayList , */IBytes
	{
		bool _Compiled=false;
		protected ArrayList _arr = new ArrayList();
		//-Debe ser sobreescrito para completar los datos necesarios 
		protected virtual void OnCompile(){	}
		
		public virtual void Compile(){
			if(!this._Compiled)
			{ 
				this._Compiled=true;
				this.OnCompile();
			}
		}
		
		public virtual int Add(object Item){
			return this._arr.Add(Item);
		}
		public virtual void Insert (int Pos, object Item){
			this._arr.Insert(Pos, Item);
		}
		public int Count{
			get{return this._arr.Count;}
		}
		/*public virtual object this[int Index]{
			get{
				return this._arr[Index];
			}
		}*/
		
		public virtual int Length
		{
			get
			{
				this.Compile();
				int temp=0;
				IBytes x;
				for (int i=0;i<this.Count;i++)
				{
					x = (IBytes)this._arr[i];
					temp+= x.Length;
				} 
				return temp;}
		}
		public virtual byte[] GetBytes
		{
			get
			{
				this.Compile();
    
				byte[] tempB = new byte[this.Length];
				int index=0;
				IBytes x;
				for (int i=0;i<this.Count;i++)
				{
					x = (IBytes)this._arr[i];
					x.GetBytes.CopyTo(tempB,index);
					index+= x.Length;
				} 
				return tempB;
    
			}
		}

	}

	/// <summary>
	/// mantiene una simetria de posicion actual de dibujo y
	/// los limites que vamos abarcando en nuestros dibujo
	/// </summary>
	public class Boundarier{
		private int X1=0, Y1=0;	//esquina sup-izda
		private int X2=0, Y2=0;	//esquina ind-dcha
		public int CurrX=0, CurrY=0; //Coordenadas actuales
		private bool Empty=true; // indica si todavia no se ha inicializado
		public int difX=0, difY=0; //diferencia al mover al origen
				
		public RECT Bounds{
			get{ return new RECT(X1, Y1, X2, Y2);}
		}
		//Mueve el RECT al inicio de coordenadas 0,0
		public void MoveToOrig(){
		    difX=X1; difY=Y1;
			X1-=difX; X2-=difX;
			Y1-=difY; Y2-=difY;
		}
		
		public void NewPos(int AbsX, int AbsY){
			if(Empty){
			  X1=X2=AbsX; Y1=Y2=AbsY;
			  Empty=false;
			}else{
			  if(AbsX<X1) X1=AbsX;
			  if(AbsX>X2) X2=AbsX;
			  if(AbsY<Y1) Y1=AbsY;
			  if(AbsY>Y2) Y2=AbsY;
			}
			CurrX=AbsX; CurrY=AbsY;
		}
		public void UpdatePos(int IncX, int IncY){
		    CurrX+=IncX; CurrY+=IncY; 
			if(CurrX<X1) X1=CurrX;
			if(CurrX>X2) X2=CurrX;
			if(CurrY<Y1) Y1=CurrY;
			if(CurrY>Y2) Y2=CurrY;
		}
		//- Abre el rectangulo Increment unidades por los lados
		public void OpenRect(int Increment){
			this.X1-=Increment;
			this.Y1-=Increment;
			this.X2+=Increment;
			this.Y2+=Increment;
		}

	}
	
	/// <summary>
	/// 
	/// </summary>
	public class CXFORM:AB{
		int aR, aG, aB;  //adition RGB
		int mR, mG, mB;	 // mult RGB
		bool hasAdd=false;
		bool hasMult=false;
		
		protected override void OnCompile()
		{  
			this.Append(new UB(1, (Bit)hasAdd));  //has color adition
			this.Append(new UB(1, (Bit)hasMult));  //has color mult
			
			BitCounter.Init();
				if(hasAdd){
					BitCounter.Push(SB.NumBits(this.aR));
					BitCounter.Push(SB.NumBits(this.aG));
					BitCounter.Push(SB.NumBits(this.aB));
				}
				if(hasMult){
					BitCounter.Push(SB.NumBits(this.mR));
					BitCounter.Push(SB.NumBits(this.mG));
					BitCounter.Push(SB.NumBits(this.mB));
				}
			int nBits = BitCounter.Maxim;
				
			this.Append(new UB(4, nBits));
			if(hasAdd)
			{
				this.Append(new SB(nBits,this.aR));
				this.Append(new SB(nBits,this.aG));
				this.Append(new SB(nBits,this.aB));
			}
			if(hasMult)
			{
				this.Append(new SB(nBits,this.mR));
				this.Append(new SB(nBits,this.mG));
				this.Append(new SB(nBits,this.mB));
			}
			base.OnCompile ();
		}

		public CXFORM(RGB AddColor, RGB MultColor):base(-1)
		{
			this.hasAdd=true;
			this.hasMult=true;
			this.aR=AddColor.Red;
			this.aG=AddColor.Green;
			this.aB=AddColor.Blue;
			this.mR=MultColor.Red;
			this.mG=MultColor.Green;
			this.mB=MultColor.Blue;
			
			
		}
	
	}
	public interface IBoundsChanger
	{	// Pasa el Bounds como referencia para que cada record lo updatee
		void UpdateBounds(Boundarier bounds);
		// hace que cada record se situe en el origen de coordenadas
		void RecordToOrig(Boundarier bounds);
	}

}