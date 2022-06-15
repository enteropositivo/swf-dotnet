/*********************************************************************
  By @EnteroPositivo (Twitter, Gmail, GitHub)
  http://enteropositivo.github.io

  Repo: https://github.com/enteropositivo/swf-dotnet
 **********************************************************************/
 
using System;
using System.Collections;
using SwfDotnet.Format.BasicTypes;
using SwfDotnet.Format.UtilTypes;

namespace SwfDotnet.Format.ActionScript{
	
	
	public class Script:ArrayData{
	
		protected override void OnCompile()
		{	
			Action ifact=null;
			int ifoff=0;
			
			bool SumOffsets=false; //- Sumar Offset
			bool GetOffsets=false; //- Volvar offsets en ifact
			bool GetAndSum=false;  //- Vuelca offsets pero sigue cogiendo (tipo else)
			
			ArrayList tmp=(ArrayList)this._arr.Clone();
			foreach(Action a in tmp){
				
				if(a._code==0x9D) {
					SumOffsets=true;
					ifact=a;
					continue;
				}
				
				if(a._code==0x99){
				    GetAndSum=true;
				}
				
				if(a._code==0x00) {  // cierres totales de llaves }
					GetOffsets=true;
					//- elimino este tipo de acciones
					this._arr.Remove(a);
				}	
				
				if(GetOffsets){
					ifact.JumpOffset=ifoff;
					ifoff=0;
					ifact=null;
					GetOffsets=false;
					SumOffsets=false;
				}
				
				if(GetAndSum){
					GetAndSum=false;
					ifact.JumpOffset=ifoff+5;  //5 = Length de un Else, ya que tiene que saltarse por encima
					ifoff=0;
					ifact=a;
					continue;
				}
				
				if(SumOffsets) ifoff+=a.Length;
			}
			
			base.OnCompile ();
		}

		
		public void Push(string Data)
		{
			Action p= new Action(0x96);
			p.Add( new UI8(0));
			p.Add( new STRING(Data));
			this.Add(p);
		}
		public void Push(int DicIndex)
		{
			Action p= new Action(0x96);
			p.Add( new UI8(0x05));
			p.Add( new UI8(DicIndex));
			this.Add(p);
		}
		public void Push2(string Data, string Data2)
		{
			Action p= new Action(0x96);
			p.Add( new UI8(0));
			p.Add( new STRING(Data));
			p.Add( new UI8(0));
			p.Add( new STRING(Data2));
			this.Add(p);
		}
		public void Push2(int Data, int Data2)
		{
			Action p= new Action(0x96);
			p.Add( new UI8(0x08));
			p.Add( new UI8(Data));
			p.Add( new UI8(0x08));
			p.Add( new UI8(Data2));
			this.Add(p);
		}
		
		public void DeclareDictionary(string[] data){
			Action p= new Action(0x88);
			p.Add(new UI16(data.Length));
			for(int x=0;x<data.Length;x++){
				p.Add(new STRING(data[x],true));
			}
			this.Add(p);
		}
		
		public void Push3(string Data, string Data2, string Data3)
		{
			Action p= new Action(0x96);
			p.Add( new UI8(0));
			p.Add( new STRING(Data));
			p.Add( new UI8(0));
			p.Add( new STRING(Data2));
			p.Add( new UI8(0));
			p.Add( new STRING(Data3));
			this.Add(p);
		}
		public void Pop()
		{
			Action p= new Action(0x17);
			this.Add(p);
		}
		public void TellTarget(string Name){
			Action p= new Action(0x8B);
			p.Add(new STRING(Name));
			this.Add(p);
		}
		
		public void gotoAndStop(int FrameNumber){
			Action p= new Action(0x81);
			p.Add(new UI16(FrameNumber-1));
			this.Add(p);
			
		}
		public void gotoAndPlay(int FrameNumber)
		{
			Action p= new Action(0x81);
			p.Add(new UI16(FrameNumber-1));
			this.Add(p);
			this.Play();
		}
		
		public void DefineVar(){
			this.Add( new Action(0x41));		
		}
		public void SetVar(){
			this.Add(new Action(0x1D));
		}
		
		public void SetVar(string varName, Script RightVal)
		{	this.Push(varName);
			this._arr.AddRange(RightVal._arr);
			this.Add(new Action(0x1D));
		}

		public Script GetVar(string varName){
			Script tmp = new Script();
			tmp.Push(varName);
			tmp.GetVar();
			return tmp;
		}	
		
		public void If(){
		
			this.Not();
			Action p= new Action(0x9D);
			p.Jump=true;
			
			//- Offset   Hay que calcular cuantos Bytes nos saltamos si se cumple
			//p.Add(new  UI16(7));
			this.Add(p);
		}
		
		public void Else(){
			Action p= new Action(0x99);
			p.Jump=true;

			this.Add(p);
		}
		
		public void Equal(){
			Action p= new Action(0x0E);
			this.Add(p);
		}
		public void LessThan()
		{
			Action p= new Action(0x0F);
			this.Add(p);
		}
		public void And()
		{
			Action p= new Action(0x10);
			this.Add(p);
		}
		public void Or()
		{
			Action p= new Action(0x11);
			this.Add(p);
		}
		public void Not()
		{
			Action p= new Action(0x12);
			this.Add(p);
		}
		
		public void EndIf(){
			Action p= new Action(0x00);
			this.Add(p);
		}
		
		public Script Sum(Script A, Script B)
		{
			Script tmp = new Script();
			tmp._arr.AddRange(A._arr);
			tmp._arr.AddRange(B._arr);
			tmp.Add(new Action(0x0A));
			return tmp;
		}
		
		public void GetVar(){
			this.Add(new Action(0x1C));
		}
		
		public void Inc(){
			this.Add(new Action(0x50));
		}
		public void Stop(){
			this.Add(new Action(0x07));
		}
		public void Play(){
			this.Add(new Action(0x06));
		}
	
	}
	
	sealed class Action: ArrayData
	{	public int _code;
	
		public int JumpOffset=0;
		public bool Jump; // Si True se usa JumpOffset
		
		protected override void OnCompile()
		{	
			if(this.Jump) {
				this.Add(new UI16(this.JumpOffset));
			}	
			
			int length=this.Length;
			if(_code>=0x80){
				this.Insert(0, new UI16(length));
			}
			this.Insert(0, new UI8(this._code));
			
			base.OnCompile ();
		}

		public Action(int tipo)
		{
			this._code=tipo;
		}
		
	}	
}