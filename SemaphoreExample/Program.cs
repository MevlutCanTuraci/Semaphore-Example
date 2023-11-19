using ThreadingTimer = System.Threading.Timer;
using Timer = System.Timers.Timer;


namespace SemaphoreExample
{
    public class Program
    {
        //SemaphoreSlim(<Slot-Count>, <Thread-Count>)
        /*
            Slot (initialCount); Semaphore'i kaç thread kullanabileceğini belirler. 
            Örnek; 1 verildiyse ve 2 thread varsa, birisi Semaphore'i birinci thread kullanırken,
            ikinci thread, birinci thread'in işleminin bitmesini bekler sonra işleme başlar.

            Thread-Count (maxCount): Bu Semaphore'i kaç thread kullanabilir olduğunu belirler. 
            Örnek; 5 değeri verilmişse sadece 5 thread bu Semaphore nesnesini kullanabilir.
            
         */

        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        static void Main(string[] args)
        {
            Example1();
            //Example2();
            //Example3();

            Console.ReadLine();
        }



        #region Example 1

        private static ThreadingTimer _tt;
        private static object _ttLock = new object();

        static void Example1()
        {
            try
            {
                lock (_ttLock)
                {
                    if (_tt != null)
                    {
                        _tt.Dispose();
                        _tt = null;
                    }

                    _tt = new ThreadingTimer(async (_) =>
                    {
                        if (_semaphoreSlim.CurrentCount <= 0)
                        {
                            Console.WriteLine($"Do not this! (Example1)");
                            return;
                        }

                        try
                        {
                           await _semaphoreSlim.WaitAsync();

                            Console.WriteLine("Working...");

                            int i = 0;
                            while (i <= 350)
                            {
                                await Task.Delay(10);
                                i++;
                            }

                            Console.WriteLine("Working finished (Example1)\r\n\n\n");
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }

                    }, null, 0, 300);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error! {ex.Message}");
            }
        }


        #endregion



        #region Example 2

        // Başlangıçta 2 izin ver, toplamda 5 izne kadar izin ver.
        static SemaphoreSlim _semaphore = new SemaphoreSlim(2, 5);

        static void Example2()
        {
            // 5 farklı task oluştur ve bunları paralel olarak çalıştır.
            Task[] tasks = new Task[5];
            for (int i = 0; i < 5; i++)
            {
                int taskId = i + 1;
                tasks[i] = Task.Run(() => AccessResource(taskId));
            }

            // Tüm task'ların tamamlanmasını bekleyelim.
            Task.WaitAll(tasks);
        }

        static void AccessResource(int taskId)
        {
            Console.WriteLine($"Task {taskId} çalışmaya başladı.");

            try
            {
                _semaphore.Wait(); // Semafora giriş izni iste

                Console.WriteLine($"Task {taskId} kritik bölgeye girdi ve işlem yapıyor.");

                // Simüle edilen işlem: 1 saniye boyunca bekleyelim
                Thread.Sleep(1000);

                Console.WriteLine($"Task {taskId} kritik bölgeden çıkıyor.");
            }
            finally
            {
                _semaphore.Release(); // Semafora çıkış iznini serbest bırak
            }
        }

        #endregion



        #region Example 3

        private static Timer _timer;
        private static object _timerLock = new object();

        static void Example3()
        {
            try
            {
                lock (_timerLock)
                {
                    if (_timer != null)
                    {
                        _timer.Dispose();
                        _timer = null;
                    }

                    _timer = new Timer(300);
                    _timer.Elapsed += async (sender, e) =>
                    {
                        lock (_timerLock)
                        {
                            if (_semaphoreSlim.CurrentCount <= 0)
                            {
                                Console.WriteLine("Do not do this! (Example3)");
                                return;
                            }

                            try
                            {
                                _semaphoreSlim.Wait();

                                Console.WriteLine("Working... (Example3)");

                                int i = 0;
                                while (i <= 400)
                                {
                                    Task.Delay(10);
                                    i++;
                                }

                                Console.WriteLine("Working finished (Example3)\r\n\n");
                            }
                            finally
                            {
                                _semaphoreSlim.Release();
                            }
                        }
                    };

                    _timer.AutoReset = true;
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error! {ex.Message}");
            }
        }


        #endregion


    }
}