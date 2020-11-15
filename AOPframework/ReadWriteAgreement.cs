using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AOPframework
{
    public class ReadWriteAgreement<T>
    {
        /// <summary>
        /// 0正常 ，1写，-1读
        /// </summary>
        private volatile int state;

        private IList<T> source;

        private IList<Task> queueW = new List<Task>();
        private IList<Task> queueR = new List<Task>();
        public ReadWriteAgreement(IList<T> source)
        {
            this.source = source; 
            Task.Run(()=>{ door(); });
        }

        private  void  door() {
            while (true)
            {
                if (state == 1)
                {
                    Task.WaitAll(queueW.ToArray());
                    state = 0;
                    queueW.Clear();
                } else if (state == -1) {
                    Task.WaitAll(queueR.ToArray());
                    state = 0;
                    queueR.Clear();
                }
            }
        }



        public IList<T> Read(Func<IEnumerable<T>,IList<T>> func)
        {
            state = -1;
            var task = Task.Run(() =>
             {
                 return func(source);
             });
            queueR.Add(task);
            return task.Result;
        }
        public void Write(Action<ICollection<T>> func)
        {
               state = 1;
               queueW.Add(Task.Run(() =>
                {
                    
                    func(source);
                    
                }));
        }
    }


}
