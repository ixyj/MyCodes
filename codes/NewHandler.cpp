#include <iostream>

template <class T>
class MyNewHandler
{
public:
    static new_handler set_new_handler(new_handler p);
    static void* operator new(size_t size);

private:
    static new_handler currentHandler;
};

template <class T>
new_handler MyNewHandler<T>::currentHandler = nullptr;

template <class T>
new_handler MyNewHandler<T>::set_new_handler(new_handler p)
{
    auto oldHandler = currentHandler;
    currentHandler = p;
    return  oldHandler;
}

template <class T>
void* MyNewHandler<T>::operator new(size_t size)
{
    auto oldHandler = std::set_new_handler(currentHandler);

    void* memory;
    try
    {
        memory = ::operator new(size);
    }
    catch (std::bad_alloc&)
    {
        std::set_new_handler(oldHandler);
        throw;
    }

    std::set_new_handler(oldHandler);
    return memory;
}

class Test : public MyNewHandler<Test>
{
public:
    static void Handler(){ std::cerr << "Test My Handler!\n"; }
};

int main(int argc, char** argv)
{
    Test::set_new_handler(Test::Handler);
    void* ptr1 = Test::operator new(1024 * 1024 * 1024);
    delete ptr1;

    Test* ptr2 = ::new(std::nothrow)Test;
    if (ptr2 == nullptr)
    {
        std::cerr << "failed to new!\n";
    }

    return 0;
}

