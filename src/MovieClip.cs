/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
 
using System;
using System.Collections;

using SwfDotnet.Format;
using SwfDotnet.Format.BasicTypes;
using SwfDotnet.Format.UtilTypes;
using SwfDotnet.Format.Tags;
using SwfDotnet.Format.ActionScript;

namespace SwfDotnet
{
	/// <summary>
	/// Creates a swf movie entity 
	/// </summary>
	public class Movie:ArrayData{
		RGB BackColor;
		public int Width, Height;
		public int FrameRate=12;
		public FrameArray Frames ;
		public Layers Layers = new Layers();
		private int _charID=0;
		
		Hashtable Dictionary = new Hashtable();
		
	
		protected override void OnCompile()
		{	
			this.Add(this.Frames);
			this.Add(new TagEnd());
			
			this.Insert(0, new  EnableDebugger2());
			this.Insert(0, new TagBgColor(BackColor));
			
			this.Insert(0,new Header(this.Width, this.Height,this.Length, this.Frames.Length, this.FrameRate));
			
			//--- no poner nada debajo ---
		}
		public Movie(int width, int height,RGB backColor){
			this.BackColor=backColor;
			this.Height=height;
			this.Width=width;
			Frames = new FrameArray( );
		}
		
		public bool IsDefined(Character character){
			return this.Dictionary.ContainsKey(character.GetHashCode());
		}
		
		public void Define(Character character){
			//-Est� ya definido ??
			if( this.IsDefined( character) ){
				//- ??? ya esta definido
				return;
			}else{
				this.Dictionary.Add( character.GetHashCode(), null );
			}
			
			this._charID++;
			character.CharacterID=this._charID;
			this.Add(character);		
		}
		public void Define(SwfFont font)
		{
			this._charID++;
			font.FontID=this._charID;
			this.Add(font);		
		}
		public void SaveToFile(string path){
			System.IO.FileStream stream = System.IO.File.Open (path, System.IO.FileMode.Create,System.IO.FileAccess.Write);
			System.IO.BinaryWriter BW = new System.IO.BinaryWriter(stream);
			
			this.AdjustDephts();
			
			BW.Write(this.GetBytes);
			BW.Flush();
			BW.Close();
			stream.Close();
		}
		
		private void AdjustDephts()
		{//- Ajusta todas los Depths de la pelicula
			int depth=1;
			for(int x=1; x<=this.Frames.Count; x++){
				depth+=this.Frames[x].AdjustDephts(depth);
			}
		}
	}

	public class FrameArray: ArrayData{

		 public Frame this[int index]{
			get{
				if(index<=0){
					throw new Exception("FrameArray is 1 based array. First Frame has index 0");
				}
				int diff=index-this._arr.Count;
				for(int f=0;f<diff;f++){
					this.Add(new Frame());
				}
				return (Frame)(this._arr[index-1]);
			}

		}
	}
	
	public class Frame:ArrayData{
		
		public Script Script = new Script();
		protected override void OnCompile()
		{	
			if(Script.Count>0) this.Add(new TagDoAction(Script));
			this.Add(new TagShowFrame());
		}
		public void Add(Character Character)
		{
		   base.Add (new TagPlaceObject2(Character, 1, null));
		}
		//-???? para probar sprites
		public void Add(Character Character, string Name, Matrix TransformMatrix)
		{	TagPlaceObject2 tp= new TagPlaceObject2(Character, 1, TransformMatrix);
			tp.Name=Name;
			base.Add (tp);
		}
		
		public void Add(Character Character, string Name)
		{
			this.Add(Character, Name, null);
		}
		
		

		public void Add(Character Character, Matrix TransformMatrix)
		{	
			base.Add (new TagPlaceObject2(Character, 1, TransformMatrix));
		}
		
		public void Remove(Character Character)
		{
			base.Add(new TagRemoveObject2(Character));
		}
		
		public int AdjustDephts(int Depth)
		{//- Ajusta todas los Depths de la pelicula
			int tmp=0;
			for(int x=0; x<this._arr.Count; x++)
			{
				if(this._arr[x] is IDepthChanger){
					tmp+=((IDepthChanger)this._arr[x]).SetDepth(tmp+Depth);
				}
				
			}
			return tmp;
		}	
	}
	
	public class Layers
	{
		ArrayList lys = new ArrayList();	
		Layer Current;
				
		public void Add(string Name)
		{	
			Layer ly = new Layer(Name);
			this.lys.Add(ly);
			this.Current = ly;
		}
		
		public void AdjustDephs(){
			for(int l=0; l<lys.Count; l++){
			
			}
		}
		
		
	}
	
	
	public class Layer
	{		
		public string Name;
		ArrayList chars = new ArrayList();  //- Lista de caracteres
		
		public Layer(string name){
			this.Name = name;
		}
		
		public void Add(int CharID){
			this.chars.Add(CharID);
		}
	}
	

	
	internal class Header: ArrayData
	{
		public Header(int width, int height, int fileLength, int FrameCount, int FrameRate)
		{
			//- FileLength=Longitud de archivo Sin contar esta cabecera
			this._FileLength=  fileLength;
			this._FrameSize= new RECT(0, 0, width, height);
			this._FrameCount= new UI16(FrameCount);
			this._FrameRate = new FB8(FrameRate);
		}
 
		private UI8  _version = new UI8(6);
		private RECT _FrameSize ;
		private FB8  _FrameRate  ;
		private UI16 _FrameCount ;
		private int _FileLength  ;

		protected override void OnCompile()
		{
			this.Add(new UI8('F'));
			this.Add(new UI8('W'));
			this.Add(new UI8('S'));
			this.Add(_version);
			this.Add(new UI32(_FileLength +(12+this._FrameSize.Length)));
			this.Add(_FrameSize);
			this.Add(_FrameRate);
			this.Add(_FrameCount);
		}

	}

	public class DefineSprite:Character, IDepthChanger 
	{
		public FrameArray _Frames = new FrameArray();
		
		public virtual FrameArray Frames{
			get{
				return this._Frames;
			}
		}
		
		protected override void OnCompile()
		{	this.Add(new UI16(this.CharacterID));
			this.Add(new UI16(this.Frames.Count));
			this.Add(this.Frames);
			this.Add(new TagEnd());
			base.OnCompile ();
		}

		public DefineSprite():base(39)
		{
			
		}
		#region Miembros de IDepthChanger

		public int SetDepth(int depth)
		{
			int depthn=0;
			for(int x=1; x<=this.Frames.Count; x++)
			{
				depthn+=this.Frames[x].AdjustDephts(depthn + depth);
			}
			
			return depthn;
		}

		#endregion
	}
}