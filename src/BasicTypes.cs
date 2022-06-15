/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
using System;
using System.Text;
using System.Collections;

namespace SwfDotnet.Format.BasicTypes{

	// Se aplica al tipo de datos que genere array de Bytes
	public interface IBytes{
	byte[] GetBytes { get;}
	int Length{get;}
	}
	// Aplicable al tipo de datos que genere sucesi�n de Bites
	public interface IBitable{
		string GetBits();
	}

	// Constructor de Bytes desde UB, y SB
		
	public class AB :IBytes, IBitable
	{
		int _nBytes;
		bool _reverse;		//- si true, en GetBytes devuelve 35 24 en vez de 24 35
		StringBuilder _temp;
		public ArrayList IBitables = new ArrayList();
	  
		bool _Compiled=false;
		//-Debe ser sobreescrito para completar los datos necesarios del tag
		protected virtual void OnCompile(){	}
		
		public virtual void Compile(){
			if(!this._Compiled)
			{ 
				this._Compiled=true;
				this.OnCompile();
			}
		}
		public AB(int numBytes)
		{	  _nBytes = numBytes;_temp= new StringBuilder();
			_reverse = false;
		}
		public string GetBits(){
			this.Compile();
			this.MakeString();
			return this._temp.ToString();
		}
		public AB(int numBytes, bool reverse){
			_nBytes = numBytes;_temp= new StringBuilder();
			_reverse = reverse;
		}
		  
		
		public virtual void Append(IBitable bitable)
		{	this.IBitables.Add(bitable);
		}
	
	  
		private void MakeString(){
		if(this._temp.Length>0) return;
			for(int i =0; i<this.IBitables.Count; i++){
				this._temp.Append(((IBitable)this.IBitables[i]).GetBits());
			}
		}
		//- Completa con ceros a la derecha
		void Complete()
		{	this._nBytes=this.Length;
			int L = _temp.Length;
			if (L> _nBytes*8){			//- Le hemos a�adido m�s Bits de su capacidad
			throw new Exception("AB Overflow");
			}else{
			_temp.Append(new string('0',_nBytes*8-L));
			}
		}
		public byte[] GetBytes
		{
		get{
			this.Compile();
			this.MakeString();
			this.Complete();
			byte[] tempB = new byte[_nBytes];
			for(int c=0;c<_nBytes;c++){
			if(!this._reverse){
				tempB[c]=Convert.ToByte(_temp.ToString().Substring(c*8,8),2);
			}else{
				tempB[_nBytes - (c+1)]=Convert.ToByte(_temp.ToString().Substring(c*8,8),2);
			}
		        
			}
			return tempB;
		}
		}

		  
		public int Length{
			get{
				this.Compile();
				this.MakeString();
				if(this._nBytes==-1) {
				//CT
					return (int)(Math.Ceiling((float)this._temp.Length/8));
				}else  {
					return this._nBytes;
				}
							
	}}
	}

	// Array de Unsigned Bits
	// - Los valores tipo 7.5 se escriben a disco con (int)7.5*65536.0
	public struct UB: IBitable
	{ private int _value;
	private int _nBits;
	 
	public UB (int nBits){_nBits = nBits;_value=0;}
	public UB (int nBits, int val){_nBits = nBits;_value=val;}
	 
	public int Value {
	get{return _value;}
	set{_value = value;}}
	 
	public string GetBits()
	{ StringBuilder temp = new StringBuilder(Convert.ToString((long)_value,2).PadLeft(_nBits,'0'));
		int L = temp.Length;
		return temp.ToString(L-_nBits,_nBits);    
	}
	  
	//- Devuelve el n�mero de bits de un un-signed
	public static int NumBits(long val){
		int topBit = 32;
		long mask = 0x80000000L;
				
		while (topBit > 0) {
			if ((val & mask) != 0)return topBit;
					
			mask >>= 1;
			topBit--;
		}
		return 0;
	}
	}

	public struct Bits: IBitable 
	{	private string _val;
		public Bits(string BitArray){
			this._val=BitArray;
		}
		public string GetBits(){
			return _val;
		}
		
	}


	// Representa un FIXED 8.8  (ej.: 7.5)
	public struct FB8: IBytes
	{ private double _value;
	   
	public FB8 (float val){_value = (double)val;}
	public FB8 (double val){_value = val;}
	 
	public byte[] GetBytes
	{ 
		get{
		Int16 ti= Convert.ToInt16(_value*256.0);
		return BitConverter.GetBytes(ti);
		}
	}
	public int Length{get{return 2;}}
	   
	public static implicit operator FB8(double x){
		return new FB8(x);
	}
	public static implicit operator double(FB8 x){
		return x._value;
	}
	}

		//medida de SWF = valor*20
		// Pasar facilmente con cast int x=(Twip)1 
		public struct Twip
		{
			private int _value;
	   
			public Twip (int val){_value = val*20;}
	   
			public static implicit operator Twip(int x)
			{
				return new Twip(x);
			}
			public static implicit operator int(Twip x)
			{
				return x._value;
			}
		}
		
	public struct Bit
	{
		private int _value;
	   
		public Bit (int val){_value = val*20;}
	   
		public static implicit operator Bit(int x)
		{
			return new Bit(x);
		}
		public static implicit operator Bit(bool x)
		{
			return new Bit(x?1:0);
		}
		public static implicit operator int(Bit x)
		{
			return x._value==0?0:1;
		}
	}


	// Representa un FIXED 16.16  (7.5)
	public struct FB16: IBytes
	{ private double _value;
	   
	public FB16 (float val){_value = (double)val;}
	public FB16 (double val){_value = val;}
	 
	public byte[] GetBytes { 
		get{
		//CT
		int ti= (int)(_value*65536.0);
		return BitConverter.GetBytes(ti);
		}
	}
	public int Length{get{return 4;}}
	    
	public static implicit operator FB16(double x){
		return new FB16(x);
	}
	public static implicit operator double(FB16 x){
		return x._value;
	}
	  
	}

		public struct FB: IBitable 
		{
			private double _value;
			private int _nbits;
			public FB (int nBits, float val){_value = (double)val; _nbits=nBits;}
			public FB (int nBits, double val){_value = val; _nbits=nBits;}
			public static int NumBits(double val){
				int ti= (int)(val*65536.0);
				return SB.NumBits(ti);
			}
			public string GetBits(){
			    //CT
				int ti= (int)(_value*65536.0);
				SB s = new SB(_nbits, ti);
				return s.GetBits();
			}  
		}



	// Array de Signed Bits
	public struct SB: IBitable
	{ private int _value;
	private int _nBits;
	 
	public SB (int nBits){_nBits = nBits;_value=0;}
	public SB (int nBits, int val){_nBits = nBits;_value=val;}
	 
	public int Value {
	get{return _value;}
	set{_value = value;}}
	 
	public string GetBits()
	{ StringBuilder temp; 
		long lval = _value & 0x7FFFFFFF;
		if (_value < 0){//A�ado bit de signo
					lval |= 1L << (_nBits - 1);
		temp= new StringBuilder(Convert.ToString(lval,2).PadLeft(_nBits,'1'));			
		}else{
		temp= new StringBuilder(Convert.ToString(lval,2).PadLeft(_nBits,'0'));
		}
	int L = temp.Length;
	return temp.ToString(L-_nBits,_nBits);    
	}
		//- devuelve el numero de bits de un Signed
		public static int NumBits(int val){
			if (val >= 0) return UB.NumBits(val) + 1;
				
			int topBit = 31;
			long mask = 0x40000000L;
				
			while (topBit > 0){
				if ((val & mask) == 0) break;
				mask >>= 1;
				topBit--;
			}
				
			if (topBit == 0) return 2;
							
			int val2 = val & ((1 << topBit) - 1);
			if (val2 == 0) topBit++;
							
			return topBit + 1;
		}
		
		
	}


	public struct SI8:IBytes
	{
	private int _value;
	 
	public SI8(int val){_value=val;}
	  
	public byte[] GetBytes
	{ 
	get{
		AB tab= new AB(1);
		tab.Append( new SB(8,_value));
		return tab.GetBytes;
	}
	}
	public int Length{get{return 1;}}
	 
	public static implicit operator SI8(int x){
		return new SI8(x);
	}
	public static implicit operator int(SI8 x){
		return x._value;
	}
	  
	}


	public struct SI16:IBytes
	{
	int _value;
	 
	public SI16(int val){_value=val;}
	  
	public byte[] GetBytes
	{ 
	get{
	AB tab= new AB(2);
	tab.Append( new SB(16,_value));
	return tab.GetBytes;
		
	}
	}
	public int Length{get{return 2;}}
	public static implicit operator SI16(int x){
		return new SI16(x);
	}
	public static implicit operator int(SI16 x){
		return x._value;
	}
	   
	}

	public struct SI32:IBytes
	{
	private int _value;
	 
	public SI32(int val){_value=val;}
	  
	public byte[] GetBytes
	{ 
	get{
		AB tab= new AB(4);
		tab.Append( new SB(32,_value));
		return tab.GetBytes;
	   
	}
	}
	public int Length{get{return 4;}}
	 
	public static implicit operator SI32(int x){
		return new SI32(x);
	}
	public static implicit operator int(SI32 x){
		return x._value;
	}
	}

	public struct UI8:IBytes
	{
	public int _value;
	 
	public UI8(int val){_value=val;}
	public UI8(char val){_value=(int)(val);} //CT
	  
	public byte[] GetBytes
	{ 
	get{
		byte[] myByteArray = new byte[1];
		byte val = Convert.ToByte(_value);
	        
		myByteArray[0] = (BitConverter.GetBytes(val))[0];
	   
		return myByteArray;}
	}
	public int Length{get{return 1;}}
	  
	public static implicit operator UI8(int x){
		return new UI8(x);
	}
	public static implicit operator int(UI8 x){
		return x._value;
	}
	}


	public struct UI16:IBytes
	{
	private int _value;
	 
	public UI16(int val){_value=val;}
	 
	public byte[] GetBytes
	{ 
	get{
		byte[] myByteArray = new byte[2];
		Int16 val = Convert.ToInt16(_value);
	        
		myByteArray = BitConverter.GetBytes(val);
		return myByteArray;}
	}
	public int Length{get{return 2;}}
	  
	public static implicit operator UI16(int x){
		return new UI16(x);
	}
	public static implicit operator int(UI16 x){
		return x._value;
	}
	}

	public struct UI32:IBytes
	{
	private int _value;
	 
	public UI32(int val){_value=val;}
	  
	public byte[] GetBytes
	{ 
	get{
		byte[] myByteArray = new byte[4];
		Int32 val = (int)(_value);//CT
	        
		myByteArray = BitConverter.GetBytes(val);
		return myByteArray;}
	}
	public int Length{get{return 4;}}
	  
	public static implicit operator UI32(int x){
		return new UI32(x);
	}
	public static implicit operator int(UI32 x){
		return x._value;
	}
	}

	
	public struct RECT:IBytes{
		    
			int bitSize ;
			int minX, minY, maxX, maxY;
					
			public int MinX	{
				get{return minX;}
				set{this.minX = value; bitSize = - 1;}
			}
			public int MinY	{
				get{return minY;}
				set{this.minY = value; bitSize = - 1;}
			}
			public int MaxX{
				get{return maxX;}
				set{this.maxX = value; bitSize = - 1;}			
			}
			public int MaxY{
				get{return maxY;}			
				set{this.maxY = value; bitSize = - 1;}			
			}
		public int Width
		{
			get{return maxX-minX;}			
				
		}
		public int Height
		{
			get{return maxY-minY;}			
				
		}
			// Devuelve la componente de la recta con mas Bits
			int BitSize{
				get	{
					if (bitSize == - 1)	{
						BitCounter.Init();
						BitCounter.Push(SB.NumBits(minX));
						BitCounter.Push(SB.NumBits(maxX));
						BitCounter.Push(SB.NumBits(minY));
						BitCounter.Push(SB.NumBits(maxY));
						bitSize=BitCounter.Maxim;
						
					}
					
					return bitSize;
				}
			}
			
			public int Length
			{	get{
				int BS=this.BitSize;
				int L = (int)Math.Ceiling((double)(BS*4+5)/8);
				return L;
				}
			}
		    
			//Devuelve una matriz de Bytes que representa a la recta
			public byte[] GetBytes{
			get{
			// Cuantos Bytes har�n falta ?? (5 + 4 Puntos)
			int BS=this.BitSize;
			int L = this.Length;	     
		      
			AB tempB = new AB(L) ;	     
		      
			tempB.Append(new UB(5,BS));
			tempB.Append(new SB(BS,this.minX));
			tempB.Append(new SB(BS,this.maxX));
			tempB.Append(new SB(BS,this.minY));
			tempB.Append(new SB(BS,this.maxY));
		      	     
			return tempB.GetBytes;}
			}
		    
		
			
			public RECT(int minX, int minY, int maxX, int maxY){
				this.minX = minX;this.minY = minY;
				this.maxX = maxX;this.maxY = maxY;
				this.bitSize=-1;
			}
			
			public override System.String ToString(){
				return "RECT bitsize=" + BitSize + " (" + minX + "," + minY + ")-(" + maxX + "," + maxY + ")";
			}
					
		}
		

		public struct STRING:IBytes	{
			private string _val;
			private bool EOS; // a�adir fin de cadena al final del string
			public STRING(string val){
				this._val=val;this.EOS=true;
			}
			public STRING(string val, bool EndOfString)
			{
				this._val=val;this.EOS=EndOfString;
			}
			public byte[] GetBytes
			{ 
				get
				{	
					int nbytes=this._val.Length;
					if(this.EOS)nbytes++;
					
					byte[] myByteArray = new byte[nbytes];
					
					for(int c=0; c<this._val.Length; c++){
						byte val = Convert.ToByte(Convert.ToChar(this._val.Substring(c,1)));
						myByteArray[c] = (BitConverter.GetBytes(val))[0];
					}
					if(this.EOS) myByteArray[nbytes-1]=Convert.ToByte(0);
					return myByteArray;}
			}
			public int Length{get{
								  int nbytes=this._val.Length;
								  if(this.EOS)nbytes++;
								  return nbytes;}}
	  
		}
		
		
	public struct RGBA: IBytes{
			
			public int Red, Green, Blue, Alpha;
			
			public RGBA(int red, int green, int blue, int alpha)
			{
				this.Red = red;
				this.Green = green;
				this.Blue = blue;
				this.Alpha=alpha;
			}
										
			public byte[] GetBytes{
			get{
				byte[] tempB = new byte[4];
				tempB[0] = Convert.ToByte(this.Red);
				tempB[1] = Convert.ToByte(this.Green);
				tempB[2] = Convert.ToByte(this.Blue);
				tempB[3] = Convert.ToByte(this.Alpha);
				return tempB;
			}
			}
			public int Length{
				get{return 4;}
			}
						
			
		}
		

	public struct RGB: IBytes
		{
			public int Red, Green , Blue;
					
			public RGB(int red, int green, int blue)
			{
				this.Red = red;
				this.Green = green;
				this.Blue = blue;
			}
		#region Miembros de IBytes									
			public byte[] GetBytes{
			get{
				byte[] tempB = new byte[3];
				tempB[0] = Convert.ToByte(this.Red);
				tempB[1] = Convert.ToByte(this.Green);
				tempB[2] = Convert.ToByte(this.Blue);
				return tempB;
			}
			}


		public int Length
		{
			get
			{
				return 3;
			}
		}

		#endregion
	}	

	//- Contador selector de bites
	//- haciendo varios Push va almacenando el mayor valor pasado	
	public class BitCounter{
		static int _maxCount=0;
		public static void Init(){
			_maxCount=0;
		}
		public static void Push(int num){
			if(num >_maxCount){
				_maxCount=num;
			}
		}
		public static int Maxim{
			get{return _maxCount;}
			set{_maxCount=value;}
		}
		

	}	
	
	
	
	public class Matrix: AB
	{
		bool hasScale=false, hasRotate=false, hasTranslation=false;
		double _angle=0.0;
		double _scaleX=1.0, _scaleY=1.0;
		double _skewX, _skewY;
		int _tranX, _tranY;
		int _objX=0, _objY=0; //x, y del objeto a aplicar la matriz
		
		double _scX=1.0, _scY=1.0;
		double _rt0, _rt1;
		int _trX, _trY;
		
		
		public void Rotate(int Angle)
		{
			this.hasRotate=true;
			this._angle=Angle;
			
		}
		public void Skew(double SkewX, double SkewY)
		{
			this.hasRotate=true;
			this._skewX=SkewX;this._skewY=SkewY;			
		}
		
		private void Apply()
		{
			double cRot = (double)Math.Cos(this._angle * Math.PI/180.0);
			double sRot = (double)Math.Sin(this._angle * Math.PI/180.0);
			double xS = this._skewX, yS = this._skewY;  //-skew
				
			this._scX=this._scaleX * (cRot - xS*sRot);
			this._rt0=this._scaleX * (yS*cRot - (xS*yS+1)*sRot);
			this._rt1=this._scaleY * (sRot + xS*cRot);
			this._scY=this._scaleY * (yS*sRot + (xS*yS+1)*cRot);
			this._trX=this._tranX;
			this._trY=this._tranY;	
		}
		

		
		public void Scale(double ScaleX, double ScaleY)
		{
			this.hasScale=true;
			this._scaleX=ScaleX; this._scaleY=ScaleY;
		}
		public void Tanslate(int X, int Y)
		{
			this.hasTranslation=true;
			this._tranX=X; this._tranY=Y;
		}
				
		protected override void OnCompile()
		{
			this.Apply();
			//Si hay rotacion hay que hacer Scale
			if(hasRotate) hasScale=true;
			
			this.Append(new UB(1, (Bit)hasScale));
			if(hasScale)
			{	
				double coseno=Math.Cos(this._angle);
					
				BitCounter.Init();
				BitCounter.Push(FB.NumBits(this._scX));
				BitCounter.Push(FB.NumBits(this._scY));
				int nBits = BitCounter.Maxim;
				
				this.Append(new UB(5, nBits));
				this.Append(new FB(nBits, this._scX));
				this.Append(new FB(nBits, this._scY));
			}
			
			this.Append(new UB(1, (Bit)hasRotate));
			if(hasRotate)
			{
				double seno=Math.Sin(this._angle);
				BitCounter.Init();
				BitCounter.Push(FB.NumBits(this._rt0));
				BitCounter.Push(FB.NumBits(this._rt1));
				int nBits = BitCounter.Maxim;
				
				this.Append(new UB(5, nBits));
				this.Append(new FB(nBits, this._rt0));
				this.Append(new FB(nBits, this._rt1));
			}
			if(this.hasTranslation)
			{			
				//-Reajuste en rotacion
				int TranX=this._tranX -this._objX ;
				int TranY=this._tranY -this._objY;
				
				if(this.hasRotate)
				{
					double coseno=Math.Cos(this._angle);
					double ScaleX= this._scaleX * coseno;
					double ScaleY= this._scaleY * coseno;
					
					TranX=this._tranX -(int)(this._objX*ScaleX) + (int)(this._objY*Math.Sin(this._angle)) ;
					TranY=this._tranY-(int)(this._objX*Math.Sin(this._angle)) - (int)(this._objY*ScaleY);
				}
				
				BitCounter.Init();
				BitCounter.Push(SB.NumBits(this._trX));
				BitCounter.Push(SB.NumBits(this._trY));
				int nBits = BitCounter.Maxim;
				
				this.Append(new UB(5, nBits));
				this.Append(new SB(nBits, this._trX));
				this.Append(new SB(nBits, this._trY));
			}
			else
			{
				this.Append(new UB(5, 0));
			}
		}

		
		public Matrix():base(-1){}
	}
	
	
}
