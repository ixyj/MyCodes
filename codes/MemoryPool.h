#pragma once
#include <vector>

/////////////////////////////////////////////////////////////////////////////////

template <typename T>
class MemoryPool
{
public:
    MemoryPool()
    {
        _freeList.reserve(1024);
    }

    ~MemoryPool()
    {
        for (auto p = _freeList.begin(); p != _freeList.end(); ++p)
            delete *p;
    }

    T* New()
    {
        if (!_freeList.empty())
        {
            T *result = _freeList.back();
            _freeList.pop_back();
            result->Reset();
            return result;
        }

        return new T();
    }

    template <typename A1>
    T* New(A1 a1)
    {
        if (!_freeList.empty())
        {
            T *result = _freeList.back();
            _freeList.pop_back();
            result->Reset(a1);
            return result;
        }

        return new T(a1);
    }

    template <typename A1, typename A2>
    T* New(A1 a1, A2 a2)
    {
        if (!_freeList.empty())
        {
            T *result = _freeList.back();
            _freeList.pop_back();
            result->Reset(a1, a2);
            return result;
        }

        return new T(a1, a2);
    }

    template <typename A1, typename A2, typename A3>
    T* New(A1 a1, A2 a2, A3 a3)
    {
        if (!_freeList.empty())
        {
            T *result = _freeList.back();
            _freeList.pop_back();
            result->Reset(a1, a2, a3);
            return result;
        }

        return new T(a1, a2, a3);
    }

    template <typename A1, typename A2, typename A3, typename A4>
    T* New(A1 a1, A2 a2, A3 a3, A4 a4)
    {
        if (!_freeList.empty())
        {
            T *result = _freeList.back();
            _freeList.pop_back();
            result->Reset(a1, a2, a3, a4);
            return result;
        }

        return new T(a1, a2, a3, a4);
    }

    void Delete(T* obj)
    {
        obj->Clear();
        _freeList.push_back(obj);
    }

    size_t GetFreeCount(void) const
	{
		return _freeList.size();
	}

protected:
     std::vector<T*> _freeList;
};
