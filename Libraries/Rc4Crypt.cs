/*
 * 由SharpDevelop创建。
 * 用户： YISH
 * 日期: 04/04/2015
 * 时间: 03:01
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;

namespace CampusNetGuard.Code.Libraries
{
	/// <summary>
	/// Description of CryptoGraphy.
	/// </summary>
	public class RC4Crypt:IDisposable{
		byte[] S;
		byte[] T;
        byte[] K;
		byte[] k;
        public RC4Crypt() { }
		public RC4Crypt(byte[] key){
			this.K=key;
		}
        public byte[] Key
        {
            get
            {
                return K;
            }
            set
            {
                K = value;
            }
        }
		//初始化状态向量S和临时向量T，供keyStream方法调用
		void initial(){
            if (S == null || T == null)
            {
                S = new byte[256];
                T = new byte[256];
            }
			for (int i = 0; i < 256; ++i) {
				S[i]=(byte)i;
                T[i] = K[i % K.Length];
			}
		}
		//初始排列状态向量S，供keyStream方法调用
		void ranges(){
			int j=0;
			for (int i = 0; i < 256; ++i) {
				j=(j+S[i]+T[i])&0xff;
				S[i]=(byte)((S[i]+S[j])&0xff);
				S[j]=(byte)((S[i]-S[j])&0xff);
				S[i]=(byte)((S[i]-S[j])&0xff);
			}
		}
		//生成密钥流
		//len:明文为len个字节
		void keyStream(int len){
			initial();
			ranges();
			int i=0,j=0,t=0;
			k=new byte[len];
			for (int r = 0; r < len; r++) {
				i=(i+1)&0xff;
				j=(j+S[i])&0xff;
				
				S[i]=(byte)((S[i]+S[j])&0xff);
				S[j]=(byte)((S[i]-S[j])&0xff);
				S[i]=(byte)((S[i]-S[j])&0xff);
				
				t=(S[i]+S[j])&0xff;
				k[r]=S[t];
			}
		}
		
		public byte[] EncryptByte(byte[] data){
			//生产密匙流
			keyStream(data.Length);
			for (int i = 0; i < data.Length; i++) {
				k[i]=(byte)(data[i]^k[i]);
			}
			return k;
		}

		public byte[] DecryptByte(byte[] data){
			return EncryptByte(data);
		}

        //是否回收完毕
        bool _disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~RC4Crypt()
        {
            Dispose(false);
        }
        //这里的参数表示示是否需要释放那些实现IDisposable接口的托管对象
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;//如果已经被回收，就中断执行
            if (disposing)
            {
                //TODO:释放那些实现IDisposable接口的托管对象

            }
            //TODO:释放非托管资源，设置对象为null
            S = null;
            T = null;
            K = null;
            k = null;
            _disposed = true;
        }
    }
}
