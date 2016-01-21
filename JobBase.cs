using System;
using System.Threading;

namespace ILvYou.JobAgent
{
    /// <summary>
    /// 所有job的基类
    /// </summary>
    public abstract class JobBase
    {
        #region Field
        private JobResult jobResult;
        private JobStatus jobStatus;
        internal int ConfigId;
        internal DateTime StartTime;
        internal DateTime lastEndTime;
        private Thread thd;
        public string Param;
        #endregion

        #region Attr
        /// <summary>
        /// Job运行状态
        /// </summary>
        public JobStatus JobStatus
        {
            get
            {
                return this.jobStatus;
            }
            set
            {
                this.jobStatus = value;
                if (value == JobStatus.Stoped)
                {
                    this.lastEndTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Job运行结果
        /// </summary>
        public JobResult JobResult
        {
            get
            {
                return this.jobResult;
            }
            set
            {
                this.jobResult = value;
            }
        }
        #endregion

        #region APIs
        /// <summary>
        /// 运行JOB
        /// </summary>
        /// <param name="param"></param>
        internal virtual void Run(string param)
        {
            this.Param = param;
            this.thd = new Thread(new ThreadStart(this.Start));
            this.thd.Start();
        }

        internal virtual void Start()
        {
            try
            {
                this.lastEndTime = DateTime.MinValue;
                this.JobStatus = JobStatus.Running;
                this.OnStart(this.Param);
            }
            catch (Exception ex)
            {
                this.OnException(ex);
            }
            finally
            {
                this.JobStatus = JobStatus.Stoped;
            }
        }

        internal virtual void Stop()
        {
            this.JobStatus = JobStatus.Stopping;
            if (this.JobStatus != JobStatus.Stoped)
            {
                this.JobResult = JobResult.Abort;
                this.Abort();
                this.JobStatus = JobStatus.Stoped;
            }
        }

        public virtual void Abort()
        {
            if (this.thd != null)
            {
                try
                {
                    this.thd.Abort();
                }
                catch (Exception)
                {
                }
                finally
                {
                    this.OnAbort();
                }
            }
        }

        internal string GetJobInfo()
        {
            return base.GetType().FullName;
        }

        /// <summary>
        /// 当JOB被启动时运行，核心逻辑请在这里实现
        /// </summary>
        /// <param name="param"></param>
        protected abstract void OnStart(string param);

        /// <summary>
        /// 当JOB被强止中止时运行
        /// </summary>
        protected virtual void OnAbort()
        {
            this.JobResult = JobResult.Abort;
        }

        /// <summary>
        /// 当JOB出现异常时运行，默认动作为"抛出异常 throw ex"
        /// </summary>
        /// <param name="ex"></param>
        protected void OnException(Exception ex)
        {
            this.jobResult = JobResult.Exception;
        }
        #endregion
    }
}