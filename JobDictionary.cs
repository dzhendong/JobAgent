using System.Collections.Generic;
using System.Threading;

namespace ILvYou.JobAgent
{
    public class JobDictionary
    {
        #region signal
        private Dictionary<string, JobBaseList> Jobs = new Dictionary<string, JobBaseList>();
        private ManualResetEvent write = new ManualResetEvent(true);

        private ManualResetEvent[] reads = new ManualResetEvent[]
			{
				new ManualResetEvent(true),
				new ManualResetEvent(true)
			};
        #endregion

        #region APIs
        public JobBaseList this[string Key]
        {
            get
            {
                //1:write信号阻止,没有调用write.Set之前
                //2:reads[1]回滚到默认的初始false阻塞线程状态
                //3:reads[1]释放线程阻塞
                this.write.WaitOne();
                this.reads[1].Reset();
                JobBaseList one = this.Jobs[Key];
                this.reads[1].Set();
                return one;
            }
        }

        public void Add(string Key, JobBaseList Value)
        {
            WaitHandle.WaitAll(this.reads);
            this.write.Reset();
            this.Jobs.Add(Key, Value);
            this.write.Set();
        }

        public bool ContainsKey(string Key)
        {
            this.write.WaitOne();
            this.reads[0].Reset();
            bool have = this.Jobs.ContainsKey(Key);
            this.reads[0].Set();
            return have;
        }
        #endregion
    }
}