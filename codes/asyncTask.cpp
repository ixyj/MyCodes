#include <iostream>
#include <future>
#include <chrono>

/**
async (launch policy, Fn&& fn, Args&&... args);
std::launch::async; // 立即启动异步线程
std::launch::deferred; // 线程延迟启动，当调用future.get或者future.wait时，才会创建异步线程并启动
std::launch::async | std::launch::deferred  // 缺省值。是否启动线程由系统负载/实现方式决定。如果系统负载过重，则可能不启动异步线程。

future_status wait_until (const chrono::time_point<Clock,Duration>& abs_time) const;    //Waits for the shared state to be ready, at most until abs_time.
future_status wait_for (const chrono::duration<Rep,Period>& rel_time) const; // Waits for the shared state to be ready for up to the time specified by rel_time.
future_status::ready	The shared state is ready: the producer has set a value or exception.
future_status::timeout	The function waited until abs_time without the shared state becoming ready.
future_status::deferred	The shared state contains a deferred function.

void wait() const;
R& future<R&>::get();
**/

int main(int argc, char** argv)
{
    std::future<bool> task = std::async([](int n)
    {
        for (auto i = 2; i < n; ++i)
        {
            if (n % i == 0)
            {
                return false;
            }
        }
        return true;
    }, 1234567890);

    auto status = task.wait_for(std::chrono::microseconds(100));
    std::cout << "status: " << (status == std::future_status::ready ? "ready" : "not ready") << std::endl;
    auto result = task.get();
    std::cout << "\1234567890 " << (result ? "is" : "is not") << " prime.\n";
    return 0;
}