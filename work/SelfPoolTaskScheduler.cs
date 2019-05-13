namespace SelfPoolTaskScheduler
{
    using System;
    using System.Linq;

    public class SelfPoolTaskScheduler : TaskScheduler
    {
        public static readonly SelfPoolTaskScheduler Default = new SelfPoolTaskScheduler(Environment.ProcessorCount << 2);
        private static readonly ManualResetEventSlim _locker = new ManualResetEventSlim(false);
        private static ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();
        private static Thread[] _threads;

        public SelfPoolTaskScheduler(int poolSize)
        {
            if (poolSize <= 0) throw new ArgumentOutOfRangeException("invalid poolSize");
            _threads = new Thread[poolSize];
            for (var i = 0; i < poolSize; ++i)
            {
                _threads[i] = new Thread(DispatchLoop) { IsBackground = true };
                _threads[i].Start();
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks;
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Enqueue(task);
            if (!_locker.IsSet) _locker.Set();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return TryExecuteTask(task);
        }

        private void DispatchLoop()
        {
            while (true)
            {
                _locker.Wait();
                if (_tasks.TryDequeue(out Task task))
                {
                    TryExecuteTask(task);
                }
                else
                {
                    _locker.Reset();
                }
            }
        }
    }}
