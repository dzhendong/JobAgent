using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ILvYou.JobAgent
{
    #region JobResult
    public enum JobResult
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 失败
        /// </summary>
        Fail,

        /// <summary>
        /// 异常
        /// </summary>
        Exception,

        /// <summary>
        /// 终止
        /// </summary>
        Abort
    }
    #endregion

    #region JobStatus
    public enum JobStatus
    {
        /// <summary>
        /// 停止
        /// </summary>
        Stop,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 停止中
        /// </summary>
        Stopping,

        /// <summary>
        /// 已停止
        /// </summary>
        Stoped
    }
    #endregion
}